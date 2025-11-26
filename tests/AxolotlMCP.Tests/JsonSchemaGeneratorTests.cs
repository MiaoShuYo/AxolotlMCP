using System.ComponentModel.DataAnnotations;
using AxolotlMCP.Core.Protocol;
using Xunit;

namespace AxolotlMCP.Tests;

/// <summary>
/// JsonSchemaGenerator 基础能力测试。
/// </summary>
public class JsonSchemaGeneratorTests
{
    private class SampleDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public int? Count { get; set; }
        public List<string>? Tags { get; set; }
    }

    /// <summary>
    /// 测试生成包含必填字段和数组字段的对象的 JSON Schema。
    /// </summary>
    [Fact]
    public void Generate_Object_With_Required_And_Array()
    {
        var schema = JsonSchemaGenerator.Generate(typeof(SampleDto));
        Assert.Equal("object", schema.Type);
        Assert.NotNull(schema.Properties);
        Assert.True(schema.Properties!.ContainsKey("Name"));
        Assert.True(schema.Required!.Contains("Name"));
        Assert.Equal("integer", schema.Properties["Count"].Type);
        Assert.Equal("array", schema.Properties["Tags"].Type);
        Assert.Equal("string", schema.Properties["Tags"].Items!.Type);
    }
}


