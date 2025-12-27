---
title: "[BUG] Missing CancellationToken in NamedPipeTransport.SendAsync"
labels: bug, medium-priority, transport
---

## Bug Description

`NamedPipeTransport.SendAsync` calls `FlushAsync()` without passing the `cancellationToken` parameter.

## Location

**File**: `src/AxolotlMCP.Core/Transport/NamedPipeTransport.cs:61`

## Current Code

```csharp
await _writer.FlushAsync().ConfigureAwait(false);
```

## Problem

The method accepts a `CancellationToken` parameter but doesn't forward it to `FlushAsync()`:
- Cannot cancel flush operation
- Part of broader pattern of missing cancellation token propagation
- Inconsistent with async best practices

## Impact

- ⚠️ Cannot cancel flush operation during send
- ⚠️ Incomplete cancellation support
- ⚠️ May delay disconnect if flush is slow
- ⚠️ Inconsistent API behavior

## Suggested Fix

```csharp
await _writer.FlushAsync(cancellationToken).ConfigureAwait(false);
```

## Priority

🟡 **MEDIUM** - Affects cancellation completeness

## Testing Recommendations

1. Add test for cancellation during send/flush
2. Add test for slow flush scenario
3. Verify cancellation propagates correctly
4. Test disconnect during send operation

## Related Issues

- Part of cancellation token support improvements (Issues #1, #2, #8)
- Related to Issues #8 and #9 (other flush issues)

---
*Discovered during code review in PR #35*
