# How to Submit Bug Reports from PR #35 to GitHub Issues

This document explains how to create GitHub issues for the 14 bugs discovered during the code review in [Pull Request #35](https://github.com/MiaoShuYo/AxolotlMCP/pull/35).

## Quick Start

The fastest way to create all 14 issues is to use one of the provided scripts:

### Using Python (Recommended - Cross-platform)

```bash
cd .github/ISSUE_TEMPLATES_FROM_PR35
python3 create_issues.py
```

### Using Bash (Linux/Mac)

```bash
cd .github/ISSUE_TEMPLATES_FROM_PR35
./create_issues.sh
```

## Prerequisites

Before running the scripts, ensure you have:

1. **GitHub CLI (`gh`)** installed
   - Install: https://cli.github.com/
   - Verify: `gh --version`

2. **GitHub CLI authentication**
   - Run: `gh auth login`
   - Follow the prompts to authenticate

3. **Permission to create issues** in the `MiaoShuYo/AxolotlMCP` repository

## What Issues Will Be Created?

The scripts will create 14 GitHub issues:

### 🔴 High Priority (4 issues)
1. Missing CancellationToken in StdioTransport.ReadLineAsync
2. Missing CancellationToken in NamedPipeTransport.ReadLineAsync  
3. Memory Leak from Event Subscriptions in DefaultHandler
4. TimeoutMiddleware May Mask Original Cancellation

### 🟡 Medium Priority (6 issues)
5. McpServer.StopAsync Doesn't Verify Read Loop Completion
6. McpClient.DisconnectAsync Doesn't Verify Read Loop Completion
7. JsonSchemaGenerator Property Access Check Too Restrictive
8. Missing CancellationToken in StdioTransport.StopAsync
9. NamedPipeTransport.StopAsync Uses Sync Operation
10. Missing CancellationToken in NamedPipeTransport.SendAsync

### 🟢 Low Priority (2 issues)
11. Unnecessary await in WebSocketTransport.DisposeAsync
12. Inconsistent Empty Line Handling in Transports

### 📝 Code Quality (2 issues)
13. Empty catch Blocks Throughout Codebase
14. Magic Numbers Throughout Codebase

Each issue includes:
- Detailed description
- Exact file location and line numbers
- Current problematic code
- Impact assessment
- Suggested fix with code examples
- Testing recommendations
- Related issues
- Appropriate labels for filtering

## Testing Before Creating Issues

The Python script supports a dry-run mode to preview what will be created:

```bash
# Python (with dry-run support)
python3 create_issues.py --dry-run
```

Note: The Bash script does not have dry-run mode. Use the Python script if you want to preview before creating issues.

## Manual Issue Creation

If you prefer to create issues manually, or if you only want to create specific issues:

1. Navigate to `.github/ISSUE_TEMPLATES_FROM_PR35/`
2. Open any issue template file (e.g., `issue-01-stdio-transport-cancellation.md`)
3. Go to https://github.com/MiaoShuYo/AxolotlMCP/issues/new
4. Copy the title from the template (after `title:`)
5. Copy the body content (everything after the second `---`)
6. Add the labels specified in the template
7. Click "Submit new issue"

Or use `gh` CLI for individual issues:

```bash
gh issue create \
  --repo MiaoShuYo/AxolotlMCP \
  --title "[BUG] Missing CancellationToken in StdioTransport.ReadLineAsync" \
  --body-file .github/ISSUE_TEMPLATES_FROM_PR35/issue-01-stdio-transport-cancellation.md \
  --label "bug,high-priority,transport"
```

## File Locations

All issue templates and scripts are located in:
```
.github/ISSUE_TEMPLATES_FROM_PR35/
├── README.md                      # Detailed documentation
├── create_issues.py               # Python script (recommended)
├── create_issues.sh               # Bash script
├── issue-01-*.md                  # Issue template files (14 total)
├── issue-02-*.md
└── ... (12 more)
```

## Additional Documentation

For more details about the bugs found, see:
- **BUGS_FOUND.md** - Detailed Chinese analysis
- **BUG_REPORT_SUMMARY.md** - English summary
- **FIXES_QUICK_REFERENCE.md** - Code snippets for fixes
- **README_CODE_REVIEW.md** - Implementation roadmap

## Troubleshooting

### "GitHub CLI is not authenticated"
```bash
gh auth login
```
Follow the prompts to authenticate.

### "gh: command not found"
Install GitHub CLI from https://cli.github.com/

### "Failed to create issue"
- Check you have permission to create issues in the repository
- Verify the repository name is correct: `MiaoShuYo/AxolotlMCP`
- Check for rate limiting (script includes delays)
- Try creating one issue manually to verify permissions

### Issues already exist
The scripts will show a warning if an issue already exists. You can safely ignore this or manually check the existing issues.

## After Creating Issues

1. Review all created issues at https://github.com/MiaoShuYo/AxolotlMCP/issues
2. Verify labels are applied correctly
3. Prioritize issues for the team
4. Assign issues to developers
5. Create a project board if needed to track progress

## Questions?

If you have questions or encounter problems:
1. Check the detailed README in `.github/ISSUE_TEMPLATES_FROM_PR35/README.md`
2. Review the original bug reports: `BUGS_FOUND.md` and `BUG_REPORT_SUMMARY.md`
3. Comment on [PR #35](https://github.com/MiaoShuYo/AxolotlMCP/pull/35)

---

*This guide is part of the bug reporting workflow from PR #35*
