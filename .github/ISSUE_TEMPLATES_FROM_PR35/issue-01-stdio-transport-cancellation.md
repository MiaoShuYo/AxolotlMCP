---
title: "[BUG] Missing CancellationToken in StdioTransport.ReadLineAsync"
labels: bug, high-priority, transport
---

## Bug Description

The `ReadLineAsync()` call in `StdioTransport.cs` doesn't pass the `cancellationToken` parameter, which means read operations cannot be cancelled even when requested.

## Location

**File**: `src/AxolotlMCP.Core/Transport/StdioTransport.cs:63`

## Current Code

```csharp
var line = await _input.ReadLineAsync();
```

## Problem

The method signature accepts a `CancellationToken` parameter but doesn't use it when calling `_input.ReadLineAsync()`. This prevents cancellation of read operations.

## Impact

- ❌ Cannot cancel read operations gracefully
- ❌ May cause hangs during shutdown
- ❌ Potential resource leaks
- ❌ Server cannot stop promptly when requested

## Suggested Fix

```csharp
var line = await _input.ReadLineAsync(cancellationToken);
```

## Priority

🔴 **HIGH** - This affects graceful shutdown and resource management

## Testing Recommendations

1. Add unit test for cancellation during read operation
2. Add integration test for transport shutdown with pending reads
3. Verify no resource leaks after cancellation

## Related Issues

- Related to Issue #2 (NamedPipeTransport has same problem)
- Part of cancellation token support improvements (Issues #8, #10)

---
*Discovered during code review in PR #35*
