# Bug Issue Creation Tool

This directory contains a tool and workflow to automatically create GitHub issues from bug reports discovered during code review.

## Overview

The tool parses the `BUGS_FOUND.md` file in the repository root and creates individual GitHub issues for each bug, with appropriate labels indicating the bug severity.

## Components

1. **CreateBugIssues** - A .NET console application that:
   - Parses the bug report markdown file
   - Extracts bug information (title, description, priority)
   - Creates GitHub issues via the GitHub API
   - Applies labels: `bug` and priority labels (`priority: critical`, `priority: medium`, `priority: low`)

2. **create-bug-issues.sh** - A shell script wrapper for easier local execution

3. **GitHub Actions Workflow** - Automated workflow for creating issues in CI/CD

## Usage

### Option 1: Manual Execution (Local)

1. Generate a GitHub Personal Access Token with `repo` scope:
   - Go to https://github.com/settings/tokens
   - Click "Generate new token (classic)"
   - Select the `repo` scope
   - Copy the token

2. Set the environment variable:
   ```bash
   export GITHUB_TOKEN=your_github_personal_access_token
   ```

3. Run the script:
   ```bash
   cd /home/runner/work/AxolotlMCP/AxolotlMCP
   ./tools/create-bug-issues.sh
   ```

### Option 2: GitHub Actions Workflow

1. Go to the repository's Actions tab
2. Select "Create Bug Issues" workflow
3. Click "Run workflow"
4. Choose whether to do a dry run or actually create issues
5. Click "Run workflow" button

The workflow uses the repository's `GITHUB_TOKEN` automatically, so no additional setup is required.

### Option 3: Direct .NET Execution

```bash
cd tools/CreateBugIssues
dotnet build
GITHUB_TOKEN=your_token dotnet run
```

Or with custom repository:
```bash
GITHUB_TOKEN=your_token dotnet run -- owner repo-name
```

### Environment Variables

The tool supports the following environment variables:
- `GITHUB_TOKEN` (required): Your GitHub personal access token
- `GITHUB_REPOSITORY_OWNER` (optional): Repository owner, defaults to "MiaoShuYo"
- `GITHUB_REPOSITORY_NAME` (optional): Repository name, defaults to "AxolotlMCP"
- `BUGS_FILE_PATH` (optional): Custom path to bug report file, defaults to searching for "BUGS_FOUND.md"
- `DEBUG` (optional): Enable detailed error stack traces for debugging

## Bug Report Format

The tool expects the bug report to follow this format:

```markdown
## 🔴 高优先级问题 (High Priority Issues)

### 1. Bug Title Here
**文件**: `path/to/file.cs:line`

**问题描述**:
Description of the problem...

**影响**:
- Impact point 1
- Impact point 2

**建议修复**:
Suggested fix...

---

## 🟡 中优先级问题 (Medium Priority Issues)

### 2. Another Bug Title
...
```

The tool recognizes:
- Priority levels by section headers (高优先级/HIGH PRIORITY → critical, 中优先级/MEDIUM PRIORITY → medium, 低优先级/LOW PRIORITY → low)
- Bug entries starting with `###` followed by a number
- All content between bug headers as the bug description

## Output

The tool creates GitHub issues with:
- **Title**: Extracted from the bug heading
- **Body**: Bug description including file path, problem description, impact, and suggested fixes
- **Labels**: 
  - `bug` (always added)
  - `priority: critical` / `priority: medium` / `priority: low` (based on the section)

## Example Output

```
=== GitHub Bug Issue Creator ===

Repository: MiaoShuYo/AxolotlMCP
Token: ghp_...xxxx

Parsing bugs from: BUGS_FOUND.md

Found 14 bugs to report

Creating issue for: StdioTransport.ReadLineAsync 缺少CancellationToken支持
  ✓ Created: https://github.com/MiaoShuYo/AxolotlMCP/issues/1

Creating issue for: NamedPipeTransport.ReadLineAsync 缺少CancellationToken支持
  ✓ Created: https://github.com/MiaoShuYo/AxolotlMCP/issues/2

...

=== Summary ===
Successfully created: 14 issues
Failed: 0 issues
```

## Troubleshooting

### "GITHUB_TOKEN environment variable not set"
- Make sure you've exported the `GITHUB_TOKEN` environment variable
- Verify the token has the correct permissions (repo scope)

### "Bug report file not found"
- Ensure `BUGS_FOUND.md` exists in the repository root
- Check that you're running the tool from the correct directory

### "GitHub API error: 401 Unauthorized"
- Your token may be expired or invalid
- Generate a new token and try again

### "GitHub API error: 422 Unprocessable Entity"
- An issue with the same title may already exist
- Check the error message for details

### Rate Limiting
- The tool includes a 1-second delay between issue creations to avoid rate limiting
- GitHub's API rate limit is 5000 requests per hour for authenticated requests

## Notes

- The tool skips "code quality suggestions" that don't have a priority level
- Each issue is created independently - if one fails, the others will still be created
- Issues are created with the labels, but you may need to create the label definitions in your repository first:
  - `bug`
  - `priority: critical`
  - `priority: medium`
  - `priority: low`

## Development

The CreateBugIssues tool is a simple .NET 8.0 console application with no external dependencies beyond the .NET SDK.

To modify the tool:
1. Edit `CreateBugIssues/Program.cs`
2. Build with `dotnet build`
3. Test locally before running in CI/CD
