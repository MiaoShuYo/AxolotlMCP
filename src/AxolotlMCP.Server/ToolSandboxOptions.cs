using System.ComponentModel.DataAnnotations;

namespace AxolotlMCP.Server;

/// <summary>
/// 工具沙箱/配额配置。
/// </summary>
public sealed class ToolSandboxOptions
{
    /// <summary>
    /// tools/call 默认超时（秒，0 表示不限制）。
    /// </summary>
    [Range(0, int.MaxValue)]
    public int DefaultTimeoutSeconds { get; set; } = 0;

    /// <summary>
    /// 每个工具的最大并发数（默认 8）。
    /// </summary>
    [Range(1, int.MaxValue)]
    public int DefaultMaxConcurrentPerTool { get; set; } = 8;
}


