using System.Collections.Concurrent;
using System.Text.Json;
using AxolotlMCP.Core.Protocol;
using AxolotlMCP.Core.Protocol.Message;
using AxolotlMCP.Core.Transport;
using Microsoft.Extensions.Logging;

namespace AxolotlMCP.Client;

/// <summary>
/// MCP 客户端：负责与服务器建立连接、发送请求并关联响应、处理通知。
/// </summary>
public sealed class McpClient : IAsyncDisposable
{
    private readonly ITransport _transport;
    private readonly ILogger<McpClient> _logger;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<ResponseMessage>> _pending = new();
    private CancellationTokenSource? _cts;
    private Task? _readLoopTask;

    /// <summary>
    /// 默认请求超时时长。
    /// </summary>
    public TimeSpan DefaultRequestTimeout { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// 通知回调事件。
    /// </summary>
    public event Func<NotificationMessage, CancellationToken, Task>? OnNotification;

    /// <summary>
    /// 构造函数。
    /// </summary>
    /// <param name="transport">底层传输实现</param>
    /// <param name="logger">日志记录器</param>
    public McpClient(ITransport transport, ILogger<McpClient> logger)
    {
        _transport = transport;
        _logger = logger;
    }

    /// <summary>
    /// 建立连接并启动读取循环。
    /// </summary>
    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        await _transport.StartAsync(_cts.Token).ConfigureAwait(false);
        _readLoopTask = Task.Run(() => ReadLoopAsync(_cts.Token), _cts.Token);
        _logger.LogInformation("MCP Client connected.");
    }

    /// <summary>
    /// 断开连接并清理资源。
    /// </summary>
    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _cts?.Cancel();
            if (_readLoopTask is not null)
            {
                await Task.WhenAny(_readLoopTask, Task.Delay(TimeSpan.FromSeconds(2), cancellationToken)).ConfigureAwait(false);
            }
        }
        finally
        {
            await _transport.StopAsync(cancellationToken).ConfigureAwait(false);
            // 失败所有未完成请求
            foreach (var kv in _pending)
            {
                if (_pending.TryRemove(kv.Key, out var tcs))
                {
                    tcs.TrySetException(new OperationCanceledException("Client disconnected"));
                }
            }
            _logger.LogInformation("MCP Client disconnected.");
        }
    }

    /// <summary>
    /// 发送请求并等待响应（带默认超时）。
    /// </summary>
    /// <param name="request">请求消息</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>响应消息</returns>
    public async Task<ResponseMessage> SendRequestAsync(RequestMessage request, CancellationToken cancellationToken = default)
    {
        EnsureConnected();

        var id = Guid.NewGuid().ToString("N");
        var requestWithId = request with { Id = id };

        var tcs = new TaskCompletionSource<ResponseMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        if (!_pending.TryAdd(id, tcs))
        {
            throw new InvalidOperationException("Duplicate request id");
        }

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(DefaultRequestTimeout);

        try
        {
            var json = JsonSerializer.Serialize(requestWithId, JsonDefaults.Options);
            await _transport.SendAsync(json, timeoutCts.Token).ConfigureAwait(false);

            await using var _ = timeoutCts.Token.Register(() => tcs.TrySetException(new TimeoutException($"Request timeout: {id}")));
            return await tcs.Task.ConfigureAwait(false);
        }
        finally
        {
            _pending.TryRemove(id, out _);
        }
    }

    /// <summary>
    /// 发送通知，不等待响应。
    /// </summary>
    public Task SendNotificationAsync(NotificationMessage notification, CancellationToken cancellationToken = default)
    {
        EnsureConnected();
        var json = JsonSerializer.Serialize(notification, JsonDefaults.Options);
        return _transport.SendAsync(json, cancellationToken);
    }

    private async Task ReadLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var line in _transport.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                try
                {
                    using var doc = JsonDocument.Parse(line);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("id", out var idProp))
                    {
                        var response = JsonSerializer.Deserialize<ResponseMessage>(line, JsonDefaults.Options);
                        if (response is null) continue;
                        if (_pending.TryGetValue(response.Id, out var tcs))
                        {
                            tcs.TrySetResult(response);
                        }
                        else
                        {
                            _logger.LogWarning("收到未知响应 Id={Id}", response.Id);
                        }
                    }
                    else if (root.TryGetProperty("method", out _))
                    {
                        var notification = JsonSerializer.Deserialize<NotificationMessage>(line, JsonDefaults.Options);
                        if (notification is null) continue;
                        var handler = OnNotification;
                        if (handler is not null)
                        {
                            await handler(notification, cancellationToken).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("收到未知消息：{Line}", line);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "处理入站消息失败：{Payload}", line);
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "读取循环异常退出");
        }
    }

    private void EnsureConnected()
    {
        if (_cts is null)
        {
            throw new InvalidOperationException("Client is not connected.");
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
    }
}


