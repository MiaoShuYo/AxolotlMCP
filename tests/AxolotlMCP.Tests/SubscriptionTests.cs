using System.Text.Json;
using AxolotlMCP.Core.Protocol;
using AxolotlMCP.Core.Protocol.Message;
using AxolotlMCP.Core.Prompts;
using AxolotlMCP.Core.Resources;
using AxolotlMCP.Core.Tools;
using AxolotlMCP.Server;
using AxolotlMCP.Tests.Fake;
using AxolotlMCP.Core.Transport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AxolotlMCP.Tests;

/// <summary>
/// 订阅变更事件的端到端最小验证。
/// </summary>
public class SubscriptionTests
{
    /// <summary>
    /// 订阅 resources 后，注册资源应触发 resources/changed 通知。
    /// </summary>
    [Fact]
    public async Task Subscribe_Then_Change_Raises_Notification()
    {
        // 准备 DI 与伪传输
        var transport = new TestCaptureTransport();
        await transport.StartAsync();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<ITransport>(sp => transport);
        services.AddSingleton<ToolRegistry>();
        services.AddSingleton<ResourceRegistry>();
        services.AddSingleton<PromptRegistry>();
        services.AddSingleton<IServerNotifier, ServerNotifier>();
        services.AddSingleton<McpServer>(sp => new McpServer(
            sp.GetRequiredService<ITransport>(),
            new DefaultHandler(
                sp.GetRequiredService<ToolRegistry>(),
                sp.GetRequiredService<ResourceRegistry>(),
                sp.GetRequiredService<PromptRegistry>(),
                sp.GetRequiredService<IServerNotifier>(),
                NullLogger<DefaultHandler>.Instance),
            NullLogger<McpServer>.Instance));

        var sp = services.BuildServiceProvider();
        var server = sp.GetRequiredService<McpServer>();
        await server.StartAsync();

        // 订阅 resources
        var subReq = new RequestMessage { Id = "1", Method = "resources/subscribe" };
        transport.EnqueueInbound(JsonSerializer.Serialize(subReq, JsonDefaults.Options));

        // 触发资源变更
        sp.GetRequiredService<ResourceRegistry>().Register(new McpResource { Name = "foo", Description = "bar" });

        // 读取一条发送的通知
        var waited = 0;
        string? sent = null;
        while (waited < 50 && (sent is null))
        {
            if (transport.Sent.TryDequeue(out sent)) break;
            await Task.Delay(20);
            waited++;
        }

        Assert.NotNull(sent);
        if (sent is not null)
        {
            var note = JsonDocument.Parse(sent).RootElement;
            Assert.Equal("resources/changed", note.GetProperty("method").GetString());
        }

        await server.StopAsync();
    }
}


