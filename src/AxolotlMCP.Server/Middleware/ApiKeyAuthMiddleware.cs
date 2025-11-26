using System.Text.Json;
using AxolotlMCP.Core.Observability;
using AxolotlMCP.Core.Protocol.Message;
using Microsoft.Extensions.Options;

namespace AxolotlMCP.Server.Middleware;

/// <summary>
/// 简单的 API Key 鉴权中间件：从请求 params.headers 中读取并校验。
/// </summary>
public sealed class ApiKeyAuthMiddleware : IRequestMiddleware
{
    private readonly SecurityOptions _options;

    /// <summary>
    /// 初始化 API Key 鉴权中间件。
    /// </summary>
    /// <param name="options"></param>
    public ApiKeyAuthMiddleware(IOptions<SecurityOptions> options)
    {
        _options = options.Value;
    }

    /// <summary>
    /// 处理请求。
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="next"></param>
    /// <returns></returns>

    public Task<ResponseMessage> InvokeAsync(RequestMessage request, CancellationToken cancellationToken, Func<RequestMessage, CancellationToken, Task<ResponseMessage>> next)
    {
        if (!_options.ApiKeyEnabled)
        {
            return next(request, cancellationToken);
        }

        using var act = McpTracing.Source.StartActivity("mcp.middleware.auth", System.Diagnostics.ActivityKind.Internal);
        act?.SetTag("middleware.name", "auth");

        // 读取 params.headers[ApiKeyHeader]
        if (request.Params is JsonElement p && p.TryGetProperty("headers", out var headers) && headers.ValueKind == JsonValueKind.Object)
        {
            if (headers.TryGetProperty(_options.ApiKeyHeader, out var keyEl) && keyEl.ValueKind == JsonValueKind.String)
            {
                var key = keyEl.GetString();
                if (!string.IsNullOrEmpty(key) && _options.AllowedKeys.Contains(key))
                {
                    act?.SetTag("middleware.success", true);
                    return next(request, cancellationToken);
                }
            }
        }

        act?.SetTag("middleware.success", false);
        return Task.FromResult(new ResponseMessage
        {
            Id = request.Id,
            Error = new McpError { Code = 401, Message = "Unauthorized" }
        });
    }
}


