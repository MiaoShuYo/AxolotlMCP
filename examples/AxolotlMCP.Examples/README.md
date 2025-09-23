# 示例说明

## 简单服务器（Console / Stdio）

项目：`AxolotlMCP.Examples`

入口：`SimpleServer/Program.cs`

功能：
- 支持 `initialize`、`tools/list`、`tools/call`（echo）。
- 通过 .NET `Host` 启动，内置控制台日志。

运行：

```bash
dotnet run --project examples/AxolotlMCP.Examples
```

配置（可选）：
- 参考 `samples/appsettings.sample.json` 创建 `appsettings.json` 放置在工作目录。


