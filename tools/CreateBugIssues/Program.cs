using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CreateBugIssues;

/// <summary>
/// Tool to create GitHub issues from bug reports
/// </summary>
class Program
{
    private static readonly HttpClient _httpClient = new();
    
    static async Task<int> Main(string[] args)
    {
        try
        {
            Console.WriteLine("=== GitHub Bug Issue Creator ===\n");
            
            // Get GitHub token from environment
            var githubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
            if (string.IsNullOrEmpty(githubToken))
            {
                Console.WriteLine("Error: GITHUB_TOKEN environment variable not set");
                Console.WriteLine("Usage: export GITHUB_TOKEN=your_github_token");
                return 1;
            }
            
            // Get repository info from environment or args
            var owner = Environment.GetEnvironmentVariable("GITHUB_REPOSITORY_OWNER") ?? "MiaoShuYo";
            var repo = Environment.GetEnvironmentVariable("GITHUB_REPOSITORY_NAME") ?? "AxolotlMCP";
            
            if (args.Length >= 2)
            {
                owner = args[0];
                repo = args[1];
            }
            
            Console.WriteLine($"Repository: {owner}/{repo}");
            Console.WriteLine($"Token: {githubToken[..4]}...{githubToken[^4..]}\n");
            
            // Parse bug reports - check for custom path first
            var bugsFile = Environment.GetEnvironmentVariable("BUGS_FILE_PATH");
            
            if (string.IsNullOrEmpty(bugsFile))
            {
                // Search for BUGS_FOUND.md by walking up the directory tree
                bugsFile = FindBugsFile(Directory.GetCurrentDirectory());
            }
            
            if (string.IsNullOrEmpty(bugsFile) || !File.Exists(bugsFile))
            {
                Console.WriteLine($"Error: Bug report file not found");
                if (!string.IsNullOrEmpty(bugsFile))
                {
                    Console.WriteLine($"  Searched at: {bugsFile}");
                }
                Console.WriteLine("Tip: Set BUGS_FILE_PATH environment variable to specify a custom path");
                return 1;
            }
            
            Console.WriteLine($"Parsing bugs from: {bugsFile}\n");
            var bugs = ParseBugReport(bugsFile);
            Console.WriteLine($"Found {bugs.Count} bugs to report\n");
            
            // Setup HTTP client
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", githubToken);
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("AxolotlMCP-BugReporter/1.0");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            
            // Create issues
            int successCount = 0;
            int failCount = 0;
            
            foreach (var bug in bugs)
            {
                try
                {
                    Console.WriteLine($"Creating issue for: {bug.Title}");
                    var issueUrl = await CreateGitHubIssue(owner, repo, bug);
                    Console.WriteLine($"  ✓ Created: {issueUrl}\n");
                    successCount++;
                    
                    // Rate limiting - wait between requests
                    await Task.Delay(1000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ✗ Failed: {ex.Message}\n");
                    failCount++;
                }
            }
            
            Console.WriteLine("=== Summary ===");
            Console.WriteLine($"Successfully created: {successCount} issues");
            Console.WriteLine($"Failed: {failCount} issues");
            
            return failCount > 0 ? 1 : 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal error: {ex.Message}");
            
            // Only log stack trace if DEBUG environment variable is set
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DEBUG")))
            {
                Console.WriteLine("\nStack trace:");
                Console.WriteLine(ex.StackTrace);
            }
            return 1;
        }
    }
    
    static string? FindBugsFile(string startDirectory)
    {
        const string fileName = "BUGS_FOUND.md";
        
        // First check the start directory
        var candidate = Path.Combine(startDirectory, fileName);
        if (File.Exists(candidate))
        {
            return candidate;
        }
        
        // Walk up the directory tree looking for BUGS_FOUND.md
        var currentDir = startDirectory;
        int maxLevels = 10; // Limit how far up we search
        
        while (currentDir != null && maxLevels > 0)
        {
            var parent = Directory.GetParent(currentDir);
            if (parent == null)
            {
                break;
            }
            
            currentDir = parent.FullName;
            candidate = Path.Combine(currentDir, fileName);
            
            if (File.Exists(candidate))
            {
                return candidate;
            }
            
            maxLevels--;
        }
        
        return null;
    }
    
    static List<BugInfo> ParseBugReport(string filePath)
    {
        var bugs = new List<BugInfo>();
        var content = File.ReadAllText(filePath);
        var lines = content.Split('\n');
        
        BugInfo? currentBug = null;
        string currentPriority = "";
        var descriptionLines = new List<string>();
        
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            
            // Detect priority sections
            if (line.Contains("高优先级问题") || line.Contains("HIGH PRIORITY"))
            {
                currentPriority = "critical";
            }
            else if (line.Contains("中优先级问题") || line.Contains("MEDIUM PRIORITY"))
            {
                currentPriority = "medium";
            }
            else if (line.Contains("低优先级问题") || line.Contains("LOW PRIORITY"))
            {
                currentPriority = "low";
            }
            
            // Detect bug heading (###)
            if (line.StartsWith("### ") && line.Length > 4 && char.IsDigit(line[4]))
            {
                // Save previous bug if exists
                if (currentBug != null && descriptionLines.Count > 0)
                {
                    currentBug.Description = string.Join("\n", descriptionLines).Trim();
                    bugs.Add(currentBug);
                }
                
                // Start new bug
                var titleMatch = Regex.Match(line, @"### \d+\.\s+(.+)");
                if (titleMatch.Success)
                {
                    currentBug = new BugInfo
                    {
                        Title = titleMatch.Groups[1].Value.Trim(),
                        Priority = currentPriority
                    };
                    descriptionLines.Clear();
                }
            }
            else if (currentBug != null && !string.IsNullOrWhiteSpace(line))
            {
                // Skip separator lines
                if (line.StartsWith("---") || line.StartsWith("##") || line.StartsWith("# "))
                {
                    continue;
                }
                
                // Add to description
                descriptionLines.Add(line);
            }
        }
        
        // Add last bug
        if (currentBug != null && descriptionLines.Count > 0)
        {
            currentBug.Description = string.Join("\n", descriptionLines).Trim();
            bugs.Add(currentBug);
        }
        
        // Filter out code quality suggestions
        return bugs.Where(b => !string.IsNullOrEmpty(b.Priority)).ToList();
    }
    
    static async Task<string> CreateGitHubIssue(string owner, string repo, BugInfo bug)
    {
        var url = $"https://api.github.com/repos/{owner}/{repo}/issues";
        
        var labels = new List<string> { "bug" };
        
        // Map priority to labels
        labels.Add(bug.Priority switch
        {
            "critical" => "priority: critical",
            "medium" => "priority: medium",
            "low" => "priority: low",
            _ => "priority: medium"
        });
        
        var payload = new
        {
            title = bug.Title,
            body = FormatIssueBody(bug),
            labels = labels.ToArray()
        };
        
        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync(url, content);
        var responseBody = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"GitHub API error: {response.StatusCode} - {responseBody}");
        }
        
        var result = JsonSerializer.Deserialize<JsonElement>(responseBody);
        return result.GetProperty("html_url").GetString() ?? "";
    }
    
    static string FormatIssueBody(BugInfo bug)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("## Bug Description");
        sb.AppendLine();
        sb.AppendLine(bug.Description);
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine($"**Priority**: {bug.Priority}");
        sb.AppendLine();
        sb.AppendLine("*This issue was automatically created from the code review bug report.*");
        
        return sb.ToString();
    }
}

class BugInfo
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Priority { get; set; } = "";
}
