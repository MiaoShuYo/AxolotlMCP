using System.Buffers;
using System.Net.WebSockets;
using System.Text;
using System.Runtime.CompilerServices;

namespace AxolotlMCP.Core.Transport;

/// <summary>
/// 基于 WebSocket 的传输实现（文本帧，UTF-8）。
/// </summary>
public sealed class WebSocketTransport : ITransport
{
    private readonly Uri _endpoint;
    private readonly ClientWebSocket _socket = new();
    private readonly Encoding _encoding;
    private volatile bool _running;

    /// <summary>
    /// 创建 WebSocket 传输。
    /// </summary>
    /// <param name="endpoint">WebSocket 服务端地址（ws:// 或 wss://）。</param>
    /// <param name="encoding">文本编码（默认 UTF-8 无 BOM）。</param>
    public WebSocketTransport(string endpoint, Encoding? encoding = null)
    {
        _endpoint = new Uri(endpoint);
        _encoding = encoding ?? new UTF8Encoding(false);
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_socket.State == WebSocketState.Open) { _running = true; return; }
        await _socket.ConnectAsync(_endpoint, cancellationToken).ConfigureAwait(false);
        _running = true;
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        _running = false;
        if (_socket.State == WebSocketState.Open)
        {
            try { await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "shutdown", cancellationToken).ConfigureAwait(false); } catch { }
        }
    }

    /// <inheritdoc />
    public async Task SendAsync(string jsonPayload, CancellationToken cancellationToken = default)
    {
        if (!_running || _socket.State != WebSocketState.Open) throw new InvalidOperationException("WebSocket is not open.");
        var bytes = _encoding.GetBytes(jsonPayload);
        var segment = new ArraySegment<byte>(bytes);
        await _socket.SendAsync(segment, WebSocketMessageType.Text, true, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<string> ReadAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var buffer = new byte[64 * 1024];
        var builder = new ArrayBufferWriter<byte>();

        while (_running && _socket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
        {
            builder.Clear();
            WebSocketReceiveResult? result;
            do
            {
                var seg = new ArraySegment<byte>(buffer);
                result = await _socket.ReceiveAsync(seg, cancellationToken).ConfigureAwait(false);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await StopAsync(cancellationToken).ConfigureAwait(false);
                    yield break;
                }
                builder.Write(seg.AsSpan(0, result.Count));
            } while (!result.EndOfMessage);

            if (result.MessageType == WebSocketMessageType.Text)
            {
                var text = _encoding.GetString(builder.WrittenSpan);
                if (text.Length > 0)
                {
                    yield return text;
                }
            }
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        _running = false;
        try { _socket.Dispose(); } catch { }
        await Task.CompletedTask;
    }
}


