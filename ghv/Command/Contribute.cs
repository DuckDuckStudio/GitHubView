using Spectre.Console;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace ghv.Command
{
    internal class Contribute
    {
        public static async Task ExecuteAsync()
        {
            string owner = GetUserInput("请输入仓库所有者名称:", ValidateOwnerOrRepo);
            string repo = GetUserInput("请输入仓库名称:", ValidateOwnerOrRepo);
            int topN = GetTopNInput("请输入要获取前几个贡献者信息:");

            bool repositoryExists = await ValidateRepositoryExists(owner, repo);
            if (!repositoryExists)
            {
                AnsiConsole.Markup("[red]该仓库不存在，请检查输入的所有者和仓库名是否正确。[/]\n");
                return;
            }

            await GetContributorsAsync(owner, repo, topN);
        }

        private static async Task GetContributorsAsync(string owner, string repo, int topN)
        {
            using HttpClient client = new();
            string url = $"https://api.github.com/repos/{owner}/{repo}/contributors?per_page={topN}";
            client.DefaultRequestHeaders.Add("User-Agent", "CSharpApp");
            HttpResponseMessage response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                JsonNode jsonNode = JsonNode.Parse(content);
                JsonArray contributors = jsonNode.AsArray();

                var table = new Table();
                table.AddColumn("排名");
                table.AddColumn("贡献者");
                table.AddColumn("贡献计数");

                int rank = 1;
                foreach (JsonNode contributor in contributors)
                {
                    JsonObject contributorObject = contributor.AsObject();
                    string login = contributorObject["login"].GetValue<string>();
                    int contributions = contributorObject["contributions"].GetValue<int>();
                    table.AddRow(rank.ToString(), $"[link=https://github.com/{login}]{login}[/]", contributions.ToString());
                    rank++;
                }

                AnsiConsole.Write(table);
            }
            else
            {
                AnsiConsole.Markup("[red]获取贡献者数据失败。[/]");
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

                if (int.TryParse(input, out topN) && topN >= 1)
                {
                    break;
                }

                AnsiConsole.Markup("[red]请输入一个大于等于 1 的正整数。[/]\n");
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
            using HttpClient client = new();
            string url = $"https://api.github.com/repos/{owner}/{repo}";
            client.DefaultRequestHeaders.Add("User-Agent", "CSharpApp");
            HttpResponseMessage response = await client.GetAsync(url);
            return response.IsSuccessStatusCode;
        }
    }
}
