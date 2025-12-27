#!/usr/bin/env python3
"""
Script to create GitHub issues from PR #35 bug report templates.
This script provides an alternative to the bash script with better error handling.
"""

import os
import re
import subprocess
import sys
import time
from pathlib import Path

REPO = "MiaoShuYo/AxolotlMCP"
ISSUE_DIR = Path(__file__).parent

def check_gh_auth():
    """Check if GitHub CLI is authenticated."""
    try:
        result = subprocess.run(
            ["gh", "auth", "status"],
            capture_output=True,
            text=True,
            check=False
        )
        return result.returncode == 0
    except FileNotFoundError:
        return False

def parse_template(file_path):
    """Parse an issue template file and extract title, labels, and body."""
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Extract front matter
    front_matter_match = re.match(r'^---\n(.*?)\n---\n(.*)', content, re.DOTALL)
    if not front_matter_match:
        return None, None, None
    
    front_matter = front_matter_match.group(1)
    body = front_matter_match.group(2).strip()
    
    # Extract title (handles both quoted and unquoted)
    title_match = re.search(r'title:\s*(.+?)\s*$', front_matter, re.MULTILINE)
    if title_match:
        title = title_match.group(1).strip()
        # Remove surrounding quotes if present
        if (title.startswith('"') and title.endswith('"')) or (title.startswith("'") and title.endswith("'")):
            title = title[1:-1]
    else:
        title = None
    
    # Extract labels
    labels_match = re.search(r'labels:\s*(.*?)\s*$', front_matter, re.MULTILINE)
    labels = labels_match.group(1).strip() if labels_match else ""
    
    return title, labels, body

def create_issue(file_path, issue_number, total_issues, dry_run=False):
    """Create a GitHub issue from a template file."""
    if not file_path.exists():
        print(f"  ❌ File not found: {file_path}")
        return False
    
    title, labels, body = parse_template(file_path)
    
    if not title or not body:
        print(f"  ❌ Failed to parse template: {file_path}")
        return False
    
    print(f"[{issue_number}/{total_issues}] Creating issue: {title}")
    
    if dry_run:
        print(f"  ℹ️  Dry run - would create with labels: {labels}")
        return True
    
    try:
        cmd = [
            "gh", "issue", "create",
            "--repo", REPO,
            "--title", title,
            "--body", body,
        ]
        
        if labels:
            cmd.extend(["--label", labels])
        
        result = subprocess.run(
            cmd,
            capture_output=True,
            text=True,
            check=False
        )
        
        if result.returncode == 0:
            print("  ✓ Created successfully")
            # Extract issue URL from output if available
            if result.stdout.strip():
                print(f"  🔗 {result.stdout.strip()}")
            return True
        else:
            print(f"  ⚠️  Failed to create: {result.stderr.strip()}")
            return False
            
    except Exception as e:
        print(f"  ❌ Error: {e}")
        return False

def main():
    """Main function to orchestrate issue creation."""
    dry_run = "--dry-run" in sys.argv
    
    print("=" * 50)
    print("Creating GitHub Issues from PR #35 Bug Report")
    print("=" * 50)
    print()
    print(f"Repository: {REPO}")
    print(f"Total issues to create: 14")
    
    if dry_run:
        print("\n⚠️  DRY RUN MODE - No issues will be created\n")
    
    print()
    
    # Check GitHub CLI
    if not dry_run:
        print("Checking GitHub CLI authentication...")
        if not check_gh_auth():
            print("❌ Error: GitHub CLI is not authenticated or not installed.")
            print("Please run: gh auth login")
            sys.exit(1)
        print("✓ GitHub CLI is authenticated")
        print()
    
    # Define issue files in order
    issue_files = [
        "issue-01-stdio-transport-cancellation.md",
        "issue-02-namedpipe-transport-cancellation.md",
        "issue-03-event-subscription-memory-leak.md",
        "issue-04-timeout-middleware-cancellation.md",
        "issue-05-server-stop-verification.md",
        "issue-06-client-disconnect-verification.md",
        "issue-07-schema-generator-readonly-properties.md",
        "issue-08-stdio-transport-flush-cancellation.md",
        "issue-09-namedpipe-transport-sync-flush.md",
        "issue-10-namedpipe-send-cancellation.md",
        "issue-11-websocket-unnecessary-await.md",
        "issue-12-empty-line-handling.md",
        "issue-13-empty-catch-blocks.md",
        "issue-14-magic-numbers.md",
    ]
    
    print("Starting issue creation...\n")
    
    success_count = 0
    fail_count = 0
    
    for i, filename in enumerate(issue_files, start=1):
        file_path = ISSUE_DIR / filename
        if create_issue(file_path, i, len(issue_files), dry_run):
            success_count += 1
        else:
            fail_count += 1
        
        # Small delay to avoid rate limiting
        if not dry_run and i < len(issue_files):
            time.sleep(1)
    
    print()
    print("=" * 50)
    print("✓ Issue creation process completed!")
    print("=" * 50)
    print()
    print("Summary:")
    print(f"  - Successfully created: {success_count}")
    if fail_count > 0:
        print(f"  - Failed: {fail_count}")
    print()
    print("Issue breakdown:")
    print("  - High Priority Issues: 4 (🔴)")
    print("  - Medium Priority Issues: 6 (🟡)")
    print("  - Low Priority Issues: 2 (🟢)")
    print("  - Code Quality: 2 (📝)")
    print()
    print(f"View all issues at: https://github.com/{REPO}/issues")
    print()
    print("Note: All issues are labeled appropriately for easy filtering.")
    print("      Use labels like 'bug', 'high-priority', 'transport', etc.")
    print()
    
    if dry_run:
        print("This was a dry run. Run without --dry-run to create issues.")
    
    sys.exit(0 if fail_count == 0 else 1)

if __name__ == "__main__":
    main()
