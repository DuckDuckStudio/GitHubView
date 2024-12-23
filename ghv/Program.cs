using ghv.Command;
using Spectre.Console;

namespace ghv
{
    class Program
    {
        public const string Version = "1.0.1";

        static async Task Main(string[] args)
        {
            if (args.Length > 0)
            {
                // 可能的参数都定义一遍？
                string owner;
                string repo;

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
                        owner = args.Length > 1 ? args[1] : string.Empty;
                        repo = args.Length > 2 ? args[2] : string.Empty;
                        int value = args.Length > 3 && int.TryParse(args[3], out int parsedValue) ? parsedValue : 0; // 0在Contribute.ExecuteAsync中处理等同于未提供
                        await Contribute.ExecuteAsync(owner, repo, value);
                        break;
                    case "labels":
                    case "label":
                        owner = args.Length > 1 ? args[1] : string.Empty;
                        repo = args.Length > 2 ? args[2] : string.Empty;
                        await Labels.ExecuteAsync(owner, repo);
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
