# 服务器 Host 集成与消息路由

本文档详细说明 AxolotlMCP 服务器侧的 Host 集成与消息路由实现原理与方式。

## 目标

- 将 `McpServer` 集成进 .NET Generic Host 生命周期（启动/停止）。
- 通过 `IServiceCollection` 完成依赖注入（传输、工具、处理器、服务器）。
- 提供 `ServerOptions` 选项以配置关闭等待、启动日志等。
- 默认接入 `StdioTransport`，可后续扩展为 WebSocket 等传输。

## 关键组件

- `ServerOptions`：服务器配置项（`ShutdownWaitSeconds`、`LogStartupMessage`）。
- `McpServerHostedService`：把 `McpServer` 作为 `IHostedService` 接入宿主。
- `ServiceCollectionExtensions.AddMcpServer`：一键注册所需服务与托管服务。
- `McpServer`：读取传输数据，反序列化并根据是否含 `id` 与 `method` 路由请求/通知。
- `DefaultHandler`：实现 `initialize`、`tools/list`、`tools/call` 的默认处理。

## 注册与配置

在 `Program.cs` 或宿主构建代码中：

```csharp
Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        services.AddMcpServer(ctx.Configuration);
    })
    .Build()
    .Run();
```

通过 `appsettings.json` 配置：

```json
{
  "Mcp": {
    "Server": {
      "ShutdownWaitSeconds": 2,
      "LogStartupMessage": true
    }
  }
}
```

## 路由策略

- 传入 JSON 文本先解析为 `JsonDocument`。
- 含 `method` 与 `id`：按请求 `RequestMessage` 反序列化，调用处理器并发送 `ResponseMessage`。
- 仅含 `method`：按通知 `NotificationMessage` 反序列化并分发。
- 其它：记录告警日志。

## 关闭流程

`McpServerHostedService.StopAsync` 将结合 `ServerOptions.ShutdownWaitSeconds` 构造超时取消令牌，调用 `McpServer.StopAsync` 优雅下线。

## 扩展点

- 传输层：替换或添加 `ITransport` 实现（WebSocket、命名管道）。
- 处理器：替换默认 `DefaultHandler`，或在其上实现路由注册中心。
- 中间件：在读取循环前后加拦截（日志、限流、审计等）。


