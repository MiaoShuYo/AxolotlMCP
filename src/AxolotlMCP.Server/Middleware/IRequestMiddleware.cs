using AxolotlMCP.Core.Protocol.Message;

namespace AxolotlMCP.Server.Middleware;

/// <summary>
/// 请求中间件：用于在请求处理前后插入横切逻辑。
/// </summary>
public interface IRequestMiddleware
{
    /// <summary>
    /// 执行中间件逻辑。
    /// </summary>
    /// <param name="request">请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <param name="next">下一个委托</param>
    Task<ResponseMessage> InvokeAsync(RequestMessage request, CancellationToken cancellationToken, Func<RequestMessage, CancellationToken, Task<ResponseMessage>> next);
}


