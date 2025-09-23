using System.Text.Json.Serialization;

namespace AxolotlMCP.Core.Protocol;

/// <summary>
/// 表示 MCP 协议中的工具定义。
/// 工具提供可被调用的能力，并通过输入的 JSON Schema 进行参数校验。
/// </summary>
public record McpTool
{
    /// <summary>
    /// 工具的唯一名称。应简短、可读且具有可识别性。
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 对该工具用途的简要说明，建议用于帮助生成式模型或使用者理解其功能。
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// 工具的输入参数 JSON Schema，用于约束请求时传入的参数格式与类型。
    /// </summary>
    [JsonPropertyName("inputSchema")]
    public JsonSchema InputSchema { get; init; } = new();
}


