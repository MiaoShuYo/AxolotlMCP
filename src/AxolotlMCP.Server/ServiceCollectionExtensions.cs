using AxolotlMCP.Core.Tools;
using AxolotlMCP.Core.Transport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AxolotlMCP.Server.Middleware;

namespace AxolotlMCP.Server;

/// <summary>
/// 服务器相关的服务注册扩展方法。
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 注册 MCP Server 所需的服务。
    /// </summary>
    public static IServiceCollection AddMcpServer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ServerOptions>().Bind(configuration.GetSection("Mcp:Server")).ValidateDataAnnotations();
        services.AddOptions<RateLimitOptions>().Bind(configuration.GetSection("Mcp:RateLimit")).ValidateDataAnnotations();

        services.AddSingleton<ToolRegistry>();
        services.AddSingleton<AxolotlMCP.Core.Resources.ResourceRegistry>();
        services.AddSingleton<AxolotlMCP.Core.Prompts.PromptRegistry>();
        services.AddSingleton<ServerLifecycle>();

        // 基础传输：默认使用 stdio。
        services.AddSingleton<ITransport, StdioTransport>();

        // 默认处理器与服务器。
        services.AddSingleton<DefaultHandler>();
        services.AddSingleton<McpServer>(sp =>
        {
            var transport = sp.GetRequiredService<ITransport>();
            var handler = sp.GetRequiredService<DefaultHandler>();
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<McpServer>>();
            var pipeline = sp.GetRequiredService<RequestMiddlewarePipeline>();
            return new McpServer(transport, handler, logger, pipeline);
        });

        // 托管服务接入宿主生命周期。
        services.AddHostedService<McpServerHostedService>();

        // 服务器通知发送器
        services.AddSingleton<IServerNotifier, ServerNotifier>();

        // 中间件相关注册（按需添加自定义实现到容器，多实例将以注册顺序组成管道）
        services.AddSingleton<RequestMiddlewarePipeline>(sp =>
        {
            var collection = sp.GetServices<IRequestMiddleware>();
            return new RequestMiddlewarePipeline(collection);
        });

        // 示例中间件的注册（受配置控制）
        services.AddSingleton<IRequestMiddleware>(sp =>
        {
            var opts = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<RateLimitOptions>>().Value;
            return new Middleware.RateLimitMiddleware(opts.PermitLimit);
        });
        services.AddSingleton<IRequestMiddleware, Middleware.TimingMiddleware>();
        return services;
    }
}


