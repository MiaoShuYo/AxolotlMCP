---
title: "[BUG] Missing CancellationToken in NamedPipeTransport.ReadLineAsync"
labels: bug, high-priority, transport
---

## Bug Description

The `ReadLineAsync()` call in `NamedPipeTransport.cs` doesn't pass the `cancellationToken` parameter, preventing cancellation of read operations.

## Location

**File**: `src/AxolotlMCP.Core/Transport/NamedPipeTransport.cs:73`

## Current Code

```csharp
line = await _reader.ReadLineAsync().ConfigureAwait(false);
```

## Problem

Similar to StdioTransport, the method accepts a `CancellationToken` but doesn't forward it to the `ReadLineAsync()` call.

## Impact

- ❌ Cannot cancel read operations gracefully
- ❌ May cause hangs when closing named pipe
- ❌ Potential resource leaks
- ❌ Client/Server cannot disconnect promptly

## Suggested Fix

```csharp
line = await _reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
```

## Priority

🔴 **HIGH** - This affects graceful shutdown and resource management

## Testing Recommendations

1. Add unit test for cancellation during read operation
2. Add integration test for named pipe closure with pending reads
3. Test behavior when client disconnects abruptly

## Related Issues

- Related to Issue #1 (StdioTransport has same problem)
- Part of cancellation token support improvements (Issues #8, #10)

---
*Discovered during code review in PR #35*
