#!/bin/bash
# Script to create GitHub issues from bug reports

set -e

# Check if GITHUB_TOKEN is set
if [ -z "$GITHUB_TOKEN" ]; then
    echo "Error: GITHUB_TOKEN environment variable is not set"
    echo ""
    echo "Please set it with:"
    echo "  export GITHUB_TOKEN=your_github_personal_access_token"
    echo ""
    echo "Or run with:"
    echo "  GITHUB_TOKEN=your_token ./create-bug-issues.sh"
    exit 1
fi

# Get repository info from git if not set
if [ -z "$GITHUB_REPOSITORY_OWNER" ]; then
    GITHUB_REPOSITORY_OWNER=$(git config --get remote.origin.url | sed -E 's/.*[:/]([^/]+)\/[^/]+\.git/\1/')
    export GITHUB_REPOSITORY_OWNER
fi

if [ -z "$GITHUB_REPOSITORY_NAME" ]; then
    GITHUB_REPOSITORY_NAME=$(git config --get remote.origin.url | sed -E 's/.*[:/][^/]+\/([^/]+)\.git/\1/')
    export GITHUB_REPOSITORY_NAME
fi

echo "Creating GitHub issues..."
echo "Repository: $GITHUB_REPOSITORY_OWNER/$GITHUB_REPOSITORY_NAME"
echo ""

# Navigate to the tool directory and run
cd "$(dirname "$0")/CreateBugIssues"
dotnet run
