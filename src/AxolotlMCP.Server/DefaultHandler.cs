using System.Text.Json;
using AxolotlMCP.Core.Interfaces;
using AxolotlMCP.Core.Protocol;
using AxolotlMCP.Core.Protocol.Message;
using AxolotlMCP.Core.Tools;
using Microsoft.Extensions.Logging;
using AxolotlMCP.Core.Resources;
using AxolotlMCP.Core.Prompts;
using AxolotlMCP.Core.Observability;
using System.Diagnostics;

namespace AxolotlMCP.Server;

/// <summary>
/// 默认处理器：实现 initialize / tools/list / tools/call。
/// </summary>
public sealed class DefaultHandler : IMcpHandler
{
    private readonly ToolRegistry _tools;
    private readonly ResourceRegistry _resources;
    private readonly PromptRegistry _prompts;
    private readonly IServerNotifier _notifier;
    private readonly ServerLifecycle _lifecycle;
    private readonly ILogger<DefaultHandler> _logger;

    /// <summary>
    /// 构造函数。
    /// </summary>
    /// <param name="tools">工具注册中心</param>
    /// <param name="resources">资源注册中心</param>
    /// <param name="prompts">提示注册中心</param>
    /// <param name="notifier">服务器通知发送器</param>
    /// <param name="lifecycle">服务器生命周期状态机</param>
    /// <param name="logger">日志记录器</param>
    public DefaultHandler(ToolRegistry tools, ResourceRegistry resources, PromptRegistry prompts, IServerNotifier notifier, ServerLifecycle lifecycle, ILogger<DefaultHandler> logger)
    {
        _tools = tools;
        _resources = resources;
        _prompts = prompts;
        _notifier = notifier;
        _lifecycle = lifecycle;
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
        // 检查生命周期状态
        if (request.Method != "initialize" && !_lifecycle.IsInitialized)
        {
            return new ResponseMessage
            {
                Id = request.Id,
                Error = new McpError { Code = ErrorCodes.NotInitialized, Message = "服务器尚未初始化" }
            };
        }

        if (_lifecycle.IsShuttingDown)
        {
            return new ResponseMessage
            {
                Id = request.Id,
                Error = new McpError { Code = ErrorCodes.ShuttingDown, Message = "服务器正在关闭" }
            };
        }

        return request.Method switch
        {
            "initialize" => await HandleInitializeAsync(request, cancellationToken),
            "tools/list" => await HandleToolsListAsync(request, cancellationToken),
            "tools/call" => await HandleToolsCallAsync(request, cancellationToken),
            "resources/list" => await HandleResourcesListAsync(request, cancellationToken),
            "prompts/list" => await HandlePromptsListAsync(request, cancellationToken),
            "resources/subscribe" => await HandleResourcesSubscribeAsync(request, cancellationToken),
            "prompts/subscribe" => await HandlePromptsSubscribeAsync(request, cancellationToken),
            "shutdown" => await HandleShutdownAsync(request, cancellationToken),
            "exit" => await HandleExitAsync(request, cancellationToken),
            _ => new ResponseMessage
            {
                Id = request.Id,
                Error = new McpError { Code = ErrorCodes.MethodNotFound, Message = "方法未找到" }
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
    public string[] GetSupportedMethods() => new[] { "initialize", "tools/list", "tools/call", "resources/list", "prompts/list", "resources/subscribe", "prompts/subscribe", "shutdown", "exit" };

    private Task<ResponseMessage> HandleInitializeAsync(RequestMessage request, CancellationToken ct)
    {
        if (!_lifecycle.TryInitialize())
        {
            _logger.LogWarning("重复初始化请求");
            return Task.FromResult(new ResponseMessage
            {
                Id = request.Id,
                Error = new McpError { Code = ErrorCodes.InvalidRequest, Message = "服务器已初始化" }
            });
        }

        _logger.LogInformation("服务器已初始化");
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
        _lifecycle.TryShutdown();
        _logger.LogInformation("服务器正在关闭");
        // 对于默认处理器：仅回包成功，具体停机动作由宿主/外层控制。
        return Task.FromResult(new ResponseMessage { Id = request.Id, Result = JsonSerializer.SerializeToElement(new { ok = true }, JsonDefaults.Options) });
    }

    private Task<ResponseMessage> HandleExitAsync(RequestMessage request, CancellationToken ct)
    {
        _lifecycle.MarkExitRequested();
        _logger.LogInformation("收到 exit 通知");
        // exit 作为通知型请求的兼容处理，直接返回成功。
        return Task.FromResult(new ResponseMessage { Id = request.Id, Result = JsonSerializer.SerializeToElement(new { ok = true }, JsonDefaults.Options) });
    }

    private Task<ResponseMessage> HandleToolsListAsync(RequestMessage request, CancellationToken ct)
    {
        using var act = McpTracing.Source.StartActivity("mcp.tools.list", ActivityKind.Internal);
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

    private Task<ResponseMessage> HandleResourcesListAsync(RequestMessage request, CancellationToken ct)
    {
        var items = _resources.GetAll();
        return Task.FromResult(new ResponseMessage
        {
            Id = request.Id,
            Result = JsonSerializer.SerializeToElement(new { resources = items }, JsonDefaults.Options)
        });
    }

    private Task<ResponseMessage> HandlePromptsListAsync(RequestMessage request, CancellationToken ct)
    {
        var items = _prompts.GetAll();
        return Task.FromResult(new ResponseMessage
        {
            Id = request.Id,
            Result = JsonSerializer.SerializeToElement(new { prompts = items }, JsonDefaults.Options)
        });
    }

    private Task<ResponseMessage> HandleResourcesSubscribeAsync(RequestMessage request, CancellationToken ct)
    {
        // 简化实现：订阅后在资源列表发生变更时发送一条通知（事件名：resources/changed）。
        _resources.OnChanged += async (action, name) =>
        {
            var note = new NotificationMessage
            {
                Method = "resources/changed",
                Params = JsonSerializer.SerializeToElement(new { action, name }, JsonDefaults.Options)
            };
            try { await _notifier.NotifyAsync(note, CancellationToken.None).ConfigureAwait(false); } catch { }
        };
        return Task.FromResult(new ResponseMessage
        {
            Id = request.Id,
            Result = JsonSerializer.SerializeToElement(new { ok = true }, JsonDefaults.Options)
        });
    }

    private Task<ResponseMessage> HandlePromptsSubscribeAsync(RequestMessage request, CancellationToken ct)
    {
        _prompts.OnChanged += async (action, name) =>
        {
            var note = new NotificationMessage
            {
                Method = "prompts/changed",
                Params = JsonSerializer.SerializeToElement(new { action, name }, JsonDefaults.Options)
            };
            try { await _notifier.NotifyAsync(note, CancellationToken.None).ConfigureAwait(false); } catch { }
        };
        return Task.FromResult(new ResponseMessage
        {
            Id = request.Id,
            Result = JsonSerializer.SerializeToElement(new { ok = true }, JsonDefaults.Options)
        });
    }

    private async Task<ResponseMessage> HandleToolsCallAsync(RequestMessage request, CancellationToken ct)
    {
        using var act = McpTracing.Source.StartActivity("mcp.tools.call", ActivityKind.Internal);
        if (request.Params is not JsonElement p) return InvalidParams(request.Id);
        if (!p.TryGetProperty("name", out var nameEl) || nameEl.ValueKind != JsonValueKind.String) return InvalidParams(request.Id);
        var name = nameEl.GetString()!;
        act?.SetTag("tool.name", name);
        if (!_tools.TryGet(name, out var tool) || tool is null)
        {
            act?.SetTag("tool.found", false);
            return new ResponseMessage
            {
                Id = request.Id,
                Error = new McpError { Code = ErrorCodes.MethodNotFound, Message = $"工具不存在: {name}" }
            };
        }
        act?.SetTag("tool.found", true);

        JsonElement args = default;
        if (p.TryGetProperty("arguments", out var argsEl)) args = argsEl;
        try
        {
            var result = await tool.ExecuteAsync(args, ct).ConfigureAwait(false);
            act?.SetTag("tool.success", true);
            return new ResponseMessage { Id = request.Id, Result = result };
        }
        catch (Exception ex)
        {
            act?.SetTag("tool.success", false);
            act?.SetTag("exception.type", ex.GetType().FullName);
            act?.SetTag("exception.message", ex.Message);
            throw;
        }
    }

    private static ResponseMessage InvalidParams(string id) => new()
    {
        Id = id,
        Error = new McpError { Code = ErrorCodes.InvalidParams, Message = "无效参数" }
    };
}


