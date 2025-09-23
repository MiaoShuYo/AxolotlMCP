using System.Collections.Concurrent;

namespace AxolotlMCP.Core.Tools;

/// <summary>
/// 工具注册中心：负责注册与查询工具。
/// </summary>
public sealed class ToolRegistry
{
    private readonly ConcurrentDictionary<string, ITool> _tools = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 注册工具（按名称去重）。
    /// </summary>
    public bool Register(ITool tool)
    {
        return _tools.TryAdd(tool.Name, tool);
    }

    /// <summary>
    /// 批量注册。
    /// </summary>
    public int RegisterRange(IEnumerable<ITool> tools)
    {
        var count = 0;
        foreach (var t in tools)
        {
            if (Register(t)) count++;
        }
        return count;
    }

    /// <summary>
    /// 获取所有已注册的工具。
    /// </summary>
    public IReadOnlyCollection<ITool> GetAll() => _tools.Values.ToArray();

    /// <summary>
    /// 按名称查找工具。
    /// </summary>
    public bool TryGet(string name, out ITool? tool) => _tools.TryGetValue(name, out tool);
}


