using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using AxolotlMCP.Core.Tools;
using System.Text.Json;

namespace AxolotlMCP.Benchmarks;

/// <summary>
/// JsonSchema 生成器性能基准测试。
/// </summary>
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net90)]
[MemoryDiagnoser]
[RankColumn]
public class JsonSchemaGeneratorBenchmarks
{
    public class SimpleDto
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
    }

    public class ComplexDto
    {
        public string Id { get; set; } = "";
        public List<string> Tags { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
        public NestedDto? Nested { get; set; }
    }

    public class NestedDto
    {
        public string Value { get; set; } = "";
        public int Count { get; set; }
    }

    [Benchmark]
    public Core.Protocol.JsonSchema GenerateSimpleSchema()
    {
        return Core.Protocol.JsonSchemaGenerator.Generate(typeof(SimpleDto));
    }

    [Benchmark]
    public Core.Protocol.JsonSchema GenerateComplexSchema()
    {
        return Core.Protocol.JsonSchemaGenerator.Generate(typeof(ComplexDto));
    }

    [Benchmark]
    public string SerializeSimpleSchema()
    {
        var schema = Core.Protocol.JsonSchemaGenerator.Generate(typeof(SimpleDto));
        return JsonSerializer.Serialize(schema);
    }

    [Benchmark]
    public string SerializeComplexSchema()
    {
        var schema = Core.Protocol.JsonSchemaGenerator.Generate(typeof(ComplexDto));
        return JsonSerializer.Serialize(schema);
    }
}
