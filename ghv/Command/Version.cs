using Spectre.Console;

namespace ghv.Command
{
    internal class Version
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
            table.AddRow("更多信息", "如需获取更多信息，请使用 [bold]ghv about[/]");
            AnsiConsole.Write(table);
        }
    }
}
