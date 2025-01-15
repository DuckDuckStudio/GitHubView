# ghv 主程序

## 介绍
ghv 的主程序实际为一个命令分发脚本，获取用户的配置和命令及其参数后，将所需内容分发给 `Command` 文件夹内的各个命令执行脚本。  

## 详细注释
```csharp
// Version 1.0.2
using ghv.Command;
using Spectre.Console; // 第三方库 NuGet 安装，MIT 许可

namespace ghv
{
    class Program
    {
        // ...
        static async Task Main(string[] args) // string[] args 用于接收参数
        {
            if (args.Length > 0) // 此处用于判断用户是否给出了需要执行的命令
            {
                // 此处我不确定是否要全部可能的参数都定义一遍，不过目前有的参数在这里定义
                string owner;
                string repo;

                switch (args[0].ToLower()) // 将命令(传给 ghv 的第1个参数)全部转为小写，使得命令不区分大小写

                // NOTE: 接下来的命令参数区分大小写
                {
                    // ghv --help
                    case "help":
                    case "--help":
                    case "-h":
                        Help.Show();
                        break;
                    // ghv --version
                    case "v":
                    case "--version":
                    case "ver":
                    case "version":
                    case "-v":
                        Command.Version.Show();
                        break;
                    // ghv about
                    case "about":
                        About.Show();
                        break;
                    // ghv ctb [owner] [repo] [topN]
                    case "contribute":
                    case "ctb":
                        /*
                        * 判断参数长度是否大于 1 (即是否存在第 2 个参数) ↓
                        * 如果有，则作为参数传递给执行脚本 | 如果没有，则作为 空 (string.Empty) 传递给执行脚本，执行脚本处会处理 ↓
                        * await 命令.ExecuteAsync(参数1, 参数2, 参数3) 来调用执行脚本并等待执行完毕 ↓
                        * break; 返回。
                        */
                        owner = args.Length > 1 ? args[1] : string.Empty;
                        repo = args.Length > 2 ? args[2] : string.Empty;
                        int value = args.Length > 3 && int.TryParse(args[3], out int parsedValue) ? parsedValue : 0; // 0 在 Command/Contribute.ExecuteAsync 中处理等同于未提供
                        await Contribute.ExecuteAsync(owner, repo, value); // 调用命令执行脚本并传递需要的参数(如果有提供)
                        break;
                    // ghv label [owner] [repo]
                    case "labels":
                    case "label":
                        // ...
                        await Labels.ExecuteAsync(owner, repo);
                        break;
                    case "user":
                        // ...
                        await User.ExecuteAsync(username);
                        break;
                    // 可以再加 case 来处理更多命令
                    default: // 默认情况，即用户给的命令是无效的情况
                        AnsiConsole.Markup("[red]无效的命令。[/]\n");
                        break;
                }
            }
            else // 如果用户没给命令谁知道要分发给谁执行啊 :(
            {
                AnsiConsole.Markup("[red]请提供一个命令。[/]\n");
                // 颜色控制如果不用 Spectre.Console 可以参考我的这篇文章:
                // https://duckduckstudio.github.io/Articles/#/信息速查/csharp/控制台程序/输出/控制颜色
            }
        }
    }
}
```

### 判断用户是否给出了命令
```csharp
static async Task Main(string[] args) // string[] args 用于接收参数
{
    if (args.Length > 0) // 如果参数长度大于 0
    {
        // ...
    }
    else
    {
        // ...
    }
}
```

### 判断用户是否给出了参数
```csharp
// Main 方法接收用户给的 args
// ghv ctb [owner] [repo] [topN]
string owner; // 参数1
string repo; // 参数2

// 通过判断参数长度来得知
owner = args.Length > 1 ? args[1] : string.Empty;
// 将表达式替换为语句:
/*
if (args.Length > 1) // 如果用户有给出第 2 个参数
{
    owner = args[1]; // 获取值
}
else
{
    owner = string.Empty; // 默认为空
}
*/
// ...
int value = args.Length > 3 && int.TryParse(args[3], out int parsedValue) ? parsedValue : 0; // 0 在 Contribute.ExecuteAsync 中处理等同于未提供
// 将表达式替换为语句:
/*
int value; // 参数3 - 整型
if (args.Length > 3 && int.TryParse(args[3], out int parsedValue))
// args.Length > 3 判断用户有没有给出第 3 个参数
// int.TryParse(args[3], out int parsedValue) 尝试将参数转为 int 类型
{
    value = parsedValue; // 赋值给第三个参数
}
else
{
    value = 0; // 默认为 0
}
*/
```
