using ghv.Command;
using Spectre.Console;

namespace ghv
{
    class Program
    {
        public const string Version = "develop";

        static async Task Main(string[] args)
        {
            if (args.Length > 0)
            {
                switch (args[0].ToLower())
                {
                    case "help":
                    case "--help":
                    case "-h":
                        Help.Show();
                        break;
                    case "v":
                    case "--version":
                    case "ver":
                    case "version":
                    case "-v":
                        Command.Version.Show();
                        break;
                    case "about":
                        About.Show();
                        break;
                    case "contribute":
                    case "ctb":
                        await Contribute.ExecuteAsync();
                        break;
                    case "labels":
                        await Labels.ExecuteAsync();
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
