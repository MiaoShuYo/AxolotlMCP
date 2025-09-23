# WebSocket 传输

本实现提供基于 `System.Net.WebSockets.ClientWebSocket` 的文本帧传输，符合 `ITransport` 抽象。

## 使用

```csharp
var transport = new WebSocketTransport("wss://example.com/mcp");
await transport.StartAsync();
await transport.SendAsync("{\"ping\":true}");
await foreach (var msg in transport.ReadAsync())
{
    Console.WriteLine(msg);
}
```

## 行为与约束
- 文本帧（UTF-8）收发；二进制帧不处理。
- 自动收集分片帧，按 EndOfMessage 聚合。
- `StopAsync` 会尝试正常关闭；异常情况下直接结束读取循环。

## 后续增强
- 心跳、背压与超时策略。
- 服务器端 WebSocket（若需要）。


