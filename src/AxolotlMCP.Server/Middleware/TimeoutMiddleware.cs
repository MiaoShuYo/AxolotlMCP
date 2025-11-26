using AxolotlMCP.Core.Protocol.Message;
using Microsoft.Extensions.Options;

namespace AxolotlMCP.Server.Middleware;

/// <summary>
/// 单请求超时中间件（基于 SecurityOptions.RequestTimeoutSeconds）。
/// </summary>
public sealed class TimeoutMiddleware : IRequestMiddleware
{
    private readonly int _timeoutSeconds;

    /// <summary>
    /// 初始化超时中间件。
    /// </summary>
    /// <param name="options"></param>
    public TimeoutMiddleware(IOptions<SecurityOptions> options)
    {
        _timeoutSeconds = options.Value.RequestTimeoutSeconds;
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
        if (_timeoutSeconds <= 0)
        {
            return await next(request, cancellationToken).ConfigureAwait(false);
        }

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(_timeoutSeconds));
        try
        {
            return await next(request, cts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested)
        {
            return new ResponseMessage
            {
                Id = request.Id,
                Error = new McpError { Code = 408, Message = "Request Timeout" }
            };
        }
    }
}


