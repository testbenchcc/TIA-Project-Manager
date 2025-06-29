using System;
using System.Diagnostics;

namespace GitHelper;

public static class GitHelper
{
    // Label for goto statement
    private const string DefaultBuildNumber = "1";
    /// <summary>
    /// Gets the Git build number (commit count) for a specific branch in a repository.
    /// </summary>
    /// <param name="repoPath">Path to the Git repository</param>
    /// <param name="branch">Branch name (defaults to "master")</param>
    /// <returns>The number of commits in the branch, or "1" if an error occurs</returns>
    public static string GetGitBuildNumber(string repoPath, string branch = "master")
    {
        try
        {
            // First check if the branch exists
            var checkBranchInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = $"show-ref --verify --quiet refs/heads/{branch}",
                WorkingDirectory = repoPath,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            try
            {
                var checkProcess = Process.Start(checkBranchInfo);
                if (checkProcess == null)
                {
                    Console.WriteLine("Failed to start git process for branch check, falling back to HEAD");
                    branch = "HEAD";
                }
                else
                {
                    using (checkProcess)
                    {
                        checkProcess.WaitForExit();
                        if (checkProcess.ExitCode != 0)
                        {
                            Console.WriteLine($"Branch '{branch}' not found, falling back to HEAD");
                            branch = "HEAD"; // Fall back to HEAD if branch doesn't exist
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking branch: {ex.Message}, falling back to HEAD");
                branch = "HEAD"; // Fall back to HEAD on error
            }
            
            var startInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = $"rev-list --count {branch}",
                WorkingDirectory = repoPath,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            var process = Process.Start(startInfo);
            if (process == null)
            {
                Console.WriteLine("Failed to start git process for rev-count");
                return DefaultBuildNumber; // Return default if process couldn't start
            }
                
            using (process)
            {
                string output = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit();
                
                if (process.ExitCode == 0 && !string.IsNullOrEmpty(output))
                {
                    Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Got Git build number: {output}");
                    return output;
                }
                else
                {
                    Console.WriteLine($"Error getting Git build number. Exit code: {process.ExitCode}");
                    // Fallback to a default value
                    return DefaultBuildNumber;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception getting Git build number: {ex.Message}");
            // Fallback to a default value
            return "1";
        }
    }
    
    /// <summary>
    /// Gets detailed Git commit information for a specific branch in a repository.
    /// </summary>
    /// <param name="repoPath">Path to the Git repository</param>
    /// <param name="branch">Branch name (defaults to "master")</param>
    /// <returns>A tuple containing commit hash, timestamp, and commit message</returns>
    public static (string commitHash, string timestamp, string message) GetGitCommitInfo(string repoPath, string branch = "master")
    {
        try
        {
            // First check if the branch exists
            var checkBranchInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = $"show-ref --verify --quiet refs/heads/{branch}",
                WorkingDirectory = repoPath,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            try
            {
                using (var checkProcess = Process.Start(checkBranchInfo))
                {
                    checkProcess.WaitForExit();
                    if (checkProcess.ExitCode != 0)
                    {
                        Console.WriteLine($"Branch '{branch}' not found, falling back to HEAD");
                        branch = "HEAD"; // Fall back to HEAD if branch doesn't exist
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking branch: {ex.Message}, falling back to HEAD");
                branch = "HEAD"; // Fall back to HEAD on error
            }
            
            // Get commit hash
            var hashInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = $"log -1 --format=\"%h\" {branch}",
                WorkingDirectory = repoPath,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            string commitHash = "Unknown";
            using (var process = Process.Start(hashInfo))
            {
                commitHash = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit();
                
                if (process.ExitCode != 0 || string.IsNullOrEmpty(commitHash))
                {
                    Console.WriteLine($"Error getting Git commit hash. Exit code: {process.ExitCode}");
                    commitHash = "Unknown";
                }
            }
            
            // Get timestamp
            var timestampInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = $"log -1 --format=\"%ci\" {branch}",
                WorkingDirectory = repoPath,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            string timestamp = "Unknown";
            using (var process = Process.Start(timestampInfo))
            {
                timestamp = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit();
                
                if (process.ExitCode != 0 || string.IsNullOrEmpty(timestamp))
                {
                    Console.WriteLine($"Error getting Git timestamp. Exit code: {process.ExitCode}");
                    timestamp = "Unknown";
                }
            }
            
            // Get commit message
            var messageInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = $"log -1 --format=\"%s\" {branch}",
                WorkingDirectory = repoPath,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            string message = "Unknown";
            using (var process = Process.Start(messageInfo))
            {
                message = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit();
                
                if (process.ExitCode != 0 || string.IsNullOrEmpty(message))
                {
                    Console.WriteLine($"Error getting Git commit message. Exit code: {process.ExitCode}");
                    message = "Unknown";
                }
            }
            
            Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Got Git commit info: Hash={commitHash}, Timestamp={timestamp}, Message={message}");
            return (commitHash, timestamp, message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception getting Git commit info: {ex.Message}");
            // Fallback to default values
            return ("Unknown", "Unknown", "Unknown");
        }
    }
}
