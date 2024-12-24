using Spectre.Console;

namespace ghv.Command
{
    internal class Help
    {
        public static void Show()
        {
            var table = new Table();
            table.AddColumn("命令");
            table.AddColumn("作用");
            table.AddColumn("参数");
            table.AddColumn("别名");

            // [] 需要转义
            table.AddRow("help", "显示帮助信息", "/", "--help -h");
            table.AddRow("version", "显示应用程序版本信息", "/", "v --version ver -v");
            table.AddRow("about", "显示有关此应用的相关信息", "/", "/");
            table.AddRow("contribute", "获取指定仓库的前 N 个贡献者信息", "ctb [[所有者]] [[仓库名]] [[前N个]]", "ctb");
            table.AddRow("labels", "获取指定仓库的所有标签", "label [[所有者]] [[仓库名]]", "label");
            table.AddRow("user", "获取指定用户的信息", "user [[用户名]]", "/");

            AnsiConsole.Write(table);
        }
    }
}
