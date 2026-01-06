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
using AxolotlMCP.Server.Middleware;

namespace AxolotlMCP.Tests;

/// <summary>
/// 订阅变更事件的端到端最小验证。
/// </summary>
public class SubscriptionTests
{
    /// <summary>
    /// 测试专用的 ServerNotifier，直接发送到 transport
    /// </summary>
    private class TestServerNotifier : IServerNotifier
    {
        private readonly ITransport _transport;

        public TestServerNotifier(ITransport transport)
        {
            _transport = transport;
        }

        public async Task NotifyAsync(NotificationMessage notification, CancellationToken cancellationToken = default)
        {
            var json = JsonSerializer.Serialize(notification, JsonDefaults.Options);
            await _transport.SendAsync(json, cancellationToken);
        }
    }

    /// <summary>
    /// 订阅 resources 后，注册资源应触发 resources/changed 通知。
    /// </summary>
    [Fact]
    public async Task Subscribe_Then_Change_Raises_Notification()
    {
        // 准备传输和注册中心
        var transport = new TestCaptureTransport();
        await using (transport)
        {
            await transport.StartAsync();

            var toolRegistry = new ToolRegistry();
            var resourceRegistry = new ResourceRegistry();
            var promptRegistry = new PromptRegistry();
            var lifecycle = new ServerLifecycle();
            var notifier = new TestServerNotifier(transport);

            // 创建 handler 和 server
            var handler = new DefaultHandler(
                toolRegistry,
                resourceRegistry,
                promptRegistry,
                notifier,
                lifecycle,
                NullLogger<DefaultHandler>.Instance);

            var server = new McpServer(
                transport,
                handler,
                NullLogger<McpServer>.Instance,
                new RequestMiddlewarePipeline(Array.Empty<IRequestMiddleware>()));

            await server.StartAsync();

            try
            {
                // 首先初始化服务器
                var initReq = new RequestMessage { Id = "init", Method = "initialize", Params = JsonSerializer.SerializeToElement(new { protocolVersion = "2024-11-05" }, JsonDefaults.Options) };
                transport.EnqueueInbound(JsonSerializer.Serialize(initReq, JsonDefaults.Options));

                // 等待初始化完成
                await Task.Delay(50);

                // 订阅 resources
                var subReq = new RequestMessage { Id = "1", Method = "resources/subscribe" };
                transport.EnqueueInbound(JsonSerializer.Serialize(subReq, JsonDefaults.Options));

                // 等待订阅请求被处理
                await Task.Delay(50);

                // 触发资源变更
                resourceRegistry.Register(new McpResource { Name = "foo", Description = "bar" });

                // 等待通知发送
                await Task.Delay(100);

                // 验证收到了三条消息：init响应 + subscribe响应 + 通知
                var allMessages = transport.Sent.ToArray();
                Assert.True(allMessages.Length >= 3, $"Expected at least 3 messages (init + subscribe + notification), got {allMessages.Length}");

                // 最后一条应该是通知
                var notification = allMessages[^1];
                var note = JsonDocument.Parse(notification).RootElement;
                Assert.Equal("resources/changed", note.GetProperty("method").GetString());
            }
            finally
            {
                // 确保服务器和传输正确停止
                await server.StopAsync();
                await transport.StopAsync();
            }
        }
    }
}


