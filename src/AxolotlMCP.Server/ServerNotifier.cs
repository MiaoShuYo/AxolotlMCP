using AxolotlMCP.Core.Protocol.Message;

namespace AxolotlMCP.Server;

/// <summary>
/// 通过 <see cref="McpServer"/> 发送通知的默认实现。
/// </summary>
public sealed class ServerNotifier : IServerNotifier
{
    private readonly McpServer _server;

    /// <summary>
    /// 初始化 <see cref="ServerNotifier"/> 的新实例。
    /// </summary>
    /// <param name="server">服务器实例。</param>
    public ServerNotifier(McpServer server)
    {
        _server = server;
    }

    /// <inheritdoc />
    public Task NotifyAsync(NotificationMessage notification, CancellationToken cancellationToken = default)
        => _server.SendNotificationAsync(notification, cancellationToken);
}


