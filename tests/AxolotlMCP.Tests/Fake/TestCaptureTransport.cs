using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using AxolotlMCP.Core.Transport;

namespace AxolotlMCP.Tests.Fake;

internal sealed class TestCaptureTransport : ITransport
{
    private readonly BlockingCollection<string> _inbound = new();
    public readonly ConcurrentQueue<string> Sent = new();
    private volatile bool _running;

    public void EnqueueInbound(string json) => _inbound.Add(json);

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        _running = true;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        _running = false;
        _inbound.CompleteAdding();
        return Task.CompletedTask;
    }

    public Task SendAsync(string jsonPayload, CancellationToken cancellationToken = default)
    {
        Sent.Enqueue(jsonPayload);
        return Task.CompletedTask;
    }

    public async IAsyncEnumerable<string> ReadAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while (_running && !_inbound.IsCompleted && !cancellationToken.IsCancellationRequested)
        {
            string? item = null;
            try
            {
                item = await Task.Run(() => _inbound.Take(cancellationToken), cancellationToken).ConfigureAwait(false);
            }
            catch
            {
            }
            if (item is not null)
            {
                yield return item;
            }
        }
    }

    public ValueTask DisposeAsync()
    {
        _running = false;
        _inbound.Dispose();
        return ValueTask.CompletedTask;
    }
}


