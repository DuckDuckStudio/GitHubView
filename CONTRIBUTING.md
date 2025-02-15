# 贡献指南

感谢您对本项目感兴趣并希望贡献您的力量！以下是一些帮助您开始的步骤：

## 如何贡献

> [!TIP]
> 在贡献本项目前，请先阅读程序的[开发文档](https://github.com/DuckDuckStudio/GitHubView/tree/master/ghv/docs)，如对文档有疑问，请提出新的优化分类 Issue。

1. **Fork 仓库**：点击右上角的 "Fork" 按钮，将仓库复制到您的 GitHub 账户中。
2. **克隆仓库**：将 Fork 的仓库克隆到本地计算机。
    ```bash
    git clone https://github.com/your-username/GitHubView.git
    ```
3. **创建分支**：在本地仓库中创建一个新的分支来进行您的更改。
    ```bash
    git checkout -b feature/your-feature-name
    ```
4. **进行更改**：在您的分支上进行更改，并确保代码风格和项目一致。
5. **提交更改**：将更改提交到您的分支。
    ```bash
    git add .
    git commit -m "描述您的更改"
    ```
6. **推送分支**：将分支推送到您的 GitHub 仓库。
    ```bash
    git push origin feature/your-feature-name
    ```
7. **创建 Pull Request**：在 GitHub 上创建一个 **指向 `develop` 分支的** Pull Request，描述您的更改并请求合并。

### 本地调试
1. 安装依赖：
    ```bash
    dotnet add ghv package Spectre.Console
    ```

2. 构建项目：
    ```bash
    dotnet build ghv
    ```

3. 调试修改：
    ```bash
    dotnet run --project ghv <命令或参数>
    ```

### 构建发布
> 只是 .NET 把这个步骤叫做发布，不是 LICENSE 的例外。

```bash
dotnet publish ghv/ghv.csproj --configuration Release --output ./output
dotnet publish ghv/ghv.csproj --configuration Release --output ./output_SingleFile /p:PublishSingleFile=true /p:PublishTrimmed=true # 单文件
```

## 报告问题

如果您发现了一个问题，请通过 GitHub Issues 报告。请提供尽可能详细的信息，以帮助我们重现和解决问题。

感谢您的贡献！
