using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using AxolotlMCP.Core.Tools;

namespace AxolotlMCP.Benchmarks;

/// <summary>
/// 工具注册与查询性能基准测试。
/// </summary>
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net90)]
[MemoryDiagnoser]
[RankColumn]
public class ToolRegistryBenchmarks
{
    private ToolRegistry _registry = null!;
    private const int ToolCount = 100;

    [GlobalSetup]
    public void Setup()
    {
        _registry = new ToolRegistry();
        
        // 注册多个工具
        for (int i = 0; i < ToolCount; i++)
        {
            var toolName = $"tool_{i}";
            _registry.Register(new BenchmarkTool(toolName, $"Test tool {i}"));
        }
    }

    private sealed class BenchmarkTool : ITool
    {
        public BenchmarkTool(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public string Name { get; }
        public string Description { get; }
        public Core.Protocol.JsonSchema InputSchema => new()
        {
            Type = "object",
            Properties = new Dictionary<string, Core.Protocol.JsonSchema>
            {
                ["param"] = new Core.Protocol.JsonSchema { Type = "string" }
            }
        };

        public Task<System.Text.Json.JsonElement> ExecuteAsync(System.Text.Json.JsonElement arguments, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(System.Text.Json.JsonSerializer.SerializeToElement(new { result = Name }));
        }
    }

    [Benchmark]
    public bool TryGetFirst()
    {
        return _registry.TryGet("tool_0", out _);
    }

    [Benchmark]
    public bool TryGetMiddle()
    {
        return _registry.TryGet("tool_50", out _);
    }

    [Benchmark]
    public bool TryGetLast()
    {
        return _registry.TryGet($"tool_{ToolCount - 1}", out _);
    }

    [Benchmark]
    public bool TryGetNonExistent()
    {
        return _registry.TryGet("non_existent_tool", out _);
    }

    [Benchmark]
    public int GetAllTools()
    {
        return _registry.GetAll().Count();
    }
}
