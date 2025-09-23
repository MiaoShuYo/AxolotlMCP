namespace AxolotlMCP.Core.Protocol.Message;

/// <summary>
/// 表示MCP协议中的响应消息
/// </summary>
public record ResponseMessage : McpMessage
{
    /// <summary>
    /// 与请求对应的标识符，应与请求中的 Id 保持一致。
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// 成功时返回的结果对象，结构由具体方法定义决定。
    /// </summary>
    [JsonPropertyName("result")]
    public JsonElement? Result { get; init; }

    /// <summary>
    /// 失败时返回的错误对象，包含错误码与描述。
    /// </summary>
    [JsonPropertyName("error")]
    public McpError? Error { get; init; }
}