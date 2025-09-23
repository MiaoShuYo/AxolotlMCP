using System.ComponentModel.DataAnnotations;

namespace AxolotlMCP.Server;

/// <summary>
/// 并发限流配置项。
/// </summary>
public sealed class RateLimitOptions
{
    /// <summary>
    /// 并发许可上限。
    /// </summary>
    [Range(1, int.MaxValue)]
    public int PermitLimit { get; set; } = 64;

    /// <summary>
    /// 等待队列上限。
    /// </summary>
    [Range(0, int.MaxValue)]
    public int QueueLimit { get; set; } = 256;
}


