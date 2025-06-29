using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace GitHelper.Tests;

public class GitHelperTests
{
    private readonly ITestOutputHelper _output;
    
    public GitHelperTests(ITestOutputHelper output)
    {
        _output = output;
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
}
