using AxolotlMCP.Core.Interfaces;
using AxolotlMCP.Core.Protocol;
using AxolotlMCP.Core.Protocol.Message;
using AxolotlMCP.Core.Transport;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using AxolotlMCP.Core.Observability;
using System.Diagnostics;

namespace AxolotlMCP.Server;

/// <summary>
/// 简单的 MCP 服务器：基于传输层读取 JSON 文本，反序列化并路由到处理器；
/// 请求将返回响应，通知仅消费不返回。
/// </summary>
public sealed class McpServer
{
    private readonly ITransport _transport;
    private readonly IMcpHandler _handler;
    private readonly Middleware.RequestMiddlewarePipeline _pipeline;
    private readonly ILogger<McpServer> _logger;
    private CancellationTokenSource? _cts;
    private Task? _readLoopTask;

    /// <summary>
    /// 构造函数。
    /// </summary>
    /// <param name="transport">底层传输实现（如 StdioTransport）</param>
    /// <param name="handler">消息处理器（路由的目标）</param>
    /// <param name="logger">日志记录器</param>
    /// <param name="pipeline">请求中间件管道</param>
    public McpServer(ITransport transport, IMcpHandler handler, ILogger<McpServer> logger, Middleware.RequestMiddlewarePipeline pipeline)
    {
        _transport = transport;
        _handler = handler;
        _logger = logger;
        _pipeline = pipeline;
    }

    /// <summary>
    /// 启动服务器：启动传输并开启读取循环。
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        await _transport.StartAsync(_cts.Token).ConfigureAwait(false);
        _readLoopTask = Task.Run(() => ReadLoopAsync(_cts.Token), _cts.Token);
        _logger.LogInformation("MCP Server started.");
    }

    /// <summary>
    /// 停止服务器：取消读取循环并关闭传输。
    /// </summary>
    public async Task StopAsync(CancellationToken cancellationToken = default)
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
            _logger.LogInformation("MCP Server stopped.");
        }
    }

    /// <summary>
    /// 发送通知给对端。
    /// </summary>
    public Task SendNotificationAsync(NotificationMessage notification, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(notification, JsonDefaults.Options);
        return _transport.SendAsync(json, cancellationToken);
    }

    private async Task ReadLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var line in _transport.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                // 粗略判断请求/通知并路由
                try
                {
                    using var doc = JsonDocument.Parse(line);
                    var root = doc.RootElement;
                    var hasMethod = root.TryGetProperty("method", out _);
                    var hasId = root.TryGetProperty("id", out _);

                    if (hasMethod && hasId)
                    {
                        var started = Stopwatch.GetTimestamp();
                        var request = JsonSerializer.Deserialize<RequestMessage>(line, JsonDefaults.Options);
                        if (request is null) continue;
                        using var activity = McpTracing.Source.StartActivity("mcp.request", ActivityKind.Server);
                        activity?.SetTag("mcp.method", request.Method);
                        activity?.SetTag("mcp.request.id", request.Id);
                        var response = await _pipeline.ExecuteAsync(
                            request,
                            cancellationToken,
                            (req, ct) => _handler.HandleRequestAsync(req, ct)
                        ).ConfigureAwait(false);
                        var json = JsonSerializer.Serialize(response, JsonDefaults.Options);
                        await _transport.SendAsync(json, cancellationToken).ConfigureAwait(false);
                        McpMetrics.RequestsReceived.Add(1);
                        McpMetrics.ResponsesSent.Add(1);
                        var elapsedMs = (Stopwatch.GetTimestamp() - started) * 1000.0 / Stopwatch.Frequency;
                        McpMetrics.RequestLatencyMs.Record(elapsedMs);
                    }
                    else if (hasMethod)
                    {
                        var notification = JsonSerializer.Deserialize<NotificationMessage>(line, JsonDefaults.Options);
                        if (notification is null) continue;
                        await _handler.HandleNotificationAsync(notification, cancellationToken).ConfigureAwait(false);
                        McpMetrics.NotificationsReceived.Add(1);
                    }
                    else
                    {
                        _logger.LogWarning("收到未知消息，缺少 method：{Payload}", line);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "处理消息失败：{Payload}", line);
                    McpMetrics.Errors.Add(1);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // 正常关闭
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "读取循环异常退出");
        }
    }
}


