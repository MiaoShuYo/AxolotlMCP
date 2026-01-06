using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Text;

namespace AxolotlMCP.Core.Transport;

/// <summary>
/// 基于 Windows 命名管道的传输实现（按行 JSON 文本）。
/// </summary>
public sealed class NamedPipeTransport : ITransport
{
    private readonly string _serverName;
    private readonly string _pipeName;
    private readonly Encoding _encoding;
    private NamedPipeClientStream? _client;
    private StreamReader? _reader;
    private StreamWriter? _writer;
    private volatile bool _running;

    /// <summary>
    /// 创建命名管道传输。
    /// </summary>
    /// <param name="pipeName">管道名称。</param>
    /// <param name="serverName">服务器名（默认 "."）。</param>
    /// <param name="encoding">编码（默认 UTF-8 无 BOM）。</param>
    public NamedPipeTransport(string pipeName, string serverName = ".", Encoding? encoding = null)
    {
        _pipeName = pipeName;
        _serverName = serverName;
        _encoding = encoding ?? new UTF8Encoding(false);
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_client is { IsConnected: true }) { _running = true; return; }
        _client = new NamedPipeClientStream(_serverName, _pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        await _client.ConnectAsync(cancellationToken).ConfigureAwait(false);
        _reader = new StreamReader(_client, _encoding, detectEncodingFromByteOrderMarks: false, bufferSize: 8192, leaveOpen: true);
        _writer = new StreamWriter(_client, _encoding, bufferSize: 8192, leaveOpen: true) { AutoFlush = true };
        _running = true;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        _running = false;
        try { _writer?.Flush(); } catch { }
        try { _client?.Dispose(); } catch { }
        _reader = null;
        _writer = null;
        _client = null;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task SendAsync(string jsonPayload, CancellationToken cancellationToken = default)
    {
        if (!_running || _writer is null) throw new InvalidOperationException("Pipe is not running.");
        await _writer.WriteLineAsync(jsonPayload.AsMemory()).ConfigureAwait(false);
        await _writer.FlushAsync().ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<string> ReadAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (_reader is null) yield break;
        while (_running && !cancellationToken.IsCancellationRequested)
        {
            string? line;
            try
            {
                line = await _reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (IOException)
            {
                yield break;
            }
            if (line is null) yield break;
            if (line.Length == 0) continue;
            yield return line;
        }
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        _running = false;
        try { _client?.Dispose(); } catch { }
        return ValueTask.CompletedTask;
    }
}


