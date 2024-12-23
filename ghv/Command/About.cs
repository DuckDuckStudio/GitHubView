using Spectre.Console;

namespace ghv.Command
{
    internal class About
    {
        public static void Show()
        {
            var table = new Table();
            table.AddColumn("条目");
            table.AddColumn("内容");
            string versionLink = Program.Version == "develop" 
                ? $"[link=https://github.com/DuckDuckStudio/GitHubView/tree/{Program.Version}][yellow]{Program.Version}[/][/]" 
                : $"[link=https://github.com/DuckDuckStudio/GitHubView/releases/tag/{Program.Version}]{Program.Version}[/]";
            table.AddRow("版本", versionLink);
            table.AddRow("作者", "[link=https://duckduckstudio.github.io/yazicbs.github.io/]鸭鸭「カモ」[/]");
            table.AddRow("许可证", "[link=https://github.com/DuckDuckStudio/GitHubView/blob/master/LICENSE.txt]GNU通用公共许可证 v3.0[/]");
            table.AddRow("仓库", "[link=https://github.com/DuckDuckStudio/GitHubView]https://github.com/DuckDuckStudio/GitHubView[/]");
            table.AddRow("依赖", "[link=https://github.com/spectreconsole/spectre.console/blob/main/LICENSE.md]Spectre.Console(MIT)[/]");
            table.AddRow("建议与反馈", "请前往 [green][link=https://github.com/DuckDuckStudio/GitHubView/issues]GitHub Issue页[/][/] 提交您的建议与反馈");
            table.AddRow("想要贡献？", "请参阅 [green][link=https://github.com/DuckDuckStudio/GitHubView/blob/master/CONTRIBUTING.md]贡献指南[/][/]");
            AnsiConsole.Write(table);
        }
    }
}
