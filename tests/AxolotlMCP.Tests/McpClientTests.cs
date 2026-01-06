using System.Text.Json;
using AxolotlMCP.Client;
using AxolotlMCP.Core.Protocol;
using AxolotlMCP.Core.Protocol.Message;
using AxolotlMCP.Tests.Fake;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AxolotlMCP.Tests;

/// <summary>
/// 针对 <see cref="McpClient"/> 的基础行为测试。
/// </summary>
public class McpClientTests
{
    /// <summary>
    /// 验证发送请求后分配 Id，并能正确接收回包并关联。
    /// </summary>
    [Fact]
    public async Task SendRequest_AssignsId_And_ReceivesResponse()
    {
        var transport = new TestCaptureTransport();
        await transport.StartAsync();
        var client = new McpClient(transport, NullLogger<McpClient>.Instance, new ClientOptions { MaxConcurrentRequests = 2, DefaultRequestTimeoutSeconds = 5 });
        await client.ConnectAsync();

        var req = new RequestMessage { Method = "ping" };

        var task = client.SendRequestAsync(req);

        // 等待客户端发送请求
        await Task.Delay(100);

        // 从发送队列获取请求并提取ID
        Assert.True(transport.Sent.TryDequeue(out var sentJson));
        var doc = JsonDocument.Parse(sentJson);
        var root = doc.RootElement;
        var id = root.GetProperty("id").GetString();

        // 模拟服务器响应
        var resp = new ResponseMessage { Id = id!, Result = JsonSerializer.SerializeToElement(new { ok = true }) };
        var respJson = JsonSerializer.Serialize(resp, JsonDefaults.Options);
        transport.EnqueueInbound(respJson);

        var response = await task;
        Assert.True(response.Result.HasValue);
        await client.DisconnectAsync();
        await transport.StopAsync();
        await transport.DisposeAsync();
    }
}


