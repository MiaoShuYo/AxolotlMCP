using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System.Text.Json;
using AxolotlMCP.Core.Protocol;
using AxolotlMCP.Core.Protocol.Message;

namespace AxolotlMCP.Benchmarks;

/// <summary>
/// JSON 序列化与反序列化性能基准测试。
/// </summary>
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net90)]
[MemoryDiagnoser]
[RankColumn]
public class JsonSerializationBenchmarks
{
    private RequestMessage _sampleRequest = null!;
    private ResponseMessage _sampleResponse = null!;
    private string _sampleRequestJson = null!;
    private string _sampleResponseJson = null!;

    [GlobalSetup]
    public void Setup()
    {
        _sampleRequest = new RequestMessage
        {
            Id = "test-id-123",
            Method = "tools/call",
            Params = JsonDocument.Parse("{\"name\":\"test_tool\",\"arguments\":{\"param1\":\"value1\",\"param2\":42}}").RootElement
        };

        _sampleResponse = new ResponseMessage
        {
            Id = "test-id-123",
            Result = JsonDocument.Parse("{\"content\":[{\"type\":\"text\",\"text\":\"Hello World\"}]}").RootElement
        };

        _sampleRequestJson = JsonSerializer.Serialize(_sampleRequest, JsonDefaults.Options);
        _sampleResponseJson = JsonSerializer.Serialize(_sampleResponse, JsonDefaults.Options);
    }

    [Benchmark]
    public string SerializeRequest()
    {
        return JsonSerializer.Serialize(_sampleRequest, JsonDefaults.Options);
    }

    [Benchmark]
    public string SerializeResponse()
    {
        return JsonSerializer.Serialize(_sampleResponse, JsonDefaults.Options);
    }

    [Benchmark]
    public RequestMessage? DeserializeRequest()
    {
        return JsonSerializer.Deserialize<RequestMessage>(_sampleRequestJson, JsonDefaults.Options);
    }

    [Benchmark]
    public ResponseMessage? DeserializeResponse()
    {
        return JsonSerializer.Deserialize<ResponseMessage>(_sampleResponseJson, JsonDefaults.Options);
    }
}
