using System.Text.Json;
using AxolotlMCP.Core.Interfaces;
using AxolotlMCP.Core.Protocol;
using AxolotlMCP.Core.Protocol.Message;
using AxolotlMCP.Core.Tools;
using Microsoft.Extensions.Logging;

namespace AxolotlMCP.Server;

/// <summary>
/// 默认处理器：实现 initialize / tools/list / tools/call。
/// </summary>
public sealed class DefaultHandler : IMcpHandler
{
    private readonly ToolRegistry _tools;
    private readonly ILogger<DefaultHandler> _logger;

    /// <summary>
    /// 构造函数。
    /// </summary>
    /// <param name="tools">工具注册中心</param>
    /// <param name="logger">日志记录器</param>
    public DefaultHandler(ToolRegistry tools, ILogger<DefaultHandler> logger)
    {
        _tools = tools;
        _logger = logger;
    }

    /// <summary>
    /// 处理请求消息，路由到 initialize / tools/list / tools/call。
    /// </summary>
    /// <param name="request">请求消息</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>响应消息</returns>
    public async Task<ResponseMessage> HandleRequestAsync(RequestMessage request, CancellationToken cancellationToken = default)
    {
        return request.Method switch
        {
            "initialize" => await HandleInitializeAsync(request, cancellationToken),
            "tools/list" => await HandleToolsListAsync(request, cancellationToken),
            "tools/call" => await HandleToolsCallAsync(request, cancellationToken),
            "shutdown" => await HandleShutdownAsync(request, cancellationToken),
            "exit" => await HandleExitAsync(request, cancellationToken),
            _ => new ResponseMessage
            {
                Id = request.Id,
                Error = new McpError { Code = -32601, Message = "方法未找到" }
            }
        };
    }

    /// <summary>
    /// 处理通知消息。
    /// </summary>
    /// <param name="notification">通知消息</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>任务</returns>
    public Task HandleNotificationAsync(NotificationMessage notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("通知: {Method}", notification.Method);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 返回当前支持的方法名列表。
    /// </summary>
    /// <returns>方法名数组</returns>
    public string[] GetSupportedMethods() => new[] { "initialize", "tools/list", "tools/call", "shutdown", "exit" };

    private Task<ResponseMessage> HandleInitializeAsync(RequestMessage request, CancellationToken ct)
    {
        var result = new
        {
            protocolVersion = "2024-11-05",
            capabilities = new { tools = new { } },
            serverInfo = new { name = "AxolotlMCP Server", version = "1.0.0" }
        };

        return Task.FromResult(new ResponseMessage
        {
            Id = request.Id,
            Result = JsonSerializer.SerializeToElement(result, JsonDefaults.Options)
        });
    }

    private Task<ResponseMessage> HandleShutdownAsync(RequestMessage request, CancellationToken ct)
    {
        // 对于默认处理器：仅回包成功，具体停机动作由宿主/外层控制。
        return Task.FromResult(new ResponseMessage { Id = request.Id, Result = JsonSerializer.SerializeToElement(new { ok = true }, JsonDefaults.Options) });
    }

    private Task<ResponseMessage> HandleExitAsync(RequestMessage request, CancellationToken ct)
    {
        // exit 作为通知型请求的兼容处理，直接返回成功。
        return Task.FromResult(new ResponseMessage { Id = request.Id, Result = JsonSerializer.SerializeToElement(new { ok = true }, JsonDefaults.Options) });
    }

    private Task<ResponseMessage> HandleToolsListAsync(RequestMessage request, CancellationToken ct)
    {
        var items = _tools.GetAll().Select(t => new McpTool
        {
            Name = t.Name,
            Description = t.Description,
            InputSchema = t.InputSchema
        }).ToArray();

        return Task.FromResult(new ResponseMessage
        {
            Id = request.Id,
            Result = JsonSerializer.SerializeToElement(new { tools = items }, JsonDefaults.Options)
        });
    }

    private async Task<ResponseMessage> HandleToolsCallAsync(RequestMessage request, CancellationToken ct)
    {
        if (request.Params is not JsonElement p) return InvalidParams(request.Id);
        if (!p.TryGetProperty("name", out var nameEl) || nameEl.ValueKind != JsonValueKind.String) return InvalidParams(request.Id);
        var name = nameEl.GetString()!;
        if (!_tools.TryGet(name, out var tool) || tool is null)
        {
            return new ResponseMessage
            {
                Id = request.Id,
                Error = new McpError { Code = -32601, Message = $"工具不存在: {name}" }
            };
        }

        JsonElement args = default;
        if (p.TryGetProperty("arguments", out var argsEl)) args = argsEl;
        var result = await tool.ExecuteAsync(args, ct).ConfigureAwait(false);
        return new ResponseMessage { Id = request.Id, Result = result };
    }

    private static ResponseMessage InvalidParams(string id) => new()
    {
        Id = id,
        Error = new McpError { Code = -32602, Message = "无效参数" }
    };
}


