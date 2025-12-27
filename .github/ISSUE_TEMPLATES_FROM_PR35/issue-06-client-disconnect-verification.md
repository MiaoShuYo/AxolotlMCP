---
title: "[BUG] McpClient.DisconnectAsync Doesn't Verify Read Loop Completion"
labels: bug, medium-priority, client
---

## Bug Description

`McpClient.DisconnectAsync` uses `Task.WhenAny` to wait for the read loop but doesn't verify if it actually completed, same issue as McpServer.

## Location

**File**: `src/AxolotlMCP.Client/McpClient.cs:78`

## Current Code

```csharp
await Task.WhenAny(_readLoopTask, Task.Delay(TimeSpan.FromSeconds(2), cancellationToken)).ConfigureAwait(false);
```

## Problem

Identical to the server-side issue:
- `Task.WhenAny` returns when ANY task completes
- No verification of which task finished
- Potential race conditions if delay completes first
- Resources may not be properly cleaned up

## Impact

- ⚠️ Resources may not be properly cleaned up
- ⚠️ Potential race conditions during disconnect
- ⚠️ Inconsistent client shutdown behavior
- ⚠️ Difficult to debug connection issues

## Suggested Fix

```csharp
var completed = await Task.WhenAny(_readLoopTask, Task.Delay(TimeSpan.FromSeconds(2), cancellationToken)).ConfigureAwait(false);
if (completed != _readLoopTask)
{
    _logger.LogWarning("Read loop did not complete within timeout, forcing disconnect");
}
else
{
    _logger.LogInformation("Client disconnected gracefully");
}
```

## Priority

🟡 **MEDIUM** - Affects reliability but has workarounds

## Testing Recommendations

1. Add test for graceful disconnect (read loop completes in time)
2. Add test for timeout scenario (read loop takes too long)
3. Add test for resource cleanup verification
4. Test reconnection after forced disconnect

## Related Issues

- Related to Issue #5 (McpServer has same problem)

---
*Discovered during code review in PR #35*
