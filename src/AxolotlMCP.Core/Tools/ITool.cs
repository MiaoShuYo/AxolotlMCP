using System.Text.Json;

namespace AxolotlMCP.Core.Tools;

/// <summary>
/// 表示一个可被 MCP 调用的工具。
/// </summary>
public interface ITool
{
    /// <summary>
    /// 工具名称（唯一标识）。
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 工具描述。
    /// </summary>
    string Description { get; }

    /// <summary>
    /// 输入参数的 JSON Schema。
    /// </summary>
    JsonSchema InputSchema { get; }

    /// <summary>
    /// 执行工具逻辑。
    /// </summary>
    /// <param name="arguments">输入参数 JSON 对象</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>执行结果 JSON 对象</returns>
    Task<JsonElement> ExecuteAsync(JsonElement arguments, CancellationToken cancellationToken = default);
}


