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
        var transport = new InMemoryTransport();
        await transport.StartAsync();
        var client = new McpClient(transport, NullLogger<McpClient>.Instance, new ClientOptions { MaxConcurrentRequests = 2, DefaultRequestTimeoutSeconds = 1 });
        await client.ConnectAsync();

        var req = new RequestMessage { Method = "ping" };

        var task = client.SendRequestAsync(req);

        // Craft a response by reading pending id from queued send: since InMemoryTransport loops back,
        // we need to simulate server echoing back id. Read one frame and turn it into response.
        await foreach (var sent in transport.ReadAsync())
        {
            var doc = JsonDocument.Parse(sent);
            var root = doc.RootElement;
            var id = root.GetProperty("id").GetString();
            var resp = new ResponseMessage { Id = id!, Result = JsonSerializer.SerializeToElement(new { ok = true }) };
            var respJson = JsonSerializer.Serialize(resp, JsonDefaults.Options);
            await transport.SendAsync(respJson);
            break;
        }

        var response = await task;
        Assert.True(response.Result.HasValue);
        await client.DisconnectAsync();
    }
}


