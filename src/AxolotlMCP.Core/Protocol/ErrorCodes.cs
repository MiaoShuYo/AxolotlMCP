namespace AxolotlMCP.Core.Protocol;

/// <summary>
/// MCP 协议错误码枚举定义，统一管理所有错误码。
/// </summary>
public static class ErrorCodes
{
    // JSON-RPC 2.0 标准错误码（-32768 到 -32000）
    /// <summary>
    /// 解析错误：接收到无效的 JSON。
    /// </summary>
    public const int ParseError = -32700;

    /// <summary>
    /// 无效请求：JSON 格式正确但不符合请求规范。
    /// </summary>
    public const int InvalidRequest = -32600;

    /// <summary>
    /// 方法未找到：请求的方法不存在或不可用。
    /// </summary>
    public const int MethodNotFound = -32601;

    /// <summary>
    /// 无效参数：方法参数无效或缺失。
    /// </summary>
    public const int InvalidParams = -32602;

    /// <summary>
    /// 内部错误：服务器内部发生错误。
    /// </summary>
    public const int InternalError = -32603;

    // MCP 特定错误码（应用层）
    /// <summary>
    /// 未授权：缺少或无效的身份验证凭据。
    /// </summary>
    public const int Unauthorized = 401;

    /// <summary>
    /// 禁止访问：请求被拒绝。
    /// </summary>
    public const int Forbidden = 403;

    /// <summary>
    /// 请求超时：请求处理时间超过限制。
    /// </summary>
    public const int RequestTimeout = 408;

    /// <summary>
    /// 请求频率超限：客户端请求过于频繁。
    /// </summary>
    public const int TooManyRequests = 429;

    /// <summary>
    /// 服务不可用：服务器暂时无法处理请求。
    /// </summary>
    public const int ServiceUnavailable = 503;

    // 业务逻辑错误码（自定义范围：10000+）
    /// <summary>
    /// 工具执行失败：工具调用过程中发生错误。
    /// </summary>
    public const int ToolExecutionFailed = 10001;

    /// <summary>
    /// 资源不存在：请求的资源未找到。
    /// </summary>
    public const int ResourceNotFound = 10002;

    /// <summary>
    /// 提示不存在：请求的提示未找到。
    /// </summary>
    public const int PromptNotFound = 10003;

    /// <summary>
    /// 未初始化：服务器尚未完成初始化。
    /// </summary>
    public const int NotInitialized = 10004;

    /// <summary>
    /// 正在关闭：服务器正在关闭，不接受新请求。
    /// </summary>
    public const int ShuttingDown = 10005;

    /// <summary>
    /// 获取错误码的描述信息。
    /// </summary>
    /// <param name="code">错误码</param>
    /// <returns>错误码描述</returns>
    public static string GetDescription(int code) => code switch
    {
        ParseError => "解析错误",
        InvalidRequest => "无效请求",
        MethodNotFound => "方法未找到",
        InvalidParams => "无效参数",
        InternalError => "内部错误",
        Unauthorized => "未授权",
        Forbidden => "禁止访问",
        RequestTimeout => "请求超时",
        TooManyRequests => "请求频率超限",
        ServiceUnavailable => "服务不可用",
        ToolExecutionFailed => "工具执行失败",
        ResourceNotFound => "资源不存在",
        PromptNotFound => "提示不存在",
        NotInitialized => "未初始化",
        ShuttingDown => "正在关闭",
        _ => "未知错误"
    };
}
