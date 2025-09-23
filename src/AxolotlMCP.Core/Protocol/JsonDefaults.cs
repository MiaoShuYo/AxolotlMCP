using System.Buffers;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization.Metadata;

namespace AxolotlMCP.Core.Protocol;

/// <summary>
/// 提供统一的 JSON 序列化配置，确保跨项目行为一致。
/// </summary>
public static class JsonDefaults
{
    /// <summary>
    /// 默认的 JSON 选项：
    /// - 小驼峰命名；
    /// - 忽略空值；
    /// - 允许注释与尾随逗号；
    /// - 不转义中文；
    /// - 高性能配置（池化/快速路径）。
    /// </summary>
    public static readonly JsonSerializerOptions Options = CreateDefaultOptions();

    private static JsonSerializerOptions CreateDefaultOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            WriteIndented = false
        };

        // 预留：可启用 SourceGen（JsonTypeInfoResolver）以进一步优化性能
        // options.TypeInfoResolver = new DefaultJsonTypeInfoResolver();

        return options;
    }
}


