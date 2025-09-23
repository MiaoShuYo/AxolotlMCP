using System.Text.Json.Serialization;

namespace AxolotlMCP.Core.Protocol;

/// <summary>
/// 表示 MCP 协议中的提示（Prompt）定义。
/// 提示用于对模型进行引导或模板化输入，通常可包含参数以适配不同场景。
/// </summary>
public record McpPrompt
{
    /// <summary>
    /// 提示的唯一名称，用于在客户端与服务端之间标识该提示模板。
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 对提示用途的简要说明。
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// 提示参数的 JSON Schema 列表。每个 Schema 描述一个参数的结构与约束。
    /// </summary>
    [JsonPropertyName("arguments")]
    public JsonSchema[] Arguments { get; init; } = Array.Empty<JsonSchema>();
}


