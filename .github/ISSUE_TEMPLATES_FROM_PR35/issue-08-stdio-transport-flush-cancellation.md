---
title: "[BUG] Missing CancellationToken in StdioTransport.StopAsync"
labels: bug, medium-priority, transport
---

## Bug Description

`StdioTransport.StopAsync` calls `FlushAsync()` without passing the `cancellationToken` parameter, potentially causing shutdown to hang.

## Location

**File**: `src/AxolotlMCP.Core/Transport/StdioTransport.cs:47`

## Current Code

```csharp
await _output.FlushAsync();
```

## Problem

The method accepts a `CancellationToken` parameter but doesn't forward it to `FlushAsync()`:
- If flush operation blocks, cannot be cancelled
- Shutdown operation may hang indefinitely
- Part of broader pattern of missing cancellation token propagation

## Impact

- ⚠️ Shutdown may hang if flush blocks
- ⚠️ Cannot cancel stuck flush operation
- ⚠️ Inconsistent cancellation support
- ⚠️ Poor user experience during shutdown

## Suggested Fix

```csharp
await _output.FlushAsync(cancellationToken);
```

## Priority

🟡 **MEDIUM** - Affects shutdown reliability

## Testing Recommendations

1. Add test for cancellation during flush
2. Add test for shutdown timeout scenario
3. Test with slow/blocked output stream
4. Verify cancellation propagates correctly

## Related Issues

- Part of cancellation token support improvements (Issues #1, #2, #10)
- Related to Issue #9 (NamedPipeTransport flush issue)

---
*Discovered during code review in PR #35*
