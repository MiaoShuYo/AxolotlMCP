using System.ComponentModel.DataAnnotations;

namespace AxolotlMCP.Server;

/// <summary>
/// 安全与请求防护配置。
/// </summary>
public sealed class SecurityOptions
{
    /// <summary>
    /// 是否启用 API Key 鉴权。
    /// </summary>
    public bool ApiKeyEnabled { get; set; } = false;

    /// <summary>
    /// API Key 所在请求头名称。
    /// </summary>
    [Required]
    public string ApiKeyHeader { get; set; } = "x-api-key";

    /// <summary>
    /// 允许的 API Key 列表。
    /// </summary>
    public string[] AllowedKeys { get; set; } = Array.Empty<string>();

    /// <summary>
    /// 单请求超时（秒，0 表示不强制）。
    /// </summary>
    [Range(0, int.MaxValue)]
    public int RequestTimeoutSeconds { get; set; } = 0;
}


