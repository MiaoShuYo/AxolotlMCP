namespace AxolotlMCP.Core.Protocol.Message;

/// <summary>
/// 表示MCP协议中的通知消息
/// </summary>
public record NotificationMessage : McpMessage
{
    /// <summary>
    /// 通知的方法名，不对应响应，例如事件或推送。
    /// </summary>
    [JsonPropertyName("method")]
    public string Method { get; init; } = string.Empty;

    /// <summary>
    /// 通知携带的参数对象，结构由具体通知类型决定。
    /// </summary>
    [JsonPropertyName("params")]
    public JsonElement? Params { get; init; }
}