using System.Diagnostics;

namespace AxolotlMCP.Core.Observability;

/// <summary>
/// MCP 跟踪（OpenTelemetry 兼容）。
/// </summary>
public static class McpTracing
{
    /// <summary>
    /// ActivitySource 名称：AxolotlMCP。
    /// </summary>
    public static readonly ActivitySource Source = new("AxolotlMCP", "1.0.0");
}


