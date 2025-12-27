# Quick Reference: How to Address the Bugs

This guide provides quick snippets for fixing each identified bug.

## 🔴 High Priority Fixes

### Fix #1 & #2: Add CancellationToken to ReadLineAsync calls

**StdioTransport.cs line 63:**
```diff
- var line = await _input.ReadLineAsync();
+ var line = await _input.ReadLineAsync(cancellationToken);
```

**NamedPipeTransport.cs line 73:**
```diff
- line = await _reader.ReadLineAsync().ConfigureAwait(false);
+ line = await _reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
```

### Fix #3: Event Subscription Memory Leak

**DefaultHandler.cs - Add subscription tracking:**

```csharp
public sealed class DefaultHandler : IMcpHandler
{
    // Add these fields
    private bool _resourcesSubscribed = false;
    private bool _promptsSubscribed = false;
    private readonly object _subscriptionLock = new();

    private Task<ResponseMessage> HandleResourcesSubscribeAsync(RequestMessage request, CancellationToken ct)
    {
        lock (_subscriptionLock)
        {
            if (!_resourcesSubscribed)
            {
                _resources.OnChanged += HandleResourceChanged;
                _resourcesSubscribed = true;
            }
        }
        return Task.FromResult(new ResponseMessage
        {
            Id = request.Id,
            Result = JsonSerializer.SerializeToElement(new { ok = true }, JsonDefaults.Options)
        });
    }

    private async void HandleResourceChanged(string action, string name)
    {
        var note = new NotificationMessage
        {
            Method = "resources/changed",
            Params = JsonSerializer.SerializeToElement(new { action, name }, JsonDefaults.Options)
        };
        try { await _notifier.NotifyAsync(note, CancellationToken.None).ConfigureAwait(false); } catch { }
    }

    // Similar changes for prompts...
}
```

### Fix #4: Timeout vs User Cancellation

**TimeoutMiddleware.cs line 42:**
```diff
- catch (OperationCanceledException) when (cts.IsCancellationRequested)
+ catch (OperationCanceledException) when (cts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
```

## 🟡 Medium Priority Fixes

### Fix #5 & #6: Verify Read Loop Completion

**McpServer.cs line 61:**
```diff
  try
  {
      _cts?.Cancel();
      if (_readLoopTask is not null)
      {
-         await Task.WhenAny(_readLoopTask, Task.Delay(TimeSpan.FromSeconds(2), cancellationToken)).ConfigureAwait(false);
+         var completed = await Task.WhenAny(_readLoopTask, Task.Delay(TimeSpan.FromSeconds(2), cancellationToken)).ConfigureAwait(false);
+         if (completed != _readLoopTask)
+         {
+             _logger.LogWarning("Read loop did not complete within timeout");
+         }
      }
  }
```

**McpClient.cs line 78:** (same pattern)

### Fix #7: JsonSchemaGenerator Property Filter

**JsonSchemaGenerator.cs line 53:**
```diff
  var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
-     .Where(p => p.GetMethod != null && p.SetMethod != null)
+     .Where(p => p.GetMethod != null)  // Allow read-only properties
      .ToArray();
```

### Fix #8: StdioTransport.StopAsync CancellationToken

**StdioTransport.cs line 47:**
```diff
  _running = false;
- await _output.FlushAsync();
+ await _output.FlushAsync(cancellationToken);
```

### Fix #9: NamedPipeTransport.StopAsync Use Async

**NamedPipeTransport.cs line 48:**
```diff
  _running = false;
- try { _writer?.Flush(); } catch { }
+ try { if (_writer != null) await _writer.FlushAsync(); } catch { }
```

### Fix #10: NamedPipeTransport.SendAsync CancellationToken

**NamedPipeTransport.cs line 61:**
```diff
  await _writer.WriteLineAsync(jsonPayload.AsMemory()).ConfigureAwait(false);
- await _writer.FlushAsync().ConfigureAwait(false);
+ await _writer.FlushAsync(cancellationToken).ConfigureAwait(false);
```

## 🟢 Low Priority Fixes

### Fix #11: Remove Unnecessary await

**WebSocketTransport.cs line 94:**
```diff
  public async ValueTask DisposeAsync()
  {
      _running = false;
      try { _socket.Dispose(); } catch { }
-     await Task.CompletedTask;
+     // Line removed - not needed
  }
```

Or better yet, remove `async` from the method signature:
```diff
- public async ValueTask DisposeAsync()
+ public ValueTask DisposeAsync()
  {
      _running = false;
      try { _socket.Dispose(); } catch { }
+     return ValueTask.CompletedTask;
  }
```

### Fix #12: Document Empty Line Behavior

Add XML documentation comment explaining the empty line skipping behavior and verify it matches MCP protocol spec.

## 📝 Code Quality Improvements

### Improvement #1: Replace Empty Catch Blocks

Example pattern to use:
```csharp
try 
{ 
    // operation 
} 
catch (Exception ex) when (ex is IOException or ObjectDisposedException)
{ 
    _logger?.LogDebug(ex, "Expected exception during cleanup");
}
```

### Improvement #2: Extract Magic Numbers

```csharp
// Add constants at class level
private static readonly TimeSpan ReadLoopShutdownTimeout = TimeSpan.FromSeconds(2);
private const int DefaultBufferSize = 64 * 1024;

// Then use them:
await Task.WhenAny(_readLoopTask, Task.Delay(ReadLoopShutdownTimeout, cancellationToken));
```

---

## Testing Checklist After Fixes

- [ ] Test cancellation during read operations
- [ ] Test multiple subscription calls (should not duplicate handlers)
- [ ] Test user cancellation vs timeout scenarios
- [ ] Test graceful shutdown with pending operations
- [ ] Verify no resource leaks with long-running services
- [ ] Test schema generation for read-only properties
- [ ] Performance test with the changes

---

## Notes

- All suggested fixes maintain backward compatibility
- No breaking API changes are required
- Most fixes are localized to single lines or small blocks
- Consider adding regression tests for each fixed bug
