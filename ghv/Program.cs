//using System;
//using System.Net.Http;
//using System.Text.Json;
//using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Spectre.Console;
using System.Text.Json.Nodes;

namespace ghv
{
    class Program
    {
        private static async Task GetContributorsAsync(string owner, string repo, int topN)
        {
            using HttpClient client = new();
            // GitHub API URL 获取贡献者信息
            string url = $"https://api.github.com/repos/{owner}/{repo}/contributors?per_page={topN}";

            // 设置 GitHub 的 User-Agent
            client.DefaultRequestHeaders.Add("User-Agent", "CSharpApp");

            // 获取响应
            HttpResponseMessage response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();

                // 使用 System.Text.Json 解析 JSON 响应
                JsonNode jsonNode = JsonNode.Parse(content);

                // 将 JsonNode 转换为 JsonArray
                JsonArray contributors = jsonNode.AsArray();

                // 使用 Spectre.Console 输出表格
                var table = new Table();
                table.AddColumn("排名");
                table.AddColumn("贡献者");
                table.AddColumn("贡献计数");

                // 输出贡献者排名
                int rank = 1;
                foreach (JsonNode contributor in contributors)
                {
                    // 将 JsonNode 转换为 JsonObject 来获取属性
                    JsonObject contributorObject = contributor.AsObject();

                    // 使用 GetPropertyValue 获取字段值
                    string login = contributorObject["login"].GetValue<string>();
                    int contributions = contributorObject["contributions"].GetValue<int>();

                    // 向表格添加一行
                    table.AddRow(rank.ToString(), login, contributions.ToString());

                    rank++;
                }

                // 使用 Write 来输出表格（替代 Render）
                AnsiConsole.Write(table);
            }
            else
            {
                AnsiConsole.Markup("[red]获取贡献者数据失败。[/]");
            }
        }

        static async Task Main(string[] args)
        {
            // 获取用户输入
            string owner = GetUserInput("请输入仓库所有者名称:", ValidateOwnerOrRepo);
            string repo = GetUserInput("请输入仓库名称:", ValidateOwnerOrRepo);
            int topN = GetTopNInput("请输入要获取前几个贡献者信息:");

            // 验证仓库是否存在
            bool repositoryExists = await ValidateRepositoryExists(owner, repo);
            if (!repositoryExists)
            {
                AnsiConsole.Markup("[red]该仓库不存在，请检查输入的所有者和仓库名是否正确。[/]\n");
                return;
            }

            // 调用方法获取贡献者排名
            await GetContributorsAsync(owner, repo, topN);
        }

        // 获取用户输入的通用方法，问题前面加蓝色 "?"
        private static string GetUserInput(string prompt, Func<string, bool> validator)
        {
            string input;
            do
            {
                // 蓝色输出询问，不换行
                AnsiConsole.Markup("[cyan]? [/]" + prompt + " "); // 使用 Spectre.Console 的颜色

                // 获取输入
                input = Console.ReadLine()?.Trim();

                if (!validator(input))
                {
                    // 红色输出错误信息
                    AnsiConsole.Markup("[red]输入无效，请重新输入。[/]\n");
                }
            }
            while (string.IsNullOrWhiteSpace(input) || !validator(input));

            return input;
        }

        // 获取并验证 topN 输入
        private static int GetTopNInput(string prompt)
        {
            int topN;
            do
            {
                // 蓝色输出询问，不换行
                AnsiConsole.Markup("[cyan]? [/]" + prompt + " "); // 使用 Spectre.Console 的颜色

                // 获取并验证输入
                string input = Console.ReadLine()?.Trim();

                // 判断是否是有效的正整数
                if (int.TryParse(input, out topN) && topN >= 1)
                {
                    break;
                }

                // 如果输入无效，输出错误提示
                AnsiConsole.Markup("[red]请输入一个大于等于 1 的正整数。[/]\n");

            }
            while (true);

            return topN;
        }

        // 验证 owner 和 repo 的输入
        private static bool ValidateOwnerOrRepo(string input)
        {
            // 输入不能为空
            if (string.IsNullOrWhiteSpace(input)) return false;

            string pattern = @"^[a-zA-Z0-9_-]+(?:[._][a-zA-Z0-9_-]+)*$";
            return !input.Contains('/') && Regex.IsMatch(input, pattern);
        }

        // 验证仓库是否存在
        private static async Task<bool> ValidateRepositoryExists(string owner, string repo)
        {
            using HttpClient client = new();
            // GitHub API URL 验证仓库存在性
            string url = $"https://api.github.com/repos/{owner}/{repo}";

            // 设置 GitHub 的 User-Agent
            client.DefaultRequestHeaders.Add("User-Agent", "CSharpApp");

            // 获取响应
            HttpResponseMessage response = await client.GetAsync(url);

            // 如果 HTTP 响应状态码是 200 (OK)，说明仓库存在
            return response.IsSuccessStatusCode;
        }
    }
}
