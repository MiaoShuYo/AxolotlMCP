using System.Text.Json;
using AxolotlMCP.Core.Prompts;
using AxolotlMCP.Core.Protocol;
using AxolotlMCP.Core.Resources;
using AxolotlMCP.Core.Tools;
using AxolotlMCP.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AxolotlMCP.Tests;

/// <summary>
/// 验证资源/提示 list 接口的返回结构。
/// </summary>
public class ResourcesPromptsTests
{
    private sealed class NoopNotifier : IServerNotifier
    {
        public Task NotifyAsync(AxolotlMCP.Core.Protocol.Message.NotificationMessage notification, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    /// <summary>
    /// 验证 resources/list 与 prompts/list 的返回结构包含数组字段。
    /// </summary>
    [Fact]
    public async Task ListEndpoints_ReturnExpectedShapes()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ToolRegistry>();
        services.AddSingleton<ResourceRegistry>();
        services.AddSingleton<PromptRegistry>();
        services.AddLogging();

        var sp = services.BuildServiceProvider();
        var handler = new DefaultHandler(
            sp.GetRequiredService<ToolRegistry>(),
            sp.GetRequiredService<ResourceRegistry>(),
            sp.GetRequiredService<PromptRegistry>(),
            new NoopNotifier(),
            NullLogger<DefaultHandler>.Instance);

        // 资源与提示注册
        sp.GetRequiredService<ResourceRegistry>().Register(new McpResource { Name = "r1", Description = "d" });
        sp.GetRequiredService<PromptRegistry>().Register(new McpPrompt { Name = "p1", Description = "d", Arguments = Array.Empty<JsonSchema>() });

        // resources/list
        var resResp = await handler.HandleRequestAsync(new AxolotlMCP.Core.Protocol.Message.RequestMessage { Method = "resources/list", Id = "1" });
        Assert.True(resResp.Result.HasValue);
        var resDoc = JsonDocument.Parse(resResp.Result.Value.GetRawText());
        Assert.True(resDoc.RootElement.TryGetProperty("resources", out var resourcesEl));
        Assert.True(resourcesEl.ValueKind == JsonValueKind.Array);

        // prompts/list
        var prResp = await handler.HandleRequestAsync(new AxolotlMCP.Core.Protocol.Message.RequestMessage { Method = "prompts/list", Id = "2" });
        Assert.True(prResp.Result.HasValue);
        var prDoc = JsonDocument.Parse(prResp.Result.Value.GetRawText());
        Assert.True(prDoc.RootElement.TryGetProperty("prompts", out var promptsEl));
        Assert.True(promptsEl.ValueKind == JsonValueKind.Array);
    }
}


