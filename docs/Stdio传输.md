# Stdio 传输实现说明

本文档介绍 `ITransport` 抽象与 `StdioTransport` 的设计与实现，说明消息收发流程、异常处理与使用方式。

## 设计目标
- 兼容 FastMCP/LLM 工具生态常用的 stdio 通信方式
- 以行为单位传输 JSON 文本（每条消息一行）
- 简洁、可测试、可替换（同抽象下可扩展 WebSocket 等实现）

## 关键类型
- `ITransport`：标准化传输接口，包含 `StartAsync`、`StopAsync`、`SendAsync`、`ReadAsync`
- `StdioTransport`：基于控制台输入/输出的默认实现

## 实现要点
- `StartAsync/StopAsync`：控制 `_running` 标志，确保收发生命周期安全
- `SendAsync`：逐行写入 JSON 文本并 Flush；运行状态校验
- `ReadAsync`：异步按行读取，跳过空行，遇到 EOF 结束
- 取消令牌：使用 `EnumeratorCancellation` 使外部能够取消读取循环

## 使用示例（伪代码）
```
var transport = new StdioTransport();
await transport.StartAsync();
await transport.SendAsync("{\"jsonrpc\":\"2.0\",...}\n");
await foreach (var json in transport.ReadAsync(ct)) {
    // 反序列化并路由处理
}
await transport.StopAsync();
```

## 后续扩展
- 消息分帧协议（如 header + length）以支持多行 JSON
- 背压策略（发送队列、最大缓冲）
- 心跳保活与超时
