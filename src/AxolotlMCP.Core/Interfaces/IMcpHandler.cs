namespace AxolotlMCP.Core.Interfaces;

/// <summary>
/// 表示MCP消息处理器接口
/// </summary>
public interface IMcpHandler
{
    /// <summary>
    /// 处理请求消息
    /// </summary>
    /// <param name="request">请求消息</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>响应消息</returns>
    Task<ResponseMessage> HandleRequestAsync(RequestMessage request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 处理通知消息
    /// </summary>
    /// <param name="notification">通知消息</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>处理任务</returns>
    Task HandleNotificationAsync(NotificationMessage notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取支持的方法列表
    /// </summary>
    /// <returns>支持的方法列表</returns>
    string[] GetSupportedMethods();
}
