namespace AxolotlMCP.Core.Interfaces;

/// <summary>
/// 表示MCP客户端接口
/// </summary>
public interface IMcpClient
{
    /// <summary>
    /// 连接到服务器
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>连接任务</returns>
    Task ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 断开连接
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>断开连接任务</returns>
    Task DisconnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送请求消息
    /// </summary>
    /// <param name="request">请求消息</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>响应消息</returns>
    Task<ResponseMessage> SendRequestAsync(RequestMessage request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送通知消息
    /// </summary>
    /// <param name="notification">通知消息</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>发送任务</returns>
    Task SendNotificationAsync(NotificationMessage notification, CancellationToken cancellationToken = default);
}