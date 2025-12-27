# Bug Report Issues from PR #35

This directory contains issue templates for 14 bugs discovered during code review in [Pull Request #35](https://github.com/MiaoShuYo/AxolotlMCP/pull/35).

## Overview

During a comprehensive code review of the AxolotlMCP repository, 14 bugs were identified ranging from critical resource management issues to code quality improvements. This directory provides structured GitHub issue templates for each bug to facilitate tracking and resolution.

## Issue Summary

### 🔴 High Priority (4 issues)

1. **Missing CancellationToken in StdioTransport.ReadLineAsync** - Prevents graceful cancellation
2. **Missing CancellationToken in NamedPipeTransport.ReadLineAsync** - Prevents graceful cancellation
3. **Memory Leak from Event Subscriptions** - Event handlers never removed
4. **TimeoutMiddleware May Mask Original Cancellation** - Incorrect error reporting

### 🟡 Medium Priority (6 issues)

5. **McpServer.StopAsync Doesn't Verify Read Loop Completion** - Resource cleanup issues
6. **McpClient.DisconnectAsync Doesn't Verify Read Loop Completion** - Resource cleanup issues
7. **JsonSchemaGenerator Property Access Check Too Restrictive** - Excludes read-only properties
8. **Missing CancellationToken in StdioTransport.StopAsync** - Shutdown may hang
9. **NamedPipeTransport.StopAsync Uses Sync Operation** - Performance impact
10. **Missing CancellationToken in NamedPipeTransport.SendAsync** - Incomplete cancellation

### 🟢 Low Priority (2 issues)

11. **Unnecessary await in WebSocketTransport.DisposeAsync** - Code redundancy
12. **Inconsistent Empty Line Handling** - Protocol compliance question

### 📝 Code Quality (2 issues)

13. **Empty catch Blocks Throughout Codebase** - Poor error visibility
14. **Magic Numbers Throughout Codebase** - Maintainability concern

## How to Create Issues

### Option 1: Automated Script (Recommended)

Use the provided script to create all issues at once:

```bash
cd .github/ISSUE_TEMPLATES_FROM_PR35

# Make script executable if needed
chmod +x create_issues.sh

# Run the script
./create_issues.sh
```

**Prerequisites:**
- GitHub CLI (`gh`) must be installed
- Must be authenticated: `gh auth login`
- Must have permission to create issues in the repository

### Option 2: Manual Creation

You can create issues manually by copying the content from each template file:

1. Navigate to the [Issues page](https://github.com/MiaoShuYo/AxolotlMCP/issues)
2. Click "New Issue"
3. Open one of the template files (e.g., `issue-01-stdio-transport-cancellation.md`)
4. Copy the title from the front matter
5. Copy the body content (everything after the `---` markers)
6. Add the labels specified in the template
7. Submit the issue

### Option 3: GitHub CLI Individual Issues

Create issues one at a time using `gh` CLI:

```bash
# First, extract the body (skip front matter)
sed -n '/^---$/,/^---$/d; p' issue-01-stdio-transport-cancellation.md > /tmp/issue-body.md

# Then create the issue
gh issue create \
  --repo MiaoShuYo/AxolotlMCP \
  --title "[BUG] Missing CancellationToken in StdioTransport.ReadLineAsync" \
  --body-file /tmp/issue-body.md \
  --label "bug,high-priority,transport"
```

## File Structure

```
ISSUE_TEMPLATES_FROM_PR35/
├── README.md                                           # This file
├── create_issues.sh                                    # Automated creation script
├── issue-01-stdio-transport-cancellation.md           # 🔴 High Priority
├── issue-02-namedpipe-transport-cancellation.md       # 🔴 High Priority
├── issue-03-event-subscription-memory-leak.md         # 🔴 High Priority
├── issue-04-timeout-middleware-cancellation.md        # 🔴 High Priority
├── issue-05-server-stop-verification.md               # 🟡 Medium Priority
├── issue-06-client-disconnect-verification.md         # 🟡 Medium Priority
├── issue-07-schema-generator-readonly-properties.md   # 🟡 Medium Priority
├── issue-08-stdio-transport-flush-cancellation.md     # 🟡 Medium Priority
├── issue-09-namedpipe-transport-sync-flush.md         # 🟡 Medium Priority
├── issue-10-namedpipe-send-cancellation.md           # 🟡 Medium Priority
├── issue-11-websocket-unnecessary-await.md            # 🟢 Low Priority
├── issue-12-empty-line-handling.md                    # 🟢 Low Priority
├── issue-13-empty-catch-blocks.md                     # 📝 Code Quality
└── issue-14-magic-numbers.md                          # 📝 Code Quality
```

## Labels Used

Each issue template includes appropriate labels for filtering and categorization:

- **Priority**: `high-priority`, `medium-priority`, `low-priority`
- **Type**: `bug`, `code-quality`, `question`
- **Component**: `transport`, `server`, `client`, `middleware`, `schema`
- **Specific**: `memory-leak`, `maintenance`

## Issue Template Format

Each template follows this structure:

```markdown
---
title: "[TYPE] Issue Title"
labels: label1, label2, label3
---

## Bug Description
Brief description of the issue

## Location
File and line number

## Current Code
Code snippet showing the problem

## Problem
Detailed explanation

## Impact
What problems this causes

## Suggested Fix
Proposed solution with code examples

## Priority
Priority level with emoji

## Testing Recommendations
How to test the fix

## Related Issues
Links to related issues

---
*Discovered during code review in PR #35*
```

## Next Steps

1. Create the issues using one of the methods above
2. Prioritize issues based on severity
3. Assign issues to team members
4. Track progress on each issue
5. Reference these issues when creating PRs with fixes

## Additional Resources

- **Original Bug Report**: [BUGS_FOUND.md](../../BUGS_FOUND.md) (Chinese)
- **Bug Summary**: [BUG_REPORT_SUMMARY.md](../../BUG_REPORT_SUMMARY.md) (English)
- **Fix Reference**: [FIXES_QUICK_REFERENCE.md](../../FIXES_QUICK_REFERENCE.md)
- **Implementation Roadmap**: [README_CODE_REVIEW.md](../../README_CODE_REVIEW.md)
- **Pull Request**: [PR #35](https://github.com/MiaoShuYo/AxolotlMCP/pull/35)

## Contributing

When working on these issues:

1. Reference the issue number in your PR
2. Include tests for the fix
3. Verify all existing tests still pass
4. Update documentation if needed
5. Follow the suggested fix as a starting point

## Questions?

If you have questions about any of these issues, please comment on the specific issue in GitHub or reach out to the team.

---

*Generated from code review findings in PR #35*
*Date: 2025-12-27*
