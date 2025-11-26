using AxolotlMCP.Core.Protocol.Message;

namespace AxolotlMCP.Server.Middleware;

/// <summary>
/// 请求中间件管道：顺序执行已注册的中间件，最后调用处理器。
/// </summary>
public sealed class RequestMiddlewarePipeline
{
    private readonly IReadOnlyList<IRequestMiddleware> _middlewares;

    /// <summary>
    /// 初始化请求中间件管道。
    /// </summary>
    /// <param name="middlewares"></param>
    public RequestMiddlewarePipeline(IEnumerable<IRequestMiddleware> middlewares)
    {
        _middlewares = middlewares.ToList();
    }
    /// <summary>
    /// 执行请求中间件管道。
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="terminal"></param>
    /// <returns></returns>
    public Task<ResponseMessage> ExecuteAsync(RequestMessage request, CancellationToken cancellationToken, Func<RequestMessage, CancellationToken, Task<ResponseMessage>> terminal)
    {
        Func<RequestMessage, CancellationToken, Task<ResponseMessage>> next = terminal;
        for (var i = _middlewares.Count - 1; i >= 0; i--)
        {
            var current = _middlewares[i];
            var capturedNext = next;
            next = (req, ct) => current.InvokeAsync(req, ct, capturedNext);
        }
        return next(request, cancellationToken);
    }
}


