using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace GitHelper.Tests;

public class TagsContainer
{
    [JsonPropertyName("items")]
    public List<GitHelper.GitTag> Items { get; set; } = new List<GitHelper.GitTag>();
}

public class ReleasesContainer
{
    [JsonPropertyName("items")]
    public List<GitHelper.GitRelease> Items { get; set; } = new List<GitHelper.GitRelease>();
}

public class GitHelperTests
{
    private readonly ITestOutputHelper _output;
    
    public GitHelperTests(ITestOutputHelper output)
    {
        _output = output;
    }
    
    /// <summary>
    /// Helper method to determine if the test is running in a Git repository
    /// </summary>
    /// <returns>True if running in a Git repository, otherwise false</returns>
    private bool IsRunningInGitRepository()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory != null && !Directory.Exists(Path.Combine(directory.FullName, ".git")))
        {
            directory = directory.Parent;
        }
        
        return directory != null;
    }

    [Fact]
    public void GetGitBuildNumber_WithValidRepo_ReturnsNumber()
    {
        // Arrange - This test will only run if we're in a git repo
        // Get the current directory and walk up until we find a .git folder
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory != null && !Directory.Exists(Path.Combine(directory.FullName, ".git")))
        {
            directory = directory.Parent;
        }
        
        // Skip test if we're not in a git repo
        if (directory == null)
        {
            _output.WriteLine("Test skipped: Not running in a git repository");
            return;
        }
        
        string repoPath = directory.FullName;
        _output.WriteLine($"Found git repo at: {repoPath}");
        
        // Act
        string buildNumber = GitHelper.GetGitBuildNumber(repoPath);
        
        // Assert
        _output.WriteLine($"Build number: {buildNumber}");
        Assert.False(string.IsNullOrEmpty(buildNumber));
        Assert.NotEqual("1", buildNumber); // Make sure we didn't get the fallback value
        Assert.True(int.TryParse(buildNumber, out int _), "Build number should be parsable as an integer");
    }

    [Fact]
    public void GetGitBuildNumber_WithNonExistentBranch_FallsBackToHead()
    {
        // Arrange - This test will only run if we're in a git repo
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory != null && !Directory.Exists(Path.Combine(directory.FullName, ".git")))
        {
            directory = directory.Parent;
        }
        
        // Skip test if we're not in a git repo
        if (directory == null)
        {
            _output.WriteLine("Test skipped: Not running in a git repository");
            return;
        }
        
        string repoPath = directory.FullName;
        
        // Act - Use a branch name that almost certainly doesn't exist
        string buildNumber = GitHelper.GetGitBuildNumber(repoPath, "non_existent_branch_name_123456789");
        
        // Assert - Should fall back to HEAD and still return a valid build number
        _output.WriteLine($"Build number from non-existent branch: {buildNumber}");
        Assert.False(string.IsNullOrEmpty(buildNumber));
        Assert.True(int.TryParse(buildNumber, out int _), "Build number should be parsable as an integer");
    }

    [Fact]
    public void GetGitBuildNumber_WithInvalidRepo_ReturnsDefault()
    {
        // Arrange - Use a path that is not a git repository
        string nonRepoPath = Path.GetTempPath();
        
        // Act
        string buildNumber = GitHelper.GetGitBuildNumber(nonRepoPath);
        
        // Assert - Should return the default value "1"
        Assert.Equal("1", buildNumber);
    }
    
    [Fact]
    public void GetGitCommitInfo_WithValidRepo_ReturnsInfo()
    {
        // Arrange - This test will only run if we're in a git repo
        // Get the current directory and walk up until we find a .git folder
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory != null && !Directory.Exists(Path.Combine(directory.FullName, ".git")))
        {
            directory = directory.Parent;
        }
        
        // Skip test if we're not in a git repo
        if (directory == null)
        {
            _output.WriteLine("Test skipped: Not running in a git repository");
            return;
        }
        
        string repoPath = directory.FullName;
        _output.WriteLine($"Found git repo at: {repoPath}");
        
        // Act
        var commitInfo = GitHelper.GetGitCommitInfo(repoPath);
        
        // Assert
        _output.WriteLine($"Commit hash: {commitInfo.commitHash}");
        _output.WriteLine($"Timestamp: {commitInfo.timestamp}");
        _output.WriteLine($"Message: {commitInfo.message}");
        
        Assert.NotEqual("Unknown", commitInfo.commitHash);
        Assert.NotEqual("Unknown", commitInfo.timestamp);
        Assert.NotEqual("Unknown", commitInfo.message);
    }
    
    [Fact]
    public void GetGitCommitInfo_WithNonExistentBranch_FallsBackToHead()
    {
        // Arrange - This test will only run if we're in a git repo
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory != null && !Directory.Exists(Path.Combine(directory.FullName, ".git")))
        {
            directory = directory.Parent;
        }
        
        // Skip test if we're not in a git repo
        if (directory == null)
        {
            _output.WriteLine("Test skipped: Not running in a git repository");
            return;
        }
        
        string repoPath = directory.FullName;
        
        // Act - Use a branch name that almost certainly doesn't exist
        var commitInfo = GitHelper.GetGitCommitInfo(repoPath, "non_existent_branch_name_123456789");
        
        // Assert - Should fall back to HEAD and still return valid info
        _output.WriteLine($"Commit hash from non-existent branch: {commitInfo.commitHash}");
        _output.WriteLine($"Timestamp from non-existent branch: {commitInfo.timestamp}");
        _output.WriteLine($"Message from non-existent branch: {commitInfo.message}");
        
        Assert.NotEqual("Unknown", commitInfo.commitHash);
        Assert.NotEqual("Unknown", commitInfo.timestamp);
        Assert.NotEqual("Unknown", commitInfo.message);
    }
    
    [Fact]
    public void GetGitCommitInfo_WithInvalidRepo_ReturnsDefaults()
    {
        // Arrange - Use a path that is not a git repository
        string nonRepoPath = Path.GetTempPath();
        
        // Act
        var commitInfo = GitHelper.GetGitCommitInfo(nonRepoPath);
        
        // Assert - Should return the default values
        Assert.Equal("Unknown", commitInfo.commitHash);
        Assert.Equal("Unknown", commitInfo.timestamp);
        Assert.Equal("Unknown", commitInfo.message);
    }
    
    [Fact]
    public void GetGitTags_WithValidRepo_ReturnsTags()
    {
        // Arrange - This test will only run if we're in a git repo
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory != null && !Directory.Exists(Path.Combine(directory.FullName, ".git")))
        {
            directory = directory.Parent;
        }
        
        // Skip test if we're not in a git repo
        if (directory == null)
        {
            _output.WriteLine("Test skipped: Not running in a git repository");
            return;
        }
        
        string repoPath = directory.FullName;
        _output.WriteLine($"Found git repo at: {repoPath}");
        
        // Act
        string tagsJson = GitHelper.GetGitTags(repoPath);
        var tags = JsonSerializer.Deserialize<List<string>>(tagsJson);
        
        // Assert
        _output.WriteLine($"Tags found: {tags?.Count ?? 0}");
        foreach (var tag in tags ?? new List<string>())
        {
            _output.WriteLine($"  - {tag}");
        }
        
        // Note: We can't assert that tags exist as the repo might not have any tags
        // But we can assert that we got a valid JSON array back
        Assert.NotNull(tags);
    }
    
    [Fact]
    public void GetGitTags_WithLimit_ReturnsLimitedTags()
    {
        // Arrange - This test will only run if we're in a git repo
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory != null && !Directory.Exists(Path.Combine(directory.FullName, ".git")))
        {
            directory = directory.Parent;
        }
        
        // Skip test if we're not in a git repo
        if (directory == null)
        {
            _output.WriteLine("Test skipped: Not running in a git repository");
            return;
        }
        
        string repoPath = directory.FullName;
        int limit = 2; // Limit to 2 tags
        
        // Act
        string tagsJson = GitHelper.GetGitTags(repoPath, limit);
        var tags = JsonSerializer.Deserialize<List<string>>(tagsJson);
        
        // Assert
        _output.WriteLine($"Tags found with limit {limit}: {tags?.Count ?? 0}");
        foreach (var tag in tags ?? new List<string>())
        {
            _output.WriteLine($"  - {tag}");
        }
        
        // If there are tags, there should be at most 'limit' of them
        if (tags?.Count > 0)
        {
            Assert.True(tags.Count <= limit, $"Expected at most {limit} tags, got {tags.Count}");
        }
    }
    
    [Fact]
    public void GetGitTags_WithInvalidRepo_ReturnsEmptyList()
    {
        // Arrange - Use a path that is not a git repository
        string nonRepoPath = Path.GetTempPath();
        
        // Act
        string tagsJson = GitHelper.GetGitTags(nonRepoPath);
        var tags = JsonSerializer.Deserialize<List<string>>(tagsJson);
        
        // Assert - Should return an empty list
        Assert.NotNull(tags);
        Assert.Empty(tags);
    }
    
    [Fact]
    public void GetGitReleases_WithValidRepo_ReturnsReleases()
    {
        // Arrange - This test will only run if we're in a git repo
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory != null && !Directory.Exists(Path.Combine(directory.FullName, ".git")))
        {
            directory = directory.Parent;
        }
        
        // Skip test if we're not in a git repo
        if (directory == null)
        {
            _output.WriteLine("Test skipped: Not running in a git repository");
            return;
        }
        
        string repoPath = directory.FullName;
        _output.WriteLine($"Found git repo at: {repoPath}");
        
        // Act
        string releasesJson = GitHelper.GetGitReleases(repoPath);
        var releases = JsonSerializer.Deserialize<List<string>>(releasesJson);
        
        // Assert
        _output.WriteLine($"Releases found: {releases?.Count ?? 0}");
        foreach (var release in releases ?? new List<string>())
        {
            _output.WriteLine($"  - {release}");
        }
        
        // Note: We can't assert that releases exist as the repo might not have any releases
        // But we can assert that we got a valid JSON array back
        Assert.NotNull(releases);
        
        // Check that all releases follow the expected format
        foreach (var release in releases ?? new List<string>())
        {
            bool isValidRelease = release.StartsWith("v") && release.Length > 1 && char.IsDigit(release[1]) ||
                                 release.Length > 0 && char.IsDigit(release[0]);
            Assert.True(isValidRelease, $"Release '{release}' does not follow the expected format");
        }
    }
    
    [Fact]
    public void GetGitReleases_WithLimit_ReturnsLimitedReleases()
    {
        // Arrange - This test will only run if we're in a git repo
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory != null && !Directory.Exists(Path.Combine(directory.FullName, ".git")))
        {
            directory = directory.Parent;
        }
        
        // Skip test if we're not in a git repo
        if (directory == null)
        {
            _output.WriteLine("Test skipped: Not running in a git repository");
            return;
        }
        
        string repoPath = directory.FullName;
        int limit = 2; // Limit to 2 releases
        
        // Act
        string releasesJson = GitHelper.GetGitReleases(repoPath, limit);
        var releases = JsonSerializer.Deserialize<List<string>>(releasesJson);
        
        // Assert
        _output.WriteLine($"Releases found with limit {limit}: {releases?.Count ?? 0}");
        foreach (var release in releases ?? new List<string>())
        {
            _output.WriteLine($"  - {release}");
        }
        
        // If there are releases, there should be at most 'limit' of them
        if (releases?.Count > 0)
        {
            Assert.True(releases.Count <= limit, $"Expected at most {limit} releases, got {releases.Count}");
        }
    }
    
    [Fact]
    public void GetGitReleases_WithInvalidRepo_ReturnsEmptyList()
    {
        // Arrange - Use a path that is not a git repository
        string nonRepoPath = Path.GetTempPath();
        
        // Act
        string releasesJson = GitHelper.GetGitReleases(nonRepoPath);
        var releases = JsonSerializer.Deserialize<List<string>>(releasesJson);
        
        // Assert - Should return an empty list
        Assert.NotNull(releases);
        Assert.Empty(releases);
    }
    
    [Fact]
    public void GetGitCommitsDetailed_WithValidRepo_ReturnsCommits()
    {
        // Arrange - This test will only run if we're in a git repo
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory != null && !Directory.Exists(Path.Combine(directory.FullName, ".git")))
        {
            directory = directory.Parent;
        }
        
        // Skip test if we're not in a git repo
        if (directory == null)
        {
            _output.WriteLine("Test skipped: Not running in a git repository");
            return;
        }
        
        string repoPath = directory.FullName;
        _output.WriteLine($"Found git repo at: {repoPath}");
        
        // Act
        string commitsJson = GitHelper.GetGitCommitsDetailed(repoPath);
        var commitsContainer = JsonSerializer.Deserialize<GitHelper.GitCommitsContainer>(commitsJson);
        
        // Assert
        Assert.NotNull(commitsContainer);
        _output.WriteLine($"Commits found: {commitsContainer?.Items?.Count ?? 0}");
        
        foreach (var commit in commitsContainer?.Items ?? new List<GitHelper.GitCommit>())
        {
            _output.WriteLine($"Hash: {commit.Hash}");
            _output.WriteLine($"Author: {commit.Author}");
            _output.WriteLine($"Date: {commit.Date}");
            _output.WriteLine($"Title: {commit.Title}");
            _output.WriteLine($"Body: {commit.Body?.Substring(0, Math.Min(commit.Body?.Length ?? 0, 50))}{(commit.Body?.Length > 50 ? "..." : "")}");
            _output.WriteLine("----------------------------");
        }
        
        Assert.NotNull(commitsContainer?.Items);
        if (commitsContainer?.Items?.Count > 0)
        {
            // Verify that commits have the required properties
            var firstCommit = commitsContainer.Items[0];
            Assert.False(string.IsNullOrEmpty(firstCommit.Hash));
            Assert.False(string.IsNullOrEmpty(firstCommit.Author));
            Assert.False(string.IsNullOrEmpty(firstCommit.Date));
            Assert.False(string.IsNullOrEmpty(firstCommit.Title));
            // Body can potentially be empty for some commits
        }
    }
    
    [Fact]
    public void GetGitCommitsDetailed_WithLimit_ReturnsLimitedCommits()
    {
        // Skip the test if not running in a Git repository
        if (!IsRunningInGitRepository())
        {
            _output.WriteLine("Test skipped: Not running in a Git repository");
            return;
        }
        
        // Arrange
        string repoPath = Directory.GetCurrentDirectory();
        while (!Directory.Exists(Path.Combine(repoPath, ".git")) && repoPath.Length > 3)
        {
            repoPath = Directory.GetParent(repoPath)?.FullName ?? "";
            if (string.IsNullOrEmpty(repoPath))
                break;
        }
        int limit = 2; // Limit to 2 commits

        // Act
        string commitsJson = GitHelper.GetGitCommitsDetailed(repoPath, limit: limit);
        var commitsContainer = JsonSerializer.Deserialize<GitHelper.GitCommitsContainer>(commitsJson);

        // Check for null before accessing properties
        Assert.NotNull(commitsContainer);
        if (commitsContainer != null)
        {
            // Output results for debugging
            _output.WriteLine($"Requested {limit} commits, found {commitsContainer.Items?.Count ?? 0}");
            
            // Verify that limit is respected
            Assert.Equal(limit, commitsContainer.Limit);
            Assert.NotNull(commitsContainer.Items);
            if (commitsContainer.Items != null)
            {
                Assert.True(commitsContainer.Items.Count <= limit, $"Expected at most {limit} commits, got {commitsContainer.Items.Count}");
            }
        }
    }
    
    [Fact]
    public void GetGitCommitsDetailed_WithInvalidRepo_ReturnsEmptyList()
    {
        // Arrange
        string invalidRepoPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        // Act
        string jsonResult = GitHelper.GetGitCommitsDetailed(invalidRepoPath);
        var commits = JsonSerializer.Deserialize<GitHelper.GitCommitsContainer>(jsonResult);

        // Assert
        Assert.Empty(commits.Items);
    }

    [Fact]
    public void GetGitTagsDetailed_WithValidRepo_ReturnsTags()
    {
        // Skip the test if not running in a Git repository
        if (!IsRunningInGitRepository())
        {
            _output.WriteLine("Test skipped: Not running in a Git repository");
            return;
        }

        // Arrange
        string repoPath = Directory.GetCurrentDirectory();
        while (!Directory.Exists(Path.Combine(repoPath, ".git")) && repoPath.Length > 3)
        {
            repoPath = Directory.GetParent(repoPath)?.FullName ?? "";
            if (string.IsNullOrEmpty(repoPath))
                break;
        }
        _output.WriteLine($"Using Git repository at: {repoPath}");

        // Act
        string jsonResult = GitHelper.GetGitTagsDetailed(repoPath);
        var tagsContainer = JsonSerializer.Deserialize<GitHelper.GitTagsContainer>(jsonResult);
        
        // Assert - Check for null first
        Assert.NotNull(tagsContainer);
        if (tagsContainer != null)
        {
            Assert.NotNull(tagsContainer.Items);
            if (tagsContainer.Items != null)
            {
                // Output results for debugging
                _output.WriteLine($"Found {tagsContainer.Items.Count} tags");
                
                // If tags are found, check the first one's properties
                if (tagsContainer.Items.Count > 0)
                {   
                    _output.WriteLine($"First tag: {tagsContainer.Items[0]?.Name}");
                    
                    var firstTag = tagsContainer.Items[0];
                    Assert.NotNull(firstTag.Name);
                    Assert.NotNull(firstTag.Hash);
                    Assert.NotNull(firstTag.CommitAuthor);
                    Assert.NotNull(firstTag.Type);
                }
            }
        }
    }

    [Fact]
    public void GetGitTagsDetailed_WithLimit_ReturnsLimitedTags()
    {
        // Skip the test if not running in a Git repository
        if (!IsRunningInGitRepository())
        {
            _output.WriteLine("Test skipped: Not running in a Git repository");
            return;
        }

        // Arrange
        string repoPath = Directory.GetCurrentDirectory();
        while (!Directory.Exists(Path.Combine(repoPath, ".git")) && repoPath.Length > 3)
        {
            repoPath = Directory.GetParent(repoPath)?.FullName ?? "";
            if (string.IsNullOrEmpty(repoPath))
                break;
        }
        int limit = 2; // Limit to 2 tags

        // Act
        string jsonResult = GitHelper.GetGitTagsDetailed(repoPath, limit);
        var tagsContainer = JsonSerializer.Deserialize<GitHelper.GitTagsContainer>(jsonResult);

        // Assert - Check for null first
        Assert.NotNull(tagsContainer);
        if (tagsContainer != null)
        {
            Assert.NotNull(tagsContainer.Items);
            if (tagsContainer.Items != null)
            {
                // Output results for debugging
                _output.WriteLine($"Requested {limit} tags, found {tagsContainer.Items.Count}");

                // Assert - The number of tags should be less than or equal to the limit
                Assert.True(tagsContainer.Items.Count <= limit);
            }
        }
    }

    [Fact]
    public void GetGitTagsDetailed_WithInvalidRepo_ReturnsEmptyList()
    {
        // Arrange
        string invalidRepoPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        // Act
        string jsonResult = GitHelper.GetGitTagsDetailed(invalidRepoPath);
        var tagsContainer = JsonSerializer.Deserialize<GitHelper.GitTagsContainer>(jsonResult);

        // Assert
        Assert.Empty(tagsContainer.Items);
    }
    

    
    [Fact]
    public void GetGitReleasesDetailed_WithValidRepo_ReturnsReleases()
    {
        // Skip the test if not running in a Git repository
        if (!IsRunningInGitRepository())
        {
            _output.WriteLine("Test skipped: Not running in a Git repository");
            return;
        }

        // Arrange
        string repoPath = Directory.GetCurrentDirectory();
        while (!Directory.Exists(Path.Combine(repoPath, ".git")) && repoPath.Length > 3)
        {
            repoPath = Directory.GetParent(repoPath)?.FullName ?? "";
            if (string.IsNullOrEmpty(repoPath))
                break;
        }
        _output.WriteLine($"Using Git repository at: {repoPath}");

        // Act
        string jsonResult = GitHelper.GetGitReleasesDetailed(repoPath);
        var releasesContainer = JsonSerializer.Deserialize<ReleasesContainer>(jsonResult);
        
        // Assert - Check for null first
        Assert.NotNull(releasesContainer);
        if (releasesContainer != null)
        {
            Assert.NotNull(releasesContainer.Items);
            if (releasesContainer.Items != null)
            {
                // Output results for debugging
                _output.WriteLine($"Found {releasesContainer.Items.Count} releases");
                
                // If releases are found, check the first one's properties
                if (releasesContainer.Items.Count > 0)
                {   
                    _output.WriteLine($"First release: {releasesContainer.Items[0]?.Name}");
                    
                    var firstRelease = releasesContainer.Items[0];
                    Assert.NotNull(firstRelease.Name);
                    Assert.NotNull(firstRelease.Hash);
                    Assert.NotNull(firstRelease.CommitAuthor);
                    Assert.NotNull(firstRelease.Type);
                }
            }
        }
    }
    
    [Fact]
    public void GetGitReleasesDetailed_WithLimit_ReturnsLimitedReleases()
    {
        // Skip the test if not running in a Git repository
        if (!IsRunningInGitRepository())
        {
            _output.WriteLine("Test skipped: Not running in a Git repository");
            return;
        }

        // Arrange
        string repoPath = Directory.GetCurrentDirectory();
        while (!Directory.Exists(Path.Combine(repoPath, ".git")) && repoPath.Length > 3)
        {
            repoPath = Directory.GetParent(repoPath)?.FullName ?? "";
            if (string.IsNullOrEmpty(repoPath))
                break;
        }
        int limit = 2; // Limit to 2 releases

        // Act
        string jsonResult = GitHelper.GetGitReleasesDetailed(repoPath, limit);
        var releasesContainer = JsonSerializer.Deserialize<ReleasesContainer>(jsonResult);

        // Assert - Check for null first
        Assert.NotNull(releasesContainer);
        if (releasesContainer != null)
        {
            Assert.NotNull(releasesContainer.Items);
            if (releasesContainer.Items != null)
            {
                // Output results for debugging
                _output.WriteLine($"Requested {limit} releases, found {releasesContainer.Items.Count}");

                // Assert - The number of releases should be less than or equal to the limit
                Assert.True(releasesContainer.Items.Count <= limit);
            }
        }
    }
    
    [Fact]
    public void GetGitReleasesDetailed_WithInvalidRepo_ReturnsEmptyList()
    {
        // Arrange
        string invalidRepoPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        // Act
        string jsonResult = GitHelper.GetGitReleasesDetailed(invalidRepoPath);
        var releasesContainer = JsonSerializer.Deserialize<ReleasesContainer>(jsonResult);

        // Assert
        Assert.NotNull(releasesContainer);
        if (releasesContainer != null)
        {
            Assert.Empty(releasesContainer.Items);
        }
    }
}
