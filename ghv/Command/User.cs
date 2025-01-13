using Spectre.Console;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace ghv.Command
{
    internal class User
    {
        public static async Task ExecuteAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                username = GetUserInput("请输入用户名:", ValidateUsername);
            }

            bool userExists = await ValidateUserExists(username);
            if (!userExists)
            {
                AnsiConsole.Markup("[red]该用户不存在，请检查输入的用户名是否正确。[/]\n");
                return;
            }

            await FetchAndDisplayUserInfo(username);
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

        private static async Task<bool> ValidateUserExists(string username)
        {
            using HttpClient client = new();
            string url = $"https://api.github.com/users/{username}";
            client.DefaultRequestHeaders.Add("User-Agent", "CSharpApp");
            HttpResponseMessage response = await client.GetAsync(url);
            return response.IsSuccessStatusCode;
        }

        private static async Task FetchAndDisplayUserInfo(string username)
        {
            using HttpClient client = new();
            string url = $"https://api.github.com/users/{username}";
            client.DefaultRequestHeaders.Add("User-Agent", "CSharpApp");
            HttpResponseMessage response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();
                JsonNode? jsonNode = JsonNode.Parse(jsonResponse);
                if (jsonNode == null)
                {
                    AnsiConsole.Markup("[red]无法解析用户数据。[/]\n");
                    return;
                }

                var table = new Table();
                table.Border(TableBorder.Rounded);
                table.BorderColor(Color.Grey);
                table.AddColumn(new TableColumn("属性").Centered());
                table.AddColumn(new TableColumn("值").Centered());

                var propertiesToDisplay = new List<string> { "login", "name", "company", "blog", "location", "email", "bio", "twitter_username" };
                var propertyTranslations = new Dictionary<string, string>
                {
                    { "login", "登录名" },
                    { "name", "姓名" },
                    { "company", "公司" },
                    { "blog", "博客" },
                    { "location", "位置" },
                    { "email", "电子邮件" },
                    { "bio", "简介" },
                    { "twitter_username", "Twitter" }
                };

                string? login = jsonNode["login"]?.ToString();
                string? name = jsonNode["name"]?.ToString();
                if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(login))
                {
                    string combinedName = $"[link=https://github.com/{login}]{name} ([grey]{login}[/])[/]";
                    table.AddRow("姓名", combinedName).Border(TableBorder.Square);
                }

                foreach (var property in jsonNode.AsObject())
                {
                    if (propertiesToDisplay.Contains(property.Key) && property.Key != "login" && property.Key != "name")
                    {
                        string propertyName = propertyTranslations.TryGetValue(property.Key, out string? value) ? value : property.Key;
                        string propertyValue = Markup.Escape(property.Value?.ToString() ?? string.Empty);

                        if (!string.IsNullOrWhiteSpace(propertyValue))
                        {
                            if (property.Key == "twitter_username")
                            {
                                string twitterLink = $"[link=https://twitter.com/{propertyValue}]{propertyValue}[/]";
                                table.AddRow(propertyName, twitterLink).Border(TableBorder.Square);
                            }
                            else if (property.Key == "blog")
                            {
                                string blogLink = $"[link={propertyValue}]{propertyValue}[/]";
                                table.AddRow(propertyName, blogLink).Border(TableBorder.Square);
                            }
                            else if (property.Key == "location")
                            {
                                string sanitizedLocation = Uri.EscapeDataString(propertyValue);
                                string locationLink = $"[link=https://www.bing.com/search?q={sanitizedLocation}]{propertyValue}[/]";
                                table.AddRow(propertyName, locationLink).Border(TableBorder.Square);
                            }
                            else if (property.Key == "email")
                            {
                                string emailLink = $"[link=mailto:{propertyValue}]{propertyValue}[/]";
                                table.AddRow(propertyName, emailLink).Border(TableBorder.Square);
                            }
                            else
                            {
                                table.AddRow(propertyName, propertyValue).Border(TableBorder.Square);
                            }
                        }
                    }
                }

                table.LeftAligned();

                AnsiConsole.Write(table);
            }
            else
            {
                AnsiConsole.Markup("[red]无法获取用户信息，请稍后再试。[/]\n");
            }
        }

        private static bool ValidateUsername(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            string pattern = @"^[a-zA-Z0-9_-]+$";
            return Regex.IsMatch(input, pattern);
        }
    }
}
