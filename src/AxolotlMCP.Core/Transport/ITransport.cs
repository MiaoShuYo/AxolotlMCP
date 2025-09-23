namespace AxolotlMCP.Core.Transport;

/// <summary>
/// 传输层抽象，负责底层连接、收发与关闭。
/// </summary>
public interface ITransport : IAsyncDisposable
{
    /// <summary>
    /// 启动传输（建立连接或准备好读写循环）。
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>启动任务</returns>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 停止传输（优雅关闭并清理资源）。
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>停止任务</returns>
    Task StopAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送一条已序列化的 JSON 文本消息。
    /// </summary>
    /// <param name="jsonPayload">JSON 文本</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>发送任务</returns>
    Task SendAsync(string jsonPayload, CancellationToken cancellationToken = default);

    /// <summary>
    /// 读取消息的异步流。每个元素为完整的一条 JSON 文本消息。
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>JSON 文本消息的异步可枚举序列</returns>
    IAsyncEnumerable<string> ReadAsync(CancellationToken cancellationToken = default);
}


