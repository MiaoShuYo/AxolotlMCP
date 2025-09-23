using System.Text.Json.Serialization;

namespace AxolotlMCP.Core.Protocol;

/// <summary>
/// 表示 MCP 协议中的资源定义。
/// 资源通常用于描述可被客户端引用或订阅的内容，例如文件、文档或外部数据源。
/// </summary>
public record McpResource
{
    /// <summary>
    /// 资源的唯一 URI。用于在会话中定位并访问该资源。
    /// </summary>
    [JsonPropertyName("uri")]
    public string Uri { get; init; } = string.Empty;

    /// <summary>
    /// 资源的人类可读名称。
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 对资源的简要说明，用于辅助理解其内容与用途。
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>
    /// 资源的 MIME 类型（如 text/plain, application/json 等）。
    /// </summary>
    [JsonPropertyName("mimeType")]
    public string? MimeType { get; init; }
}


