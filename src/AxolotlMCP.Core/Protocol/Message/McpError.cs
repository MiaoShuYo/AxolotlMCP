namespace AxolotlMCP.Core.Protocol.Message;

/// <summary>
/// 表示MCP协议中的错误信息
/// </summary>
public record McpError
{
    /// <summary>
    /// 错误代码，遵循 JSON-RPC / MCP 约定的数值标识。
    /// </summary>
    [JsonPropertyName("code")]
    public int Code { get; init; }

    /// <summary>
    /// 错误消息的简要描述，便于人类阅读与日志排查。
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// 可选的附加数据，包含与错误相关的上下文信息。
    /// </summary>
    [JsonPropertyName("data")]
    public JsonElement? Data { get; init; }
}