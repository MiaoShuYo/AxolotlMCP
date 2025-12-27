---
title: "[BUG] Memory Leak from Event Subscriptions in DefaultHandler"
labels: bug, high-priority, memory-leak, server
---

## Bug Description

Event handlers are added in `HandleResourcesSubscribeAsync` and `HandlePromptsSubscribeAsync` but never removed, leading to memory leaks and duplicate event handlers.

## Location

**File**: `src/AxolotlMCP.Server/DefaultHandler.cs` (lines 156 and 172)

## Current Code

```csharp
// In HandleResourcesSubscribeAsync (line 156)
_resources.OnChanged += async (action, name) => { ... };

// In HandlePromptsSubscribeAsync (line 172)
_prompts.OnChanged += async (action, name) => { ... };
```

## Problem

Each call to these subscribe methods adds a new event handler without:
1. Checking if already subscribed
2. Providing unsubscribe mechanism
3. Removing handlers when client disconnects

## Impact

- ❌ Multiple subscriptions lead to duplicate event handlers
- ❌ Memory leak - handlers never garbage collected
- ❌ Same notification sent multiple times to client
- ❌ Performance degradation over time
- ❌ Potential DoS if client repeatedly subscribes

## Suggested Fix Options

### Option 1: Subscription Flag
```csharp
private bool _resourcesSubscribed = false;

public async Task<SubscribeResponse> HandleResourcesSubscribeAsync(...)
{
    if (!_resourcesSubscribed)
    {
        _resources.OnChanged += async (action, name) => { ... };
        _resourcesSubscribed = true;
    }
    // ...
}
```

### Option 2: Unsubscribe Mechanism
```csharp
private EventHandler<(string action, string name)>? _resourcesHandler;

public async Task<SubscribeResponse> HandleResourcesSubscribeAsync(...)
{
    if (_resourcesHandler == null)
    {
        _resourcesHandler = async (action, name) => { ... };
        _resources.OnChanged += _resourcesHandler;
    }
    // ...
}

// Add unsubscribe method or cleanup in Dispose
public void UnsubscribeFromResources()
{
    if (_resourcesHandler != null)
    {
        _resources.OnChanged -= _resourcesHandler;
        _resourcesHandler = null;
    }
}
```

### Option 3: Weak References
Use weak event pattern to prevent memory leaks.

## Priority

🔴 **HIGH** - Memory leak that worsens over time

## Testing Recommendations

1. Add test that subscribes multiple times and verifies single handler
2. Add memory profiling test to detect leaks
3. Add test for duplicate notification prevention
4. Test handler cleanup on client disconnect

## Related Issues

None

---
*Discovered during code review in PR #35*
