namespace AxolotlMCP.Client;

/// <summary>
/// 客户端配置选项。
/// </summary>
public sealed class ClientOptions
{
    /// <summary>
    /// 并发请求上限（小于等于 0 表示不限制）。
    /// </summary>
    public int MaxConcurrentRequests { get; set; } = 0;

    /// <summary>
    /// 每个请求的默认超时时长（秒）。
    /// </summary>
    public int DefaultRequestTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// 发送请求失败时的最大重试次数（0 表示不重试）。
    /// 仅在可重试异常（如超时/暂时性）时生效。
    /// </summary>
    public int MaxRetries { get; set; } = 0;

    /// <summary>
    /// 重试的指数退避基数（毫秒），第 n 次重试等待 base * 2^(n-1)。
    /// </summary>
    public int RetryBackoffBaseMs { get; set; } = 200;
}


