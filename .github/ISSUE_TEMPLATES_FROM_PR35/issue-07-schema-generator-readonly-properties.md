---
title: "[BUG] JsonSchemaGenerator Property Access Check Too Restrictive"
labels: bug, medium-priority, schema
---

## Bug Description

`JsonSchemaGenerator` requires properties to have both getter AND setter, excluding valid read-only properties from generated schemas.

## Location

**File**: `src/AxolotlMCP.Core/Protocol/JsonSchemaGenerator.cs:53`

## Current Code

```csharp
.Where(p => p.GetMethod != null && p.SetMethod != null)
```

## Problem

The filter requires both `GetMethod` and `SetMethod` to be present. However:
- Read-only properties (only getter) are common and valid
- Output DTOs often have calculated/derived properties
- Many API responses include read-only fields
- Excluding these properties results in incomplete schemas

## Impact

- ⚠️ Read-only properties missing from generated schemas
- ⚠️ Incomplete API documentation
- ⚠️ Schema validation may fail for valid objects
- ⚠️ Confusion for API consumers

## Example

```csharp
public class UserInfo
{
    public string Name { get; set; }  // Included
    public string Id { get; }  // Excluded (read-only)
    public DateTime CreatedAt { get; }  // Excluded (read-only)
}
```

The schema would only include `Name`, missing `Id` and `CreatedAt`.

## Suggested Fix

### Option 1: Only require getter (Recommended)
```csharp
.Where(p => p.GetMethod != null)
```

### Option 2: Add configuration option
```csharp
.Where(p => p.GetMethod != null && (options.IncludeReadOnlyProperties || p.SetMethod != null))
```

## Priority

🟡 **MEDIUM** - Affects schema completeness but may have workarounds

## Testing Recommendations

1. Add test with read-only properties
2. Add test with write-only properties (if supported)
3. Add test with mixed property types
4. Verify generated schemas match expected output

## Related Issues

None

---
*Discovered during code review in PR #35*
