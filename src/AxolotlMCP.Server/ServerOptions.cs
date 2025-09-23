using System.ComponentModel.DataAnnotations;

namespace AxolotlMCP.Server;

/// <summary>
/// 服务器配置选项。
/// </summary>
public sealed class ServerOptions
{
    /// <summary>
    /// 读取循环的关闭等待时长。
    /// </summary>
    [Range(0, int.MaxValue)]
    public int ShutdownWaitSeconds { get; set; } = 2;

    /// <summary>
    /// 是否在启动时写一条启动日志。
    /// </summary>
    public bool LogStartupMessage { get; set; } = true;
}


