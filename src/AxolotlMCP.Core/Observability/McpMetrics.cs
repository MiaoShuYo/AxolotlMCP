using System.Diagnostics.Metrics;

namespace AxolotlMCP.Core.Observability;

/// <summary>
/// MCP 基础指标。
/// </summary>
public static class McpMetrics
{
    private static readonly Meter Meter = new("AxolotlMCP", "1.0.0");

    /// <summary>
    /// 服务器接收的请求数量计数器。
    /// </summary>
    public static readonly Counter<long> RequestsReceived = Meter.CreateCounter<long>(
        name: "mcp.server.requests.received",
        unit: "count",
        description: "Server received requests count");

    /// <summary>
    /// 服务器接收的通知数量计数器。
    /// </summary>
    public static readonly Counter<long> NotificationsReceived = Meter.CreateCounter<long>(
        name: "mcp.server.notifications.received",
        unit: "count",
        description: "Server received notifications count");

    /// <summary>
    /// 服务器发送的响应数量计数器。
    /// </summary>
    public static readonly Counter<long> ResponsesSent = Meter.CreateCounter<long>(
        name: "mcp.server.responses.sent",
        unit: "count",
        description: "Server responses sent count");

    /// <summary>
    /// 已处理错误数量计数器。
    /// </summary>
    public static readonly Counter<long> Errors = Meter.CreateCounter<long>(
        name: "mcp.errors",
        unit: "count",
        description: "Number of handled errors");

    /// <summary>
    /// 请求端到端耗时直方图（毫秒）。
    /// </summary>
    public static readonly Histogram<double> RequestLatencyMs = Meter.CreateHistogram<double>(
        name: "mcp.request.latency",
        unit: "ms",
        description: "End-to-end request latency in milliseconds");
}


