---
title: "[BUG] NamedPipeTransport.StopAsync Uses Sync Operation"
labels: bug, medium-priority, transport
---

## Bug Description

`NamedPipeTransport.StopAsync` uses synchronous `Flush()` instead of `FlushAsync()`, which is not best practice in async methods.

## Location

**File**: `src/AxolotlMCP.Core/Transport/NamedPipeTransport.cs:48`

## Current Code

```csharp
try { _writer?.Flush(); } catch { }
```

## Problem

Using synchronous `Flush()` in an async method:
- Blocks the thread instead of yielding
- Performance impact under load
- Inconsistent with async/await patterns
- May cause thread pool starvation

## Impact

- ⚠️ May block thread during shutdown
- ⚠️ Performance impact
- ⚠️ Inconsistent async patterns
- ⚠️ Potential thread pool issues under load

## Suggested Fix

```csharp
try { await _writer?.FlushAsync(); } catch { }
```

Or if CancellationToken should be supported:
```csharp
try 
{ 
    if (_writer != null)
        await _writer.FlushAsync(cancellationToken); 
} 
catch { }
```

## Priority

🟡 **MEDIUM** - Affects performance and consistency

## Testing Recommendations

1. Add performance test comparing sync vs async flush
2. Add test for high-load scenarios
3. Test thread pool usage during shutdown
4. Verify no blocking behavior

## Related Issues

- Related to Issue #8 (StdioTransport flush issue)
- Part of cancellation token support improvements (Issue #10)

---
*Discovered during code review in PR #35*
