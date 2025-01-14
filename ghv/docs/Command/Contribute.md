# ghv ctb 命令

> [!TIP]  
> 接收的参数 `[owner] [repo] [topN]`，  
> 对应 `所有者 仓库名 前N个`。  

## 接分发的参数
```csharp
public static async Task ExecuteAsync(string owner, string repo, int topN)
{
    if (string.IsNullOrWhiteSpace(owner)) // 如果某个参数用户没给，让用户给
    {
        owner = GetUserInput("请输入仓库所有者名称:", ValidateOwnerOrRepo); // 调用获取输入方法
    }

    // ...

    if (topN < 1) // 此参数必须 >= 1
    {
        topN = GetTopNInput("请输入要获取前几个贡献者信息:"); // 调用后续的获取输入 + 验证方法
    }

    // ...
}

private static string GetUserInput(string prompt, Func<string, bool> validator)
// 获取用户输入方法
{
    string input;
    do
    {
        AnsiConsole.Markup("[cyan]? [/]" + prompt + " ");
        input = (Console.ReadLine() ?? string.Empty).Trim(); // 防止用户还是不给参数

        if (!validator(input))
        {
            AnsiConsole.Markup("[red]输入无效，请重新输入。[/]\n");
        }
    }
    while (string.IsNullOrWhiteSpace(input) || !validator(input));

    // 我这用 do...while , 你想用其他循环 (例如直接 while 循环) 也可以

    return input; // 返回用户输入
}

private static int GetTopNInput(string prompt)
// 获取 topN 输入 + 验证
{
    int topN;
    do
    {
        AnsiConsole.Markup("[cyan]? [/]" + prompt + " ");
        string input = (Console.ReadLine() ?? string.Empty).Trim();

        // 尝试解析输入为 int 类型
        if (int.TryParse(input, out topN))
        {
            // 检查输入的数字是否在 int 类型的有效范围内
            if (topN >= 1)
            {
                break;
            }
            else
            {
                AnsiConsole.Markup("[red]请输入一个大于等于 1 的正整数。[/]\n");
            }
        }
        else
        {
            // 如果 int.TryParse 失败，说明输入不符合 int 类型的格式
            // 检查是否超出 int 范围
            if (input.Length > 10 || long.TryParse(input, out long largeInput) && largeInput > int.MaxValue)
            {
                AnsiConsole.Markup("[red]要被塞满了！int 类型最大为 2,147,483,647[/]\n");
            }
            else
            {
                // 普通的格式错误提示
                AnsiConsole.Markup("[red]请输入一个有效的正整数。[/]\n");
            }
        }
    }
    while (true);

    return topN;
}
```

### 验证参数准确性
```csharp
// 前面我们接收了 owner 和 repo，同时判断完了 topN 参数，这里我们需要验证用户给的 owner 和 repo 是不是对的
public static async Task ExecuteAsync(string owner, string repo, int topN)
{
    // ...

    bool repositoryExists = await ValidateRepositoryExists(owner, repo);
    if (!repositoryExists) // 如果仓库被认为不存在或无法访问
    {
        return;
    }

    // ...
}

private static bool ValidateOwnerOrRepo(string input)
// 验证 owner 和 repo 是否没有带什么奇奇怪怪的特殊符号
{
    if (string.IsNullOrWhiteSpace(input)) return false;
    string pattern = @"^[a-zA-Z0-9_-]+(?:[._][a-zA-Z0-9_-]+)*$"; // 匹配正则
    return !input.Contains('/') && Regex.IsMatch(input, pattern);
}

private static async Task<bool> ValidateRepositoryExists(string owner, string repo)
// 验证用户指定的 owner 和 repo 组合是否成立
// 即验证仓库是否存在
{
    try
    {
        using HttpClient client = new();
        string url = $"https://api.github.com/repos/{owner}/{repo}";
        client.DefaultRequestHeaders.Add("User-Agent", "CSharpApp"); // 请求头
        HttpResponseMessage response = await client.GetAsync(url); // 尝试向仓库 API 发送请求

        if (!response.IsSuccessStatusCode) // 如果**不是**返回成功状态码
        {
            // 仓库不存在的提示
            AnsiConsole.Markup("[red]仓库不存在(或没有访问权限)，请检查输入的所有者和仓库名是否正确。[/]\n");
            return false;
        }
        return true; // 否则返回 true
    }
    catch (HttpRequestException e) // 捕获请求异常
    {
        // 处理速率限制或其他访问错误
        AnsiConsole.Markup($"[red]无法验证仓库是否存在: 请求错误[/]\n[red]您的请求可能已达到 [link=https://docs.github.com/zh/rest/using-the-rest-api/rate-limits-for-the-rest-api]GitHub API 速率限制[/][/]\n[red]详细错误信息: {e}[/]\n");

        // 如果用户在第一次请求就超过速率...TA这是多想请求啊...让我请求!!!
        // 这里如果你想的话可以添加更多分类，例如检查是仓库不存在(404)还是超过速率

        return false;
    }
    catch (Exception ex) // 捕获其他异常
    {
        // 其他异常处理
        AnsiConsole.Markup($"[red]无法验证仓库是否存在: {ex.Message}[/]\n");
        return false;
    }
}
```

