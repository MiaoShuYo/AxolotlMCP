---
title: "[BUG] McpServer.StopAsync Doesn't Verify Read Loop Completion"
labels: bug, medium-priority, server
---

## Bug Description

`McpServer.StopAsync` uses `Task.WhenAny` to wait for the read loop but doesn't verify if it actually completed, potentially leaving resources uncleaned.

## Location

**File**: `src/AxolotlMCP.Server/McpServer.cs:61`

## Current Code

```csharp
await Task.WhenAny(_readLoopTask, Task.Delay(TimeSpan.FromSeconds(2), cancellationToken)).ConfigureAwait(false);
```

## Problem

`Task.WhenAny` returns when ANY task completes, but the code doesn't check which task finished:
- If the delay completes first, `_readLoopTask` is still running
- The transport gets stopped while the read loop might still be active
- Potential race conditions and resource leaks

## Impact

- ⚠️ Resources may not be properly cleaned up
- ⚠️ Potential race conditions between shutdown and active read
- ⚠️ Difficult to debug shutdown issues
- ⚠️ Inconsistent shutdown behavior

## Suggested Fix

```csharp
var completed = await Task.WhenAny(_readLoopTask, Task.Delay(TimeSpan.FromSeconds(2), cancellationToken)).ConfigureAwait(false);
if (completed != _readLoopTask)
{
    _logger.LogWarning("Read loop did not complete within timeout, forcing shutdown");
}
else
{
    _logger.LogInformation("Read loop completed gracefully");
}
```

## Priority

🟡 **MEDIUM** - Affects reliability but has workarounds

## Testing Recommendations

1. Add test for graceful shutdown (read loop completes in time)
2. Add test for timeout scenario (read loop takes too long)
3. Add test for resource cleanup verification
4. Monitor for race conditions in integration tests

## Related Issues

- Related to Issue #6 (McpClient has same problem)

---
*Discovered during code review in PR #35*
