using System.Text.Json.Serialization;

namespace AxolotlMCP.Core.Protocol;

/// <summary>
/// 表示 JSON Schema 定义。
/// 用于约束与描述工具输入、提示参数以及其他结构化数据的格式与规则。
/// </summary>
public record JsonSchema
{
    /// <summary>
    /// JSON Schema 的类型，例如："object"、"array"、"string" 等。
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// 当 <see cref="Type"/> 为 "object" 时，定义对象的属性集合。
    /// 键为属性名，值为该属性对应的 Schema。
    /// </summary>
    [JsonPropertyName("properties")]
    public Dictionary<string, JsonSchema>? Properties { get; init; }

    /// <summary>
    /// 当 <see cref="Type"/> 为 "object" 时，指定必填属性名称列表。
    /// </summary>
    [JsonPropertyName("required")]
    public string[]? Required { get; init; }

    /// <summary>
    /// 当 <see cref="Type"/> 为 "array" 时，定义数组元素的 Schema。
    /// </summary>
    [JsonPropertyName("items")]
    public JsonSchema? Items { get; init; }

    /// <summary>
    /// 当取值类型为枚举时，限定可选值的集合。
    /// </summary>
    [JsonPropertyName("enum")]
    public object[]? Enum { get; init; }

    /// <summary>
    /// 对该 Schema 的说明信息。
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }
}