### 获取仓库贡献排名并展示给用户
```csharp
private static async Task GetContributorsAsync(string owner, string repo, int topN)
{
    using HttpClient client = new();
    client.DefaultRequestHeaders.Add("User-Agent", "CSharpApp");
    int pages = (int)Math.Ceiling((double)topN / 100); // 提前声明总页数，每页个数在后面计算
    try
    {
        if (topN <= 100) // 100个以内不需要分页
        {
            pages = 1; // 1 页就够 - 不用再请求总页数
        }
        else // 100个以上则先定义每页100个以减少页数，进而减少请求次数
        {
            HttpResponseMessage response = await client.GetAsync("https://api.github.com/repos/{owner}/{repo}/contributors"); // 先请求一下获取总页数
            if (response.IsSuccessStatusCode)
            {
                string? linkHeader = response.Headers.Contains("Link") ? response.Headers.GetValues("Link").FirstOrDefault() : null;
                if (linkHeader == null)
                {
                    // 解析失败
                    AnsiConsole.Markup("[yellow]无法解析总分页数: Link 头为 null[/]\n");
                }
                else
                {
                    /* 返回样例
                    $ curl -I https://api.github.com/repos/microsoft/PowerToys/contributors
                    HTTP/1.1 200 OK
                    Content-Type: application/json; charset=utf-8
                    Link: <https://api.github.com/repositories/184456251/contributors?page=2>; rel="next", <https://api.github.com/repositories/184456251/contributors?page=14>; rel="last" - 分页的下一页和最后一页，如果你请求的不是第一页还会返回上一页，通过 rel="xxx" 来判断到底哪个链接是哪个类型，例如这里返回下一页是第2页，一共有14页。分页默认1页30个，如果指定分页格式后总页数就会不一样。
                    x-github-api-version-selected: 2022-11-28 - API 版本
                    X-RateLimit-Limit: 60 - 速率限制(每小时)
                    X-RateLimit-Remaining: 59 - 剩余速率
                    X-RateLimit-Reset: 1736856585 - 速率重置时间
                    X-RateLimit-Resource: core - 速率资源
                    X-RateLimit-Used: 1 - 使用的速率
                    ...
                    */
                    Regex regex = new(@"<([^>]+)>;\s*rel=""last"""); // 去掉 URL 两侧的 < >
                    Match match = regex.Match(linkHeader);
                    string allPagesString = match.Groups[1].Value; // 返回匹配的 URL 部分

                    if (match.Success)
                    {
                        if (int.TryParse(allPagesString, out int allPages))
                        {
                            // 成功解析，allPages 现在是整数
                            /*
                                首先将 topN 强制转换为 double 类型，以确保执行除法时获得浮点数。
                                然后通过 Math.Ceiling() 向上取整。
                                最后将结果转换回 int 类型获得最终需要请求的总页数。
                            */
                            if (pages > allPages)
                            {
                                pages = allPages; // 再多也不能超出总页数呀
                            }
                        }
                        else
                        {
                            // 解析失败
                            AnsiConsole.Markup("[yellow]无法解析总分页数: 匹配失败[/]\n");
                        }
                    }
                }
            }
        }
    }
    catch (HttpRequestException e)
    {
        // 请求失败 - 但不影响后续处理
        AnsiConsole.Markup($"[yellow]无法解析总分页数: 请求失败[/]\n[yellow]信息:{e.Message}[/]");
        // 直接用最开始算的，后面还有错再报，这里用警告样式
    }

#if DEBUG // 调试输出
    AnsiConsole.Markup($"[purple][[DEBUG]] 请求分页数: {pages}[/]\n");
#endif

    List<JsonNode> allContributors = []; // 存放获取到的贡献数据
    string url;

    for (int i = 1; i <= pages; i++) // 循环直到请求完所有分页
    {
        // 计算此页个数
        if ((i == pages) && (topN % 100 != 0)) // 如果是最后一页
        {
            url = $"https://api.github.com/repos/{owner}/{repo}/contributors?per_page={topN % 100}&page={i}";
        }
        else
        {
            // 以最大的每页个数(100)来请求，减少请求次数，进而避免速率限制
            url = $"https://api.github.com/repos/{owner}/{repo}/contributors?per_page={100}&page={i}";
        }

#if DEBUG // 调试输出
        AnsiConsole.Markup($"[purple][[DEBUG]] 请求: {url}[/]\n");
#endif

        try
        {
            HttpResponseMessage response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();
                JsonNode? jsonNode = JsonNode.Parse(jsonResponse);
                if (jsonNode == null || jsonNode.AsArray() == null)
                {
                    AnsiConsole.Markup("[red]无法解析贡献者数据: 请求成功，但返回 null[/]\n");
                    return;
                }

                // 合并获取到的贡献者
                allContributors.AddRange(jsonNode.AsArray().Cast<JsonNode>());
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            // 如果还是不幸遇到了速率限制
            {
                string? resetTime = response.Headers.Contains("X-RateLimit-Reset")
                    ? response.Headers.GetValues("X-RateLimit-Reset").FirstOrDefault() // 获取第一个值
                    : "未知";

                AnsiConsole.Markup($"[yellow]你 太 快 了 ！[/]\n[yellow]您的请求已达到 [link=https://docs.github.com/zh/rest/using-the-rest-api/rate-limits-for-the-rest-api]GitHub API 速率限制[/][/]\n");
                if ((resetTime != null) && (resetTime != "未知"))
                {
                    // 等待速率限制重置
#if DEBUG
                    AnsiConsole.Markup($"[purple][[DEBUG]] 速率重置时间戳: {Markup.Escape(resetTime)}[/]\n");
#endif
                    long resetTimestamp = long.Parse(resetTime);
                    DateTime resetDateTimeUtc = DateTimeOffset.FromUnixTimeSeconds(resetTimestamp).DateTime;
                    // 将 UTC 时间转换为本地时间
                    DateTime resetDateTimeLocal = resetDateTimeUtc.ToLocalTime();
#if DEBUG
                    AnsiConsole.Markup($"[purple][[DEBUG]] 转化后速率重置时间: {resetDateTimeLocal}[/]\n");
#endif
                    TimeSpan waitTime = resetDateTimeLocal - DateTime.Now;
                    AnsiConsole.Markup($"[yellow]重置时间: {resetDateTimeLocal} (需要等待 {waitTime} )[/]");
                    Thread.Sleep(waitTime); // 等待重置 - 等待时 Ctrl + C可退出
                    i--; // 本次请求没成功，后面重试时再请求这页
                }
                else
                // 我请求不到，又不知道啥时候才能再请求到，我还能咋办？
                {
                    AnsiConsole.Markup("[red]无法获取贡献数据: 请求达到速率限制，但无法获取重置时间戳: X-RateLimit-Reset 头为 null 或无法转为 String 类型[/]\n");
                    return;
                }
            }
            else
            // 请求失败
            {

                AnsiConsole.Markup("[red]无法获取贡献数据: 请求失败: 未捕获异常[/]\n");
                return;
            }
        }
        catch (Exception ex)
        // 请求失败 - 捕获异常
        {
            // 捕获网络或其他错误
            AnsiConsole.Markup($"[red]无法获取贡献数据: 请求发生异常: {Markup.Escape(ex.Message)}[/]\n");
        }

        Thread.Sleep(1000); // 请求间间隔 1 秒 - https://docs.github.com/zh/rest/using-the-rest-api/best-practices-for-using-the-rest-api?apiVersion=2022-11-28#handle-rate-limit-errors-appropriately - 使用 REST API 的最佳做法
    }

    if (allContributors.Count > 0)
    // 如果前面有请求到
    {
        Table table = new();
        table.AddColumn("排名");
        table.AddColumn("贡献者");
        table.AddColumn("贡献计数");

        int rank = 1;
        foreach (JsonNode contributor in allContributors)
        // 循环添加每个贡献者的数据 - [贡献排名] [登录名] [贡献数]
        // 这里我们不需要手动去计算贡献排名，GitHub API就是按贡献从多到少返回的 :)
        {
            string login = Markup.Escape(contributor["login"]?.ToString() ?? string.Empty);
            int contributions = contributor["contributions"]?.GetValue<int>() ?? 0;
            table.AddRow(rank.ToString(), $"[link=https://github.com/{login}]{login}[/]", contributions.ToString());
            rank++;
        }

        AnsiConsole.Write(table);
    }
    else
    // 如果前面啥都没获取到
    {
        AnsiConsole.Markup("[red]该仓库没有贡献者 (如果仓库为空则可能出现此情况)。[/]");
    }
}
```
