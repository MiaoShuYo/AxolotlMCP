#!/bin/bash

# Script to create GitHub issues from PR #35 bug report
# This script will create 14 issues based on the bugs discovered during code review

# Continue on error - we want to create remaining issues even if some fail
set +e

REPO="MiaoShuYo/AxolotlMCP"
ISSUE_DIR=".github/ISSUE_TEMPLATES_FROM_PR35"

echo "================================================"
echo "Creating GitHub Issues from PR #35 Bug Report"
echo "================================================"
echo ""
echo "Repository: $REPO"
echo "Total issues to create: 14"
echo ""

# Check if gh CLI is authenticated
if ! gh auth status >/dev/null 2>&1; then
    echo "❌ Error: GitHub CLI is not authenticated."
    echo "Please run: gh auth login"
    exit 1
fi

echo "✓ GitHub CLI is authenticated"
echo ""

# Function to create an issue from a template file
create_issue() {
    local issue_file=$1
    local issue_number=$2
    
    if [ ! -f "$issue_file" ]; then
        echo "❌ File not found: $issue_file"
        return 1
    fi
    
    # Extract title from front matter (handles both quoted and unquoted)
    title=$(grep "^title:" "$issue_file" | sed 's/^title: *//' | sed 's/^"\(.*\)"$/\1/')
    
    # Extract labels from front matter
    labels=$(grep "^labels:" "$issue_file" | sed 's/labels: //')
    
    # Extract body (everything after the front matter)
    body=$(sed -n '/^---$/,/^---$/d; p' "$issue_file")
    
    echo "[$issue_number/14] Creating issue: $title"
    
    # Create the issue
    if gh issue create \
        --repo "$REPO" \
        --title "$title" \
        --body "$body" \
        --label "$labels" >/dev/null 2>&1; then
        echo "  ✓ Created successfully"
    else
        echo "  ⚠️ Failed to create (may already exist or lack permissions)"
    fi
    
    # Small delay to avoid rate limiting
    sleep 1
}

echo "Starting issue creation..."
echo ""

# Create all issues in order
create_issue "$ISSUE_DIR/issue-01-stdio-transport-cancellation.md" 1
create_issue "$ISSUE_DIR/issue-02-namedpipe-transport-cancellation.md" 2
create_issue "$ISSUE_DIR/issue-03-event-subscription-memory-leak.md" 3
create_issue "$ISSUE_DIR/issue-04-timeout-middleware-cancellation.md" 4
create_issue "$ISSUE_DIR/issue-05-server-stop-verification.md" 5
create_issue "$ISSUE_DIR/issue-06-client-disconnect-verification.md" 6
create_issue "$ISSUE_DIR/issue-07-schema-generator-readonly-properties.md" 7
create_issue "$ISSUE_DIR/issue-08-stdio-transport-flush-cancellation.md" 8
create_issue "$ISSUE_DIR/issue-09-namedpipe-transport-sync-flush.md" 9
create_issue "$ISSUE_DIR/issue-10-namedpipe-send-cancellation.md" 10
create_issue "$ISSUE_DIR/issue-11-websocket-unnecessary-await.md" 11
create_issue "$ISSUE_DIR/issue-12-empty-line-handling.md" 12
create_issue "$ISSUE_DIR/issue-13-empty-catch-blocks.md" 13
create_issue "$ISSUE_DIR/issue-14-magic-numbers.md" 14

echo ""
echo "================================================"
echo "✓ Issue creation process completed!"
echo "================================================"
echo ""
echo "Summary:"
echo "  - High Priority Issues: 4 (🔴)"
echo "  - Medium Priority Issues: 6 (🟡)"
echo "  - Low Priority Issues: 2 (🟢)"
echo "  - Code Quality: 2 (📝)"
echo ""
echo "View all issues at: https://github.com/$REPO/issues"
echo ""
echo "Note: All issues are labeled appropriately for easy filtering."
echo "      Use labels like 'bug', 'high-priority', 'transport', etc."
echo ""
