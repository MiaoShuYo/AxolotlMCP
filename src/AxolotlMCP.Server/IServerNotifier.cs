using AxolotlMCP.Core.Protocol.Message;

namespace AxolotlMCP.Server;

/// <summary>
/// 服务器通知发送抽象。
/// </summary>
public interface IServerNotifier
{
    /// <summary>
    /// 发送通知消息到对端。
    /// </summary>
    Task NotifyAsync(NotificationMessage notification, CancellationToken cancellationToken = default);
}


