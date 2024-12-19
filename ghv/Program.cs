using Spectre.Console;
using ghv.Command;

namespace ghv
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length > 0)
            {
                switch (args[0].ToLower())
                {
                    case "contribute":
                        await Contribute.ExecuteAsync();
                        break;
                    // ...更多命令...
                    default:
                        AnsiConsole.Markup("[red]无效的命令。[/]\n");
                        break;
                }
            }
            else
            {
                AnsiConsole.Markup("[red]请提供一个命令。[/]\n");
            }
        }
    }
}
