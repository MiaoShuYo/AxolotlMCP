namespace AxolotlMCP.Core.Protocol.Message;

/// <summary>
/// 表示MCP协议中的请求消息
/// </summary>
public record RequestMessage : McpMessage
{
    /// <summary>
    /// 请求的唯一标识符，用于将响应与请求进行关联。
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// 调用的方法名，例如 "initialize"、"tools/list" 等。
    /// </summary>
    [JsonPropertyName("method")]
    public string Method { get; init; } = string.Empty;

    /// <summary>
    /// 方法调用的参数对象，结构由具体方法定义决定。
    /// </summary>
    [JsonPropertyName("params")]
    public JsonElement? Params { get; init; }
}