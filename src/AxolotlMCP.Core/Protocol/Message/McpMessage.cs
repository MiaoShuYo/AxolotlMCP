namespace AxolotlMCP.Core.Protocol.Message;

/// <summary>
/// 表示MCP协议中的基础消息类型
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "jsonrpc")]
[JsonDerivedType(typeof(RequestMessage), "2.0")]
[JsonDerivedType(typeof(ResponseMessage), "2.0")]
[JsonDerivedType(typeof(NotificationMessage), "2.0")]
public abstract record McpMessage
{
    /// <summary>
    /// JSON-RPC 协议版本标识，固定为 "2.0"。
    /// </summary>
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; init; } = "2.0";
}