using Spectre.Console;
using ghv.Command;

namespace ghv
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length > 0 && args[0].ToLower() == "contribute")
            {
                await Contribute.ExecuteAsync();
            }
            else
            {
                AnsiConsole.Markup("[red]无效的命令。请使用 'ghv.exe contribute'。[/]\n");
            }
        }
    }
}
