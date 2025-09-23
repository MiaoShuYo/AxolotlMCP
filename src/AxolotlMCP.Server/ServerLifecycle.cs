namespace AxolotlMCP.Server;

/// <summary>
/// 服务端生命周期状态机：initialize → (running) → shutdown → exit。
/// </summary>
public sealed class ServerLifecycle
{
    private int _initialized; // 0/1
    private int _shuttingDown; // 0/1
    private int _exitRequested; // 0/1（通知）

    /// <summary>
    /// 是否已完成初始化。
    /// </summary>
    public bool IsInitialized => Interlocked.CompareExchange(ref _initialized, 0, 0) == 1;

    /// <summary>
    /// 是否处于关停阶段。
    /// </summary>
    public bool IsShuttingDown => Interlocked.CompareExchange(ref _shuttingDown, 0, 0) == 1;

    /// <summary>
    /// 是否已收到 exit 通知。
    /// </summary>
    public bool ExitRequested => Interlocked.CompareExchange(ref _exitRequested, 0, 0) == 1;

    /// <summary>
    /// 尝试初始化，若已初始化将返回 false。
    /// </summary>
    public bool TryInitialize()
    {
        return Interlocked.Exchange(ref _initialized, 1) == 0;
    }

    /// <summary>
    /// 尝试关停，如果尚未初始化也允许进入关停态（返回 true），多次调用幂等。
    /// </summary>
    public bool TryShutdown()
    {
        return Interlocked.Exchange(ref _shuttingDown, 1) == 0;
    }

    /// <summary>
    /// 标记已收到 exit 通知（幂等）。
    /// </summary>
    public void MarkExitRequested()
    {
        Interlocked.Exchange(ref _exitRequested, 1);
    }
}


