namespace AxolotlMCP.Core.Interfaces;

/// <summary>
/// 表示MCP服务器接口
/// </summary>
public interface IMcpServer
{
    /// <summary>
    /// 启动服务器
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>启动任务</returns>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 停止服务器
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>停止任务</returns>
    Task StopAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送通知消息
    /// </summary>
    /// <param name="notification">通知消息</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>发送任务</returns>
    Task SendNotificationAsync(NotificationMessage notification, CancellationToken cancellationToken = default);
}