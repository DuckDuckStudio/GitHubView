using Spectre.Console;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace ghv.Command
{
    internal class Labels
    {
        public static async Task ExecuteAsync(string owner, string repo)
        {
            if (string.IsNullOrWhiteSpace(owner))
            {
                owner = GetUserInput("请输入仓库所有者名称:", ValidateOwnerOrRepo);
            }

            if (string.IsNullOrWhiteSpace(repo))
            {
                repo = GetUserInput("请输入仓库名称:", ValidateOwnerOrRepo);
            }

            bool repositoryExists = await ValidateRepositoryExists(owner, repo);
            if (!repositoryExists)
            {
                AnsiConsole.Markup("[red]该仓库不存在，请检查输入的所有者和仓库名是否正确。[/]\n");
                return;
            }

            await FetchAndDisplayLabels(owner, repo);
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

        private static async Task FetchAndDisplayLabels(string owner, string repo)
        {
            using HttpClient client = new();
            string baseUrl = "https://api.github.com/repos";
            string url = $"{baseUrl}/{owner}/{repo}/labels?per_page=100"; // 设置 per_page=100 以减少分页 - https://docs.github.com/zh/rest/issues/labels#list-labels-for-a-repository
            client.DefaultRequestHeaders.Add("User-Agent", "CSharpApp");

            List<JsonNode> allLabels = [];
            string? nextUrl = url;

            while (nextUrl != null)
            {
                HttpResponseMessage response = await client.GetAsync(nextUrl);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    JsonNode? jsonNode = JsonNode.Parse(jsonResponse);
                    if (jsonNode == null || jsonNode.AsArray() == null)
                    {
                        AnsiConsole.Markup("[red]无法解析标签数据。[/]\n");
                        return;
                    }

                    // 合并获取到的标签
                    allLabels.AddRange(jsonNode.AsArray().Cast<JsonNode>());

                    // 检查 Link 头以获取分页信息
                    string? linkHeader = response.Headers.Contains("Link") ? response.Headers.GetValues("Link").FirstOrDefault() : null;
                    nextUrl = GetNextPageUrlFromLinkHeader(linkHeader); // 只需要处理分页 URL
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
                        // 本次请求没成功，后面重试时再请求这页 - 只有请求成功才会获取下一个分页 url
                    }
                    else
                    {
                        AnsiConsole.Markup("[red]无法获取标签数据: 请求达到速率限制，但无法获取重置时间戳: X-RateLimit-Reset 头为 null 或无法转为 String 类型[/]\n");
                        return;
                    }
                }
                else
                {
                    AnsiConsole.Markup("[red]无法获取标签，请稍后再试。[/]\n");
                    return;
                }

                Thread.Sleep(1000); // 请求间间隔 1 秒 - https://docs.github.com/zh/rest/using-the-rest-api/best-practices-for-using-the-rest-api?apiVersion=2022-11-28#handle-rate-limit-errors-appropriately
            }

            if (allLabels.Count > 0)
            {
                Table table = new();
                table.Border(TableBorder.Rounded);
                table.BorderColor(Color.Grey);
                table.AddColumn(new TableColumn("标签名称").Centered());
                table.AddColumn(new TableColumn("颜色").Centered());
                table.AddColumn(new TableColumn("描述").Centered());

                foreach (JsonNode label in allLabels)
                {
                    string labelName = Markup.Escape(label["name"]?.ToString() ?? string.Empty);
                    string labelColor = label["color"]?.ToString() ?? string.Empty;
                    string labelDescription = Markup.Escape(label["description"]?.ToString() ?? string.Empty);

                    table.AddRow(
                        $"[#{labelColor}]{labelName}[/]",
                        $"[#{labelColor}]{labelColor}[/]",
                        $"[#{labelColor}]{labelDescription}[/]"
                    ).Border(TableBorder.Square);
                }

                    table.LeftAligned();
                    table.LeftAligned();

                table.LeftAligned();

                AnsiConsole.Write(table);
            }
            else
            {
                AnsiConsole.Markup("[yellow]该仓库没有标签。[/]\n");
            }
        }

        // 解析 Link 头，获取下一页的 URL
        private static string? GetNextPageUrlFromLinkHeader(string? linkHeader)
        {
            if (string.IsNullOrEmpty(linkHeader)) return null;

            Regex regex = new(@"<([^>]+)>;\s*rel=""next""");
            Match match = regex.Match(linkHeader);

            if (match.Success)
            {
                return match.Groups[1].Value; // 返回匹配的 URL 部分
            }

            return null;
        }
    }
}
