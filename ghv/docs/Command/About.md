# ghv about 命令

## 在这里我们需要做什么
接收到执行指令后，显示一个带有程序信息的表格。

## 如何实现表格显示
我使用第三方库 [Spectre.Console](https://spectreconsole.net/) 来实现表格显示，官方文档: https://spectreconsole.net/widgets/table

## 如何区分开发版本与发行版本
```csharp
using Spectre.Console;

/* 条件表达式
string versionLink = Program.Version == "develop" 
    ? $"[link=https://github.com/DuckDuckStudio/GitHubView/tree/{Program.Version}][yellow]{Program.Version}[/][/]" 
    : $"[link=https://github.com/DuckDuckStudio/GitHubView/releases/tag/{Program.Version}]{Program.Version}[/]";
table.AddRow("版本", versionLink);
*/

Table table = new();
table.AddColumn("条目");
table.AddColumn("内容");

string versionLink;
if (Program.Version == "develop") // 从主程序 (Program.cs) 中获取版本号
{
    versionLink = $"[link=https://github.com/DuckDuckStudio/GitHubView/tree/{Program.Version}][yellow]{Program.Version}[/][/]";
}
else
{
    versionLink = $"[link=https://github.com/DuckDuckStudio/GitHubView/releases/tag/{Program.Version}]{Program.Version}[/]";
}

table.AddRow("版本", versionLink);
```
