using AxolotlMCP.Core.Interfaces;
using AxolotlMCP.Core.Protocol;
using AxolotlMCP.Core.Protocol.Message;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AxolotlMCP.Examples.SimpleServer;

/// <summary>
/// 简单的MCP服务器示例
/// </summary>
public class SimpleServer : IMcpHandler
{
    private readonly ILogger<SimpleServer> _logger;

    /// <summary>
    /// 构造函数。
    /// </summary>
    /// <param name="logger">日志记录器实例</param>
    public SimpleServer(ILogger<SimpleServer> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 处理请求消息，根据 <paramref name="request"/> 的 <c>method</c> 路由到对应处理方法。
    /// </summary>
    /// <param name="request">请求消息</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>对应的响应消息</returns>
    public async Task<ResponseMessage> HandleRequestAsync(RequestMessage request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("处理请求: {Method}", request.Method);

        return request.Method switch
        {
            "initialize" => await HandleInitializeAsync(request),
            "tools/list" => await HandleToolsListAsync(request),
            "tools/call" => await HandleToolsCallAsync(request),
            _ => new ResponseMessage
            {
                Id = request.Id,
                Error = new McpError
                {
                    Code = -32601,
                    Message = "方法未找到"
                }
            }
        };
    }

    /// <summary>
    /// 处理通知消息，不返回响应。
    /// </summary>
    /// <param name="notification">通知消息</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>完成任务</returns>
    public Task HandleNotificationAsync(NotificationMessage notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("处理通知: {Method}", notification.Method);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 获取当前服务器支持的方法名列表。
    /// </summary>
    /// <returns>方法名数组</returns>
    public string[] GetSupportedMethods()
    {
        return new[] { "initialize", "tools/list", "tools/call" };
    }

    private Task<ResponseMessage> HandleInitializeAsync(RequestMessage request)
    {
        var result = new
        {
            protocolVersion = "2024-11-05",
            capabilities = new
            {
                tools = new { }
            },
            serverInfo = new
            {
                name = "AxolotlMCP Simple Server",
                version = "1.0.0"
            }
        };

        return Task.FromResult(new ResponseMessage
        {
            Id = request.Id,
            Result = JsonSerializer.SerializeToElement(result)
        });
    }

    private Task<ResponseMessage> HandleToolsListAsync(RequestMessage request)
    {
        var tools = new[]
        {
            new McpTool
            {
                Name = "echo",
                Description = "回显输入的文本",
                InputSchema = new JsonSchema
                {
                    Type = "object",
                    Properties = new Dictionary<string, JsonSchema>
                    {
                        ["text"] = new JsonSchema
                        {
                            Type = "string",
                            Description = "要回显的文本"
                        }
                    },
                    Required = new[] { "text" }
                }
            }
        };

        return Task.FromResult(new ResponseMessage
        {
            Id = request.Id,
            Result = JsonSerializer.SerializeToElement(new { tools })
        });
    }

    private Task<ResponseMessage> HandleToolsCallAsync(RequestMessage request)
    {
        if (request.Params?.TryGetProperty("name", out var nameElement) == true &&
            nameElement.GetString() == "echo")
        {
            if (request.Params.Value.TryGetProperty("arguments", out var argsElement) &&
                argsElement.TryGetProperty("text", out var textElement))
            {
                var result = new
                {
                    content = new[]
                    {
                        new
                        {
                            type = "text",
                            text = textElement.GetString()
                        }
                    }
                };

                return Task.FromResult(new ResponseMessage
                {
                    Id = request.Id,
                    Result = JsonSerializer.SerializeToElement(result)
                });
            }
        }

        return Task.FromResult(new ResponseMessage
        {
            Id = request.Id,
            Error = new McpError
            {
                Code = -32602,
                Message = "无效参数"
            }
        });
    }
}

/// <summary>
/// 程序入口点
/// </summary>
public class Program
{
    /// <summary>
    /// 应用程序入口点。
    /// </summary>
    /// <param name="args">命令行参数</param>
    public static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services.AddSingleton<IMcpHandler, SimpleServer>();
                services.AddLogging(builder => builder.AddConsole());
            })
            .Build();

        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("启动AxolotlMCP简单服务器示例");

        await host.RunAsync();
    }
}
