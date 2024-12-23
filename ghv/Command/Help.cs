using Spectre.Console;

namespace ghv.Command
{
    internal class Help
    {
        public static void Show()
        {
            var table = new Table();
            table.AddColumn("命令");
            table.AddColumn("描述");

            table.AddRow("contribute", "获取指定仓库的前 N 个贡献者信息");
            table.AddRow("help", "显示帮助信息");
            table.AddRow("labels", "显示指定仓库的所有标签");

            AnsiConsole.Write(table);
        }
    }
}
