using Spectre.Console;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace ghv.Command
{
    internal class Labels
    {
        public static async Task ExecuteAsync()
        {
            string owner = GetUserInput("请输入仓库所有者名称:", ValidateOwnerOrRepo);
            string repo = GetUserInput("请输入仓库名称:", ValidateOwnerOrRepo);

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
            string url = $"https://api.github.com/repos/{owner}/{repo}/labels";
            client.DefaultRequestHeaders.Add("User-Agent", "CSharpApp");
            HttpResponseMessage response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();
                JsonNode? jsonNode = JsonNode.Parse(jsonResponse);
                if (jsonNode == null || jsonNode.AsArray() == null)
                {
                    AnsiConsole.Markup("[red]无法解析标签数据。[/]\n");
                    return;
                }
                JsonArray labels = jsonNode.AsArray();

                if (labels.Count > 0)
                {
                    var table = new Table();
                    table.Border(TableBorder.Rounded);
                    table.BorderColor(Color.Grey);
                    table.AddColumn(new TableColumn("标签名称").Centered());
                    table.AddColumn(new TableColumn("颜色").Centered());
                    table.AddColumn(new TableColumn("描述").Centered());

                    foreach (var label in labels)
                    {
                        string labelName = label["name"]?.ToString() ?? string.Empty;
                        string labelColor = label["color"]?.ToString() ?? "000000";
                        string labelDescription = label["description"]?.ToString() ?? string.Empty;

                        table.AddRow(
                            $"[#{labelColor}]{labelName}[/]",
                            $"[#{labelColor}]{labelColor}[/]",
                            $"[#{labelColor}]{labelDescription}[/]"
                        ).Border(TableBorder.Square);
                    }

                    table.LeftAligned();

                    AnsiConsole.Write(table);
                }
                else
                {
                    AnsiConsole.Markup("[yellow]该仓库没有标签。[/]\n");
                }
            }
            else
            {
                AnsiConsole.Markup("[red]无法获取标签，请稍后再试。[/]\n");
            }
        }
    }
}
