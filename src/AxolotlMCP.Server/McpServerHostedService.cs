using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AxolotlMCP.Server;

/// <summary>
/// 将 <see cref="McpServer"/> 以托管服务形式集成到宿主生命周期。
/// </summary>
public sealed class McpServerHostedService : IHostedService
{
    private readonly McpServer _server;
    private readonly IOptions<ServerOptions> _options;
    private readonly ILogger<McpServerHostedService> _logger;

    /// <summary>
    /// 初始化 <see cref="McpServerHostedService"/> 的新实例。
    /// </summary>
    /// <param name="server">MCP 服务器实例，用于处理消息读写与路由。</param>
    /// <param name="options">服务器配置选项。</param>
    /// <param name="logger">日志记录器。</param>
    public McpServerHostedService(McpServer server, IOptions<ServerOptions> options, ILogger<McpServerHostedService> logger)
    {
        _server = server;
        _options = options;
        _logger = logger;
    }

    /// <summary>
    /// 启动托管服务，启动底层 <see cref="McpServer"/> 并根据配置记录启动日志。
    /// </summary>
    /// <param name="cancellationToken">取消令牌。</param>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _server.StartAsync(cancellationToken).ConfigureAwait(false);
        if (_options.Value.LogStartupMessage)
        {
            _logger.LogInformation("MCP Server hosted service started.");
        }
    }

    /// <summary>
    /// 停止托管服务，优雅关闭 <see cref="McpServer"/>。
    /// </summary>
    /// <param name="cancellationToken">取消令牌。</param>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_options.Value.ShutdownWaitSeconds));
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token);
        await _server.StopAsync(linked.Token).ConfigureAwait(false);
        _logger.LogInformation("MCP Server hosted service stopped.");
    }
}


