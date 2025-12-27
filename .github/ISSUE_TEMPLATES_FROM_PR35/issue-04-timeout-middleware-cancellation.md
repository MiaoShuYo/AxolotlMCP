---
title: "[BUG] TimeoutMiddleware May Mask Original Cancellation"
labels: bug, high-priority, middleware
---

## Bug Description

The timeout middleware's exception handling only checks the local timeout `CancellationTokenSource`, potentially masking user-initiated cancellations as timeouts.

## Location

**File**: `src/AxolotlMCP.Server/Middleware/TimeoutMiddleware.cs:42`

## Current Code

```csharp
catch (OperationCanceledException) when (cts.IsCancellationRequested)
{
    throw new McpException(ErrorCodes.RequestTimeout, "Request timed out");
}
```

## Problem

The condition `when (cts.IsCancellationRequested)` only checks if the local timeout token was cancelled. If the original `cancellationToken` (user cancellation) is also cancelled, it will incorrectly report this as a timeout.

## Impact

- ❌ User cancellations incorrectly reported as timeouts
- ❌ Wrong error code (`RequestTimeout` vs. appropriate cancellation code)
- ❌ Misleading error messages to clients
- ❌ Difficult to debug real timeout vs. user cancellation issues

## Suggested Fix

```csharp
catch (OperationCanceledException) when (cts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
{
    throw new McpException(ErrorCodes.RequestTimeout, "Request timed out");
}
```

This ensures we only treat it as a timeout if:
1. The timeout token was cancelled (cts.IsCancellationRequested)
2. AND the original token was NOT cancelled (!cancellationToken.IsCancellationRequested)

## Priority

🔴 **HIGH** - Affects error reporting accuracy and debugging

## Testing Recommendations

1. Add test for timeout scenario (should throw RequestTimeout)
2. Add test for user cancellation scenario (should not throw RequestTimeout)
3. Add test for both timeout and user cancellation simultaneously
4. Verify correct error codes and messages in each scenario

## Related Issues

None

---
*Discovered during code review in PR #35*
