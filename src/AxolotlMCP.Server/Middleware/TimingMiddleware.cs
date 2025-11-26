using System.Diagnostics;
using AxolotlMCP.Core.Observability;
using AxolotlMCP.Core.Protocol.Message;

namespace AxolotlMCP.Server.Middleware;

/// <summary>
/// 记录请求耗时的示例中间件。
/// </summary>
public sealed class TimingMiddleware : IRequestMiddleware
{
    /// <summary>
    /// 处理请求并记录其处理时间。
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public async Task<ResponseMessage> InvokeAsync(RequestMessage request, CancellationToken cancellationToken, Func<RequestMessage, CancellationToken, Task<ResponseMessage>> next)
    {
        var sw = Stopwatch.StartNew();
        using var act = McpTracing.Source.StartActivity("mcp.middleware.timing", System.Diagnostics.ActivityKind.Internal);
        act?.SetTag("middleware.name", "timing");
        act?.SetTag("mcp.method", request.Method);
        try
        {
            var resp = await next(request, cancellationToken).ConfigureAwait(false);
            act?.SetTag("middleware.success", true);
            return resp;
        }
        catch (Exception ex)
        {
            act?.SetTag("middleware.success", false);
            act?.SetTag("exception.type", ex.GetType().FullName);
            act?.SetTag("exception.message", ex.Message);
            throw;
        }
        finally
        {
            sw.Stop();
            act?.SetTag("duration.ms", sw.Elapsed.TotalMilliseconds);
        }
    }
}


