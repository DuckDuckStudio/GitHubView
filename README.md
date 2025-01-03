# GitHubView

GitHubView 是一个用于查看 GitHub 上的信息的命令行工具。目前仅支持一些小功能，未来将添加更多功能。

## 可用命令

以下是当前可用的命令列表  

| 命令 | 作用 | 参数 | 别名 |
|-----|-----|-----|-----|
| `help` | 查看帮助 | / | `--help` `-h` |
| `version` | 查看版本信息 | / | `v` `--version` `ver` `-v` |
| `about` | 查看关于此应用的信息 | / | / |
| `contribute` | 获取指定仓库的前 N 个贡献者信息 (贡献者排行榜) | `ctb 所有者 仓库名 前N个` | `ctb` |
| `labels` | 获取指定仓库的所有标签 | `label 所有者 仓库名` | `label` |
| `user` | 获取指定用户的信息 | `user 用户名` | / |

### 详细介绍
#### help
> 别名: `--help` `-h`  

<div style="display: flex; gap: 15px;">
  <img src="https://duckduckstudio.github.io/GitHubView/images/README/help.png" alt="显示所有命令的表格" style="height: 75%; width: 75%;">
  <p><strong>显示关于此程序的帮助信息。</strong></p>
</div>

#### version
> 别名: `v` `--version` `ver` `-v`  

<div style="display: flex; gap: 15px;">
  <img src="https://duckduckstudio.github.io/GitHubView/images/README/version.png" alt="显示版本信息的表格" style="height: 75%; width: 75%;">
  <p><strong>显示关于此程序的版本信息。</strong><br>在部分终端可以点击内容跳转到详细链接。</p>
</div>

#### about
<div style="display: flex; gap: 15px;">
  <img src="https://duckduckstudio.github.io/GitHubView/images/README/about.png" alt="显示详细信息的表格" style="height: 75%; width: 75%;">
  <p><strong>显示关于此程序的详细信息。</strong><br>在部分终端可以点击内容跳转到详细链接。</p>
</div>

#### contribute
> 别名: `ctb`  

<div style="display: flex; gap: 15px;">
  <img src="https://duckduckstudio.github.io/GitHubView/images/README/contribute.png" alt="使用示例" style="height: 75%; width: 75%;">
  <p><strong>获取指定仓库的前 N 个贡献者信息。</strong><br>换句话说就是获取仓库的贡献数/者排行榜。<br>点击用户名可跳转用户界面。</p>
</div>

#### labels
> 别名: `label`  

<div style="display: flex; gap: 15px;">
  <img src="https://duckduckstudio.github.io/GitHubView/images/README/labels.png" alt="使用示例" style="height: 75%; width: 75%;">
  <p><strong>获取指定仓库的所有标签。</strong><br><del>相当于<code><a href="https://github.com/DuckDuckStudio/GitHub-Labels-Manager" target="_blank">glm</a> get</code>的表格版。</del></p>
</div>

#### user

<div style="display: flex; gap: 15px;">
  <img src="https://duckduckstudio.github.io/GitHubView/images/README/user.png" alt="使用示例" style="height: 75%; width: 75%;">
  <p><strong>获取指定用户的信息。</strong><br>在部分终端可以点击内容跳转到详细链接。<br>如果某项属性为空则不会显示那项属性。</p>
</div>

## 项目状态
### ⭐星星
[![星标者](https://reporoster.com/stars/DuckDuckStudio/GitHubView)](https://github.com/DuckDuckStudio/GitHubView/stargazers)  
[![星标历史](https://api.star-history.com/svg?repos=DuckDuckStudio/GitHubView&type=Date)](https://star-history.com/#DuckDuckStudio/GitHubView&Date)

### 状态
![仓库状态展示图](https://repobeats.axiom.co/api/embed/a2154e530ad59472054c1230b08ea3efa8d129f7.svg)

## 许可证
本项目基于 [GNU通用公共许可证 v3.0](LICENSE.txt) 许可证开源。  
