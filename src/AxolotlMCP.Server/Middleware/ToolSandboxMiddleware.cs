using System.Collections.Concurrent;
using System.Text.Json;
using AxolotlMCP.Core.Protocol.Message;
using Microsoft.Extensions.Options;

namespace AxolotlMCP.Server.Middleware;

/// <summary>
/// tools/call 的最小沙箱与配额：按工具名限流，并支持执行超时。
/// </summary>
public sealed class ToolSandboxMiddleware : IRequestMiddleware
{
    private readonly ToolSandboxOptions _options;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _toolSemaphores = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 创建工具沙箱中间件。
    /// </summary>
    /// <param name="options"></param>
    public ToolSandboxMiddleware(IOptions<ToolSandboxOptions> options)
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
    public async Task<ResponseMessage> InvokeAsync(RequestMessage request, CancellationToken cancellationToken, Func<RequestMessage, CancellationToken, Task<ResponseMessage>> next)
    {
        if (!string.Equals(request.Method, "tools/call", StringComparison.Ordinal))
        {
            return await next(request, cancellationToken).ConfigureAwait(false);
        }

        string toolName = "";
        int maxConc = _options.DefaultMaxConcurrentPerTool;
        int timeoutSec = _options.DefaultTimeoutSeconds;

        if (request.Params is JsonElement p)
        {
            if (p.TryGetProperty("name", out var nameEl) && nameEl.ValueKind == JsonValueKind.String)
            {
                toolName = nameEl.GetString() ?? string.Empty;
            }
            if (p.TryGetProperty("sandbox", out var sb) && sb.ValueKind == JsonValueKind.Object)
            {
                if (sb.TryGetProperty("maxConcurrent", out var mc) && mc.TryGetInt32(out var mcVal) && mcVal > 0)
                {
                    maxConc = mcVal;
                }
                if (sb.TryGetProperty("timeoutSeconds", out var ts) && ts.TryGetInt32(out var tsVal) && tsVal >= 0)
                {
                    timeoutSec = tsVal;
                }
            }
        }

        var sem = _toolSemaphores.GetOrAdd(toolName, _ => new SemaphoreSlim(maxConc, maxConc));
        await sem.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (timeoutSec <= 0)
            {
                return await next(request, cancellationToken).ConfigureAwait(false);
            }

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(timeoutSec));
            try
            {
                return await next(request, cts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cts.IsCancellationRequested)
            {
                return new ResponseMessage { Id = request.Id, Error = new McpError { Code = 408, Message = "Tool Timeout" } };
            }
        }
        finally
        {
            sem.Release();
        }
    }
}


