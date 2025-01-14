using Spectre.Console;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace ghv.Command
{
    internal class Contribute
    {
        public static async Task ExecuteAsync(string owner, string repo, int topN)
        {
            if (string.IsNullOrWhiteSpace(owner))
            {
                owner = GetUserInput("请输入仓库所有者名称:", ValidateOwnerOrRepo);
            }

            if (string.IsNullOrWhiteSpace(repo))
            {
                repo = GetUserInput("请输入仓库名称:", ValidateOwnerOrRepo);
            }

            if (topN < 1)
            {
                topN = GetTopNInput("请输入要获取前几个贡献者信息:");
            }

            bool repositoryExists = await ValidateRepositoryExists(owner, repo);
            if (!repositoryExists)
            {
                return;
            }

            await GetContributorsAsync(owner, repo, topN);
        }

        private static async Task GetContributorsAsync(string owner, string repo, int topN)
        {
            using HttpClient client = new();
            client.DefaultRequestHeaders.Add("User-Agent", "CSharpApp");
            int pages = (int)Math.Ceiling((double)topN / 100); // 提前声明总页数，每页个数在后面计算
            try
            {
                if (topN <= 100) // 100个以内不需要分页
                {
                    pages = 1; // 1 页就够 - 不用再请求总页数
                }
                else // 100个以上则先定义每页100个以减少页数，进而减少请求次数
                {
                    HttpResponseMessage response = await client.GetAsync("https://api.github.com/repos/{owner}/{repo}/contributors"); // 先请求一下获取总页数
                    if (response.IsSuccessStatusCode)
                    {
                        string? linkHeader = response.Headers.Contains("Link") ? response.Headers.GetValues("Link").FirstOrDefault() : null;
                        if (linkHeader == null)
                        {
                            // 解析失败
                            AnsiConsole.Markup("[yellow]无法解析总分页数: Link 头为 null[/]\n");
                        }
                        else
                        {
                            Regex regex = new(@"<([^>]+)>;\s*rel=""last""");
                            Match match = regex.Match(linkHeader);
                            string allPagesString = match.Groups[1].Value; // 返回匹配的 URL 部分

                            if (match.Success)
                            {
                                if (int.TryParse(allPagesString, out int allPages))
                                {
                                    // 成功解析，allPages 现在是整数
                                    /*
                                        首先将 topN 强制转换为 double 类型，以确保执行除法时获得浮点数。
                                        然后通过 Math.Ceiling() 向上取整。
                                        最后将结果转换回 int 类型获得最终需要请求的总页数。
                                    */
                                    if (pages > allPages)
                                    {
                                        pages = allPages; // 再多也不能超出总页数呀
                                    }
                                }
                                else
                                {
                                    // 解析失败
                                    AnsiConsole.Markup("[yellow]无法解析总分页数: 匹配失败[/]\n");
                                }
                            }
                        }
                    }
                }
            }
            catch (HttpRequestException e)
            {
                AnsiConsole.Markup($"[yellow]无法解析总分页数: 请求失败[/]\n[yellow]信息:{e.Message}[/]");
                // 直接用最开始算的，后面还有错再报，这里用警告样式
            }

#if DEBUG
            AnsiConsole.Markup($"[purple][[DEBUG]] 请求分页数: {pages}[/]\n");
#endif

            List<JsonNode> allContributors = [];
            string url;

            for (int i = 1; i <= pages; i++)
            {
                // 计算此页个数
                if ((i == pages) && (topN % 100 != 0)) // 如果是最后一页
                {
                    url = $"https://api.github.com/repos/{owner}/{repo}/contributors?per_page={topN % 100}&page={i}";
                }
                else
                {
                    url = $"https://api.github.com/repos/{owner}/{repo}/contributors?per_page={100}&page={i}";
                }

#if DEBUG
                AnsiConsole.Markup($"[purple][[DEBUG]] 请求: {url}[/]\n");
#endif

                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        JsonNode? jsonNode = JsonNode.Parse(jsonResponse);
                        if (jsonNode == null || jsonNode.AsArray() == null)
                        {
                            AnsiConsole.Markup("[red]无法解析贡献者数据: 请求成功，但返回 null[/]\n");
                            return;
                        }

                        // 合并获取到的贡献者
                        allContributors.AddRange(jsonNode.AsArray().Cast<JsonNode>());
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        string? resetTime = response.Headers.Contains("X-RateLimit-Reset")
                            ? response.Headers.GetValues("X-RateLimit-Reset").FirstOrDefault() // 获取第一个值
                            : "未知";

                        AnsiConsole.Markup($"[yellow]你 太 快 了 ！[/]\n[yellow]您的请求已达到 [link=https://docs.github.com/zh/rest/using-the-rest-api/rate-limits-for-the-rest-api]GitHub API 速率限制[/][/]\n");
                        if ((resetTime != null) && (resetTime != "未知"))
                        {
                            // 等待速率限制重置
#if DEBUG
                            AnsiConsole.Markup($"[purple][[DEBUG]] 速率重置时间戳: {Markup.Escape(resetTime)}[/]\n");
#endif
                            long resetTimestamp = long.Parse(resetTime);
                            DateTime resetDateTimeUtc = DateTimeOffset.FromUnixTimeSeconds(resetTimestamp).DateTime;
                            // 将 UTC 时间转换为本地时间
                            DateTime resetDateTimeLocal = resetDateTimeUtc.ToLocalTime();
#if DEBUG
                            AnsiConsole.Markup($"[purple][[DEBUG]] 转化后速率重置时间: {resetDateTimeLocal}[/]\n");
#endif
                            TimeSpan waitTime = resetDateTimeLocal - DateTime.Now;
                            AnsiConsole.Markup($"[yellow]重置时间: {resetDateTimeLocal} (需要等待 {waitTime} )[/]");
                            Thread.Sleep(waitTime); // 等待重置
                            i--; // 本次请求没成功，后面重试时再请求这页
                        }
                        else
                        {
                            AnsiConsole.Markup("[red]无法获取贡献数据: 请求达到速率限制，但无法获取重置时间戳: X-RateLimit-Reset 头为 null 或无法转为 String 类型[/]\n");
                            return;
                        }
                    }
                    else
                    {

                        AnsiConsole.Markup("[red]无法获取贡献数据: 请求失败: 未捕获异常[/]\n");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    // 捕获网络或其他错误
                    AnsiConsole.Markup($"[red]无法获取贡献数据: 请求发生异常: {Markup.Escape(ex.Message)}[/]\n");
                }

                Thread.Sleep(1000); // 请求间间隔 1 秒 - https://docs.github.com/zh/rest/using-the-rest-api/best-practices-for-using-the-rest-api?apiVersion=2022-11-28#handle-rate-limit-errors-appropriately
            }

            if (allContributors.Count > 0)
            {
                Table table = new();
                table.AddColumn("排名");
                table.AddColumn("贡献者");
                table.AddColumn("贡献计数");

                int rank = 1;
                foreach (JsonNode contributor in allContributors)
                {
                    string login = Markup.Escape(contributor["login"]?.ToString() ?? string.Empty);
                    int contributions = contributor["contributions"]?.GetValue<int>() ?? 0;
                    table.AddRow(rank.ToString(), $"[link=https://github.com/{login}]{login}[/]", contributions.ToString());
                    rank++;
                }

                AnsiConsole.Write(table);
            }
            else
            {
                AnsiConsole.Markup("[red]该仓库没有贡献者 (如果仓库为空则可能出现此情况)。[/]");
            }
        }

        private static string GetUserInput(string prompt, Func<string, bool> validator)
        {
            string input;
            do
            {
                AnsiConsole.Markup("[cyan]? [/]" + prompt + " ");
                input = (Console.ReadLine() ?? string.Empty).Trim();

                if (!validator(input))
                {
                    AnsiConsole.Markup("[red]输入无效，请重新输入。[/]\n");
                }
            }
            while (string.IsNullOrWhiteSpace(input) || !validator(input));

            return input;
        }

        private static int GetTopNInput(string prompt)
        {
            int topN;
            do
            {
                AnsiConsole.Markup("[cyan]? [/]" + prompt + " ");
                string input = (Console.ReadLine() ?? string.Empty).Trim();

                // 尝试解析输入为 int 类型
                if (int.TryParse(input, out topN))
                {
                    // 检查输入的数字是否在 int 类型的有效范围内
                    if (topN >= 1)
                    {
                        break;
                    }
                    else
                    {
                        AnsiConsole.Markup("[red]请输入一个大于等于 1 的正整数。[/]\n");
                    }
                }
                else
                {
                    // 如果 int.TryParse 失败，说明输入不符合 int 类型的格式
                    // 检查是否超出 int 范围
                    if (input.Length > 10 || long.TryParse(input, out long largeInput) && largeInput > int.MaxValue)
                    {
                        AnsiConsole.Markup("[red]太多了！int 类型最大为 2,147,483,647[/]\n");
                    }
                    else
                    {
                        // 普通的格式错误提示
                        AnsiConsole.Markup("[red]请输入一个有效的正整数。[/]\n");
                    }
                }
            }
            while (true);

            return topN;
        }

        private static bool ValidateOwnerOrRepo(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            string pattern = @"^[a-zA-Z0-9_-]+(?:[._][a-zA-Z0-9_-]+)*$";
            return !input.Contains('/') && Regex.IsMatch(input, pattern);
        }

        private static async Task<bool> ValidateRepositoryExists(string owner, string repo)
        {
            try
            {
                using HttpClient client = new();
                string url = $"https://api.github.com/repos/{owner}/{repo}";
                client.DefaultRequestHeaders.Add("User-Agent", "CSharpApp");
                HttpResponseMessage response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    // 仓库不存在的提示
                    AnsiConsole.Markup("[red]仓库不存在(或没有访问权限)，请检查输入的所有者和仓库名是否正确。[/]\n");
                    return false;
                }
                return true;
            }
            catch (HttpRequestException e)
            {
                // 处理速率限制或其他访问错误
                AnsiConsole.Markup($"[red]无法验证仓库是否存在: 请求错误[/]\n[red]您的请求可能已达到 [link=https://docs.github.com/zh/rest/using-the-rest-api/rate-limits-for-the-rest-api]GitHub API 速率限制[/][/]\n[red]详细错误信息: {e}[/]\n");
                return false;
            }
            catch (Exception ex)
            {
                // 其他异常处理
                AnsiConsole.Markup($"[red]无法验证仓库是否存在: {ex.Message}[/]\n");
                return false;
            }
        }
    }
}
