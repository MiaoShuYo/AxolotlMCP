using System.Threading.RateLimiting;
using AxolotlMCP.Core.Observability;
using AxolotlMCP.Core.Protocol.Message;

namespace AxolotlMCP.Server.Middleware;

/// <summary>
/// 简单并发限流中间件（令牌桶）。
/// </summary>
public sealed class RateLimitMiddleware : IRequestMiddleware, IDisposable
{
    private readonly RateLimiter _limiter;

    public RateLimitMiddleware(int permitLimit = 64)
    {
        _limiter = new ConcurrencyLimiter(new ConcurrencyLimiterOptions
        {
            PermitLimit = permitLimit,
            QueueLimit = permitLimit * 4,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        });
    }

    public async Task<ResponseMessage> InvokeAsync(RequestMessage request, CancellationToken cancellationToken, Func<RequestMessage, CancellationToken, Task<ResponseMessage>> next)
    {
        using var lease = await _limiter.AcquireAsync(1, cancellationToken).ConfigureAwait(false);
        if (!lease.IsAcquired)
        {
            using var act = McpTracing.Source.StartActivity("mcp.middleware.ratelimit", System.Diagnostics.ActivityKind.Internal);
            act?.SetTag("middleware.name", "ratelimit");
            act?.SetTag("middleware.success", false);
            return new ResponseMessage
            {
                Id = request.Id,
                Error = new Core.Protocol.Message.McpError { Code = 429, Message = "Too Many Requests" }
            };
        }

        return await next(request, cancellationToken).ConfigureAwait(false);
    }

    public void Dispose()
    {
        _limiter.Dispose();
    }
}


