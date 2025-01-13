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
            using HttpClient client = new();
            string url = $"https://api.github.com/repos/{owner}/{repo}";
            client.DefaultRequestHeaders.Add("User-Agent", "CSharpApp");
            HttpResponseMessage response = await client.GetAsync(url);
            return response.IsSuccessStatusCode;
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
                    var linkHeader = response.Headers.Contains("Link") ? response.Headers.GetValues("Link").FirstOrDefault() : null;
                    nextUrl = GetNextPageUrlFromLinkHeader(linkHeader); // 只需要处理分页 URL
                }
                else
                {
                    AnsiConsole.Markup("[red]无法获取标签，请稍后再试。[/]\n");
                    return;
                }
            }

            if (allLabels.Count > 0)
            {
                var table = new Table();
                table.Border(TableBorder.Rounded);
                table.BorderColor(Color.Grey);
                table.AddColumn(new TableColumn("标签名称").Centered());
                table.AddColumn(new TableColumn("颜色").Centered());
                table.AddColumn(new TableColumn("描述").Centered());

                foreach (var label in allLabels)
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

            var regex = new Regex(@"<([^>]+)>;\s*rel=""next""");
            var match = regex.Match(linkHeader);

            if (match.Success)
            {
                return match.Groups[1].Value; // 返回匹配的 URL 部分
            }

            return null;
        }
    }
}
