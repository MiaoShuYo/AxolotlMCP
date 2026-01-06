using System.Text;
using System.Runtime.CompilerServices;

namespace AxolotlMCP.Core.Transport;

/// <summary>
/// 基于标准输入/输出的传输实现，兼容 FastMCP 等生态的 stdio 交互。
/// 一条消息为一行（以换行结尾）的 JSON 文本。
/// </summary>
public sealed class StdioTransport : ITransport
{
    private readonly TextReader _input;
    private readonly TextWriter _output;
    private readonly Encoding _encoding;
    private volatile bool _running;

    /// <summary>
    /// 使用控制台标准输入/输出初始化传输。
    /// </summary>
    public StdioTransport()
        : this(Console.In, Console.Out, new UTF8Encoding(false)) { }

    /// <summary>
    /// 使用给定的输入、输出与编码初始化传输。
    /// </summary>
    /// <param name="input">输入流（逐行读取JSON）</param>
    /// <param name="output">输出写入器（逐行写入JSON）</param>
    /// <param name="encoding">文本编码（默认UTF-8无BOM）</param>
    public StdioTransport(TextReader input, TextWriter output, Encoding? encoding = null)
    {
        _input = input;
        _output = output;
        _encoding = encoding ?? new UTF8Encoding(false);
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        _running = true;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        _running = false;
        await _output.FlushAsync();
    }

    /// <inheritdoc />
    public async Task SendAsync(string jsonPayload, CancellationToken cancellationToken = default)
    {
        if (!_running) throw new InvalidOperationException("Transport is not running.");
        await _output.WriteLineAsync(jsonPayload.AsMemory());
        await _output.FlushAsync();
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<string> ReadAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while (_running && !cancellationToken.IsCancellationRequested)
        {
            var line = await _input.ReadLineAsync(cancellationToken);
            if (line is null)
            {
                yield break; // EOF
            }
            if (line.Length == 0)
            {
                continue; // 跳过空行
            }
            yield return line;
        }
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        _running = false;
        return ValueTask.CompletedTask;
    }
}


