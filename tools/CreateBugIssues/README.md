# Create Bug Issues Tool

This tool automatically creates GitHub issues from the bug reports found in the code review.

## Usage

### Prerequisites
- .NET 8.0 SDK or later
- GitHub Personal Access Token with `repo` scope

### Set up GitHub Token

```bash
export GITHUB_TOKEN=your_github_personal_access_token
```

### Run the tool

From the repository root directory:

```bash
cd tools/CreateBugIssues
dotnet run
```

Or with custom repository:

```bash
dotnet run -- owner repo-name
```

### Environment Variables

- `GITHUB_TOKEN` (required): Your GitHub personal access token
- `GITHUB_REPOSITORY_OWNER` (optional): Repository owner, defaults to "MiaoShuYo"
- `GITHUB_REPOSITORY_NAME` (optional): Repository name, defaults to "AxolotlMCP"
- `BUGS_FILE_PATH` (optional): Custom path to the bug report file, defaults to searching for "BUGS_FOUND.md" in current and parent directories
- `DEBUG` (optional): Set to any value to enable detailed stack traces in error messages

## What it does

1. Parses the `BUGS_FOUND.md` file in the repository root
2. Extracts each bug with its priority level
3. Creates a GitHub issue for each bug with:
   - Title from the bug heading
   - Description from the bug details
   - Labels: `bug` and priority label (`priority: critical`, `priority: medium`, or `priority: low`)

## Output

The tool will output:
- Progress for each issue creation
- Summary of successful and failed issue creations
- Direct links to created issues
