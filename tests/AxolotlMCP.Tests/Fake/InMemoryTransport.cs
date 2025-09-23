using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using AxolotlMCP.Core.Transport;

namespace AxolotlMCP.Tests.Fake;

internal sealed class InMemoryTransport : ITransport
{
    private readonly ConcurrentQueue<string> _incoming = new();
    private readonly SemaphoreSlim _signal = new(0);
    private volatile bool _running;

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        _running = true;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        _running = false;
        _signal.Release();
        return Task.CompletedTask;
    }

    public Task SendAsync(string jsonPayload, CancellationToken cancellationToken = default)
    {
        // Loopback for tests: enqueue to incoming as if peer responded.
        _incoming.Enqueue(jsonPayload);
        _signal.Release();
        return Task.CompletedTask;
    }

    public async IAsyncEnumerable<string> ReadAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while (_running && !cancellationToken.IsCancellationRequested)
        {
            if (_incoming.TryDequeue(out var msg))
            {
                yield return msg;
                continue;
            }
            await _signal.WaitAsync(TimeSpan.FromMilliseconds(50), cancellationToken);
        }
    }

    public ValueTask DisposeAsync()
    {
        _running = false;
        _signal.Dispose();
        return ValueTask.CompletedTask;
    }
}


