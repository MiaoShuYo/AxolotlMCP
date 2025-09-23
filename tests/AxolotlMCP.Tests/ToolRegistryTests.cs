using AxolotlMCP.Core.Tools;
using Xunit;

namespace AxolotlMCP.Tests;

/// <summary>
/// 针对工具注册中心的基础测试。
/// </summary>
public class ToolRegistryTests
{
    private sealed class DummyTool : ITool
    {
        public string Name => "dummy";
        public string Description => "d";
        public Core.Protocol.JsonSchema InputSchema => new();
        public Task<System.Text.Json.JsonElement> ExecuteAsync(System.Text.Json.JsonElement arguments, CancellationToken cancellationToken = default)
            => Task.FromResult(System.Text.Json.JsonSerializer.SerializeToElement(new { ok = true }));
    }

    /// <summary>
    /// 验证注册与获取工具成功。
    /// </summary>
    [Fact]
    public void Register_And_Get()
    {
        var reg = new ToolRegistry();
        Assert.True(reg.Register(new DummyTool()));
        Assert.True(reg.TryGet("dummy", out var tool));
        Assert.NotNull(tool);
    }
}


