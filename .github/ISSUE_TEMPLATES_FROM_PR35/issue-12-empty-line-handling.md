---
title: "[QUESTION] Inconsistent Empty Line Handling in Transports"
labels: question, low-priority, transport
---

## Issue Description

Both `StdioTransport` and `NamedPipeTransport` skip empty lines in their read loops. This behavior needs verification against MCP protocol specification.

## Location

**Files**: 
- `src/AxolotlMCP.Core/Transport/StdioTransport.cs:68`
- `src/AxolotlMCP.Core/Transport/NamedPipeTransport.cs:80`

## Current Code

Both transports have:
```csharp
if (line.Length == 0)
    continue;
```

## Question

Is this the correct behavior according to MCP protocol specification?

### Considerations:

**Pros of skipping empty lines:**
- Prevents issues with spurious empty messages
- Cleaner message processing
- May be protocol requirement

**Cons of skipping empty lines:**
- May not match protocol specification
- Could cause incompatibility with some clients
- Silently ignores potentially meaningful data

## Impact

- 🟢 Potential incompatibility with some clients
- 🟢 May not match protocol specification
- 🟢 Currently working in practice

## Suggested Actions

1. Review MCP protocol specification for empty line handling
2. If empty lines should be preserved, remove the skip logic
3. If empty lines should be skipped, add comment explaining why
4. Consider adding configuration option if behavior varies by use case

## Priority

🟢 **LOW** - Currently functional, needs specification review

## Testing Recommendations

1. Review protocol specification
2. Test with various MCP clients
3. Add test for empty line handling if behavior is defined
4. Document expected behavior

## Related Issues

None

---
*Discovered during code review in PR #35*
