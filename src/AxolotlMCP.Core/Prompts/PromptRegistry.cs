using System.Collections.Concurrent;
using AxolotlMCP.Core.Protocol;

namespace AxolotlMCP.Core.Prompts;

/// <summary>
/// 提示注册中心：负责注册与查询提示模板。
/// </summary>
public sealed class PromptRegistry
{
    private readonly ConcurrentDictionary<string, McpPrompt> _prompts = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 提示变更事件：action = added/removed/updated；name = 提示名。
    /// </summary>
    public event Action<string, string>? OnChanged;

    /// <summary>
    /// 注册提示模板（按名称去重）。
    /// </summary>
    public bool Register(McpPrompt prompt)
    {
        var added = _prompts.TryAdd(prompt.Name, prompt);
        if (added)
        {
            OnChanged?.Invoke("added", prompt.Name);
        }
        return added;
    }

    /// <summary>
    /// 批量注册提示模板。
    /// </summary>
    public int RegisterRange(IEnumerable<McpPrompt> prompts)
    {
        var count = 0;
        foreach (var p in prompts)
        {
            if (Register(p)) count++;
        }
        return count;
    }

    /// <summary>
    /// 获取所有已注册的提示模板。
    /// </summary>
    public IReadOnlyCollection<McpPrompt> GetAll() => _prompts.Values.ToArray();

    /// <summary>
    /// 按名称查找提示模板。
    /// </summary>
    public bool TryGet(string name, out McpPrompt? prompt) => _prompts.TryGetValue(name, out prompt);

    /// <summary>
    /// 移除提示模板。
    /// </summary>
    public bool Remove(string name)
    {
        var removed = _prompts.TryRemove(name, out _);
        if (removed)
        {
            OnChanged?.Invoke("removed", name);
        }
        return removed;
    }

    /// <summary>
    /// 更新（覆盖）提示模板。
    /// </summary>
    public void Upsert(McpPrompt prompt)
    {
        _prompts[prompt.Name] = prompt;
        OnChanged?.Invoke("updated", prompt.Name);
    }
}


