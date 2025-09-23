# Tracing 与导出

本项目通过 `ActivitySource` 暴露跟踪数据（OpenTelemetry 兼容）。

## ActivitySource
- 名称：`AxolotlMCP`，版本：`1.0.0`
- 关键 span：
  - `mcp.request`（Server 侧每个请求）
  - 标签：`mcp.method`、`mcp.request.id`

### 工具相关细化 span
- `mcp.tools.list`（Internal）
  - 触发：`tools/list`
  - 典型用途：统计工具列表调用量与时延

- `mcp.tools.call`（Internal）
  - 触发：`tools/call`
  - 标签：
    - `tool.name`：被调用工具名
    - `tool.found`：是否找到工具（true/false）
    - `tool.success`：执行是否成功（true/false）
    - `exception.type`：异常类型（失败时）
    - `exception.message`：异常消息（失败时）

## 使用 OpenTelemetry（示例）

```csharp
using OpenTelemetry;
using OpenTelemetry.Trace;

using var provider = Sdk.CreateTracerProviderBuilder()
    .AddSource("AxolotlMCP")
    .AddConsoleExporter() // 或 .AddOtlpExporter()
    .Build();

// 运行 MCP 服务...
```

## 后续计划
- 为工具调用与中间件扩展更多 span。
- 与指标（Metrics）统一标签与相关性（trace_id）。

## 中间件追踪规划（预留）
- 总体：`mcp.middleware.pipeline`（覆盖整条中间件链）
- 单个中间件：`mcp.middleware.{name}`（每个中间件一个子 span）
  - 标签：
    - `middleware.name`（名称）
    - `middleware.order`（顺序）
    - `middleware.kind`（logging/auth/rate_limit/audit 等）
    - `middleware.success`（true/false）
    - `exception.type` / `exception.message`（失败时）
- 归属：均作为 `mcp.request` 的子 span，形成完整调用链。


