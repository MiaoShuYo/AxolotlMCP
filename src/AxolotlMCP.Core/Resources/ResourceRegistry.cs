using System.Collections.Concurrent;
using AxolotlMCP.Core.Protocol;

namespace AxolotlMCP.Core.Resources;

/// <summary>
/// 资源注册中心：负责注册与查询资源描述。
/// </summary>
public sealed class ResourceRegistry
{
    private readonly ConcurrentDictionary<string, McpResource> _resources = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 资源变更事件：action = added/removed/updated；name = 资源名。
    /// </summary>
    public event Action<string, string>? OnChanged;

    /// <summary>
    /// 注册资源（按名称去重）。
    /// </summary>
    public bool Register(McpResource resource)
    {
        var added = _resources.TryAdd(resource.Name, resource);
        if (added)
        {
            OnChanged?.Invoke("added", resource.Name);
        }
        return added;
    }

    /// <summary>
    /// 批量注册。
    /// </summary>
    public int RegisterRange(IEnumerable<McpResource> resources)
    {
        var count = 0;
        foreach (var r in resources)
        {
            if (Register(r)) count++;
        }
        return count;
    }

    /// <summary>
    /// 移除资源。
    /// </summary>
    public bool Remove(string name)
    {
        var removed = _resources.TryRemove(name, out _);
        if (removed)
        {
            OnChanged?.Invoke("removed", name);
        }
        return removed;
    }

    /// <summary>
    /// 更新（覆盖）资源定义。
    /// </summary>
    public void Upsert(McpResource resource)
    {
        _resources[resource.Name] = resource;
        OnChanged?.Invoke("updated", resource.Name);
    }

    /// <summary>
    /// 获取所有已注册的资源。
    /// </summary>
    public IReadOnlyCollection<McpResource> GetAll() => _resources.Values.ToArray();

    /// <summary>
    /// 按名称查找资源。
    /// </summary>
    public bool TryGet(string name, out McpResource? resource) => _resources.TryGetValue(name, out resource);
}


