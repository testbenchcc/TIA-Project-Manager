using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

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

    /// <summary>
    /// Gets a list of Git tags from a repository.
    /// </summary>
    /// <param name="repoPath">Path to the Git repository</param>
    /// <param name="limit">Maximum number of tags to return (0 for all)</param>
    /// <returns>A JSON string containing a list of tags</returns>
    public static string GetGitTags(string repoPath, int limit = 0)
    {
        try
        {
            var limitArg = limit > 0 ? $"-n {limit}" : "";
            var startInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = $"tag --sort=-creatordate {limitArg}",
                WorkingDirectory = repoPath,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            var process = Process.Start(startInfo);
            if (process == null)
            {
                Console.WriteLine("Failed to start git process for tags");
                return JsonSerializer.Serialize(new List<string>());
            }
                
            using (process)
            {
                var tags = new List<string>();
                string line;
                while ((line = process.StandardOutput.ReadLine()) != null)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        tags.Add(line.Trim());
                    }
                }
                process.WaitForExit();
                
                if (process.ExitCode == 0)
                {
                    Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Got {tags.Count} Git tags");
                    return JsonSerializer.Serialize(tags);
                }
                else
                {
                    Console.WriteLine($"Error getting Git tags. Exit code: {process.ExitCode}");
                    return JsonSerializer.Serialize(new List<string>());
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception getting Git tags: {ex.Message}");
            return JsonSerializer.Serialize(new List<string>());
        }
    }
    
    /// <summary>
    /// Gets a list of Git releases from a repository.
    /// A release is considered a tag that starts with 'v' followed by a number or is just a number.
    /// </summary>
    /// <param name="repoPath">Path to the Git repository</param>
    /// <param name="limit">Maximum number of releases to return (0 for all)</param>
    /// <returns>A JSON string containing a list of releases</returns>
    public static string GetGitReleases(string repoPath, int limit = 0)
    {
        try
        {
            // First get all tags
            var allTagsJson = GetGitTags(repoPath, 0);
            var allTags = JsonSerializer.Deserialize<List<string>>(allTagsJson) ?? new List<string>();
            
            // Filter for release tags (v1.0.0 or 1.0.0 format)
            var releases = new List<string>();
            foreach (var tag in allTags)
            {
                if (IsReleaseTag(tag))
                {
                    releases.Add(tag);
                    if (limit > 0 && releases.Count >= limit)
                    {
                        break;
                    }
                }
            }
            
            Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Got {releases.Count} Git releases");
            return JsonSerializer.Serialize(releases);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception getting Git releases: {ex.Message}");
            return JsonSerializer.Serialize(new List<string>());
        }
    }
    
    /// <summary>
    /// Determines if a tag is a release tag.
    /// A release tag either starts with 'v' followed by a number or is just a number.
    /// </summary>
    /// <param name="tag">The tag to check</param>
    /// <returns>True if the tag is a release tag, false otherwise</returns>
    private static bool IsReleaseTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            return false;
        }
        
        // Check if tag starts with 'v' followed by a number
        if (tag.StartsWith("v") && tag.Length > 1 && char.IsDigit(tag[1]))
        {
            return true;
        }
        
        // Check if tag is just a number or starts with a number followed by a dot
        if (tag.Length > 0 && char.IsDigit(tag[0]))
        {
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Represents a Git commit with detailed information.
    /// </summary>
    public class GitCommit
    {
        /// <summary>
        /// The date of the commit in ISO format (dd-MM-yy HH:mm).
        /// </summary>
        [JsonPropertyName("date")]
        public string Date { get; set; } = "";
        
        /// <summary>
        /// The commit message title/subject line.
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; } = "";
        
        /// <summary>
        /// The commit message body (everything after the first line).
        /// </summary>
        [JsonPropertyName("body")]
        public string Body { get; set; } = "";
        
        /// <summary>
        /// The author of the commit.
        /// </summary>
        [JsonPropertyName("author")]
        public string Author { get; set; } = "";
        
        /// <summary>
        /// The commit hash.
        /// </summary>
        [JsonPropertyName("hash")]
        public string Hash { get; set; } = "";
    }
    
    /// <summary>
    /// Container class for commits with a limit.
    /// </summary>
    public class GitCommitsContainer
    {
        /// <summary>
        /// Maximum number of commits to retrieve.
        /// </summary>
        [JsonPropertyName("limit")]
        public int Limit { get; set; } = 10;
        
        /// <summary>
        /// List of commit objects.
        /// </summary>
        [JsonPropertyName("items")]
        public List<GitCommit> Items { get; set; } = new List<GitCommit>();
    }
    
    /// <summary>
    /// Gets detailed information about multiple Git commits from a repository.
    /// </summary>
    /// <param name="repoPath">Path to the Git repository</param>
    /// <param name="branch">Branch name (defaults to "master")</param>
    /// <param name="limit">Maximum number of commits to return (default is 10)</param>
    /// <returns>A JSON string containing commit information matching the sectionDataObject.json format</returns>
    public static string GetGitCommitsDetailed(string repoPath, string branch = "master", int limit = 10)
    {
        var commitsContainer = new GitCommitsContainer
        {
            Limit = limit
        };
        
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
            
            // Get commit data using the git log command
            // Format each commit to include hash, author, date, subject, and body
            var startInfo = new ProcessStartInfo
            {
                FileName = "git",
                // Use alternate format to avoid escaping issues in different shells
                Arguments = $"log {branch} --format=%H^^^%an^^^%aI^^^%s^^^%b -n {limit}",
                WorkingDirectory = repoPath,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            var process = Process.Start(startInfo);
            if (process == null)
            {
                Console.WriteLine("Failed to start git process for commits");
                return JsonSerializer.Serialize(commitsContainer);
            }
                
            using (process)
            {
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                
                if (process.ExitCode == 0 && !string.IsNullOrEmpty(output))
                {
                    // Split the output by lines - each line is a separate commit
                    string[] commitLines = output.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    
                    foreach (string commitLine in commitLines)
                    {
                        // Process each commit using ^^^ as separator
                        string[] parts = commitLine.Split(new[] { "^^^" }, StringSplitOptions.None);
                        
                        if (parts.Length >= 4) // At minimum we need hash, author, date, and subject
                        {
                            var commit = new GitCommit
                            {
                                Hash = parts[0].Trim(),
                                Author = parts[1].Trim(),
                                Date = parts[2].Trim(),
                                Title = parts[3].Trim()
                            };
                            
                            // Body is the 5th part if it exists
                            if (parts.Length > 4 && !string.IsNullOrWhiteSpace(parts[4]))
                            {
                                commit.Body = parts[4].Trim();
                            }
                            
                            commitsContainer.Items.Add(commit);
                        }
                    }
                    
                    Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Got {commitsContainer.Items.Count} Git commits");
                }
                else
                {
                    Console.WriteLine($"Error getting Git commits. Exit code: {process.ExitCode}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception getting Git commits: {ex.Message}");
        }
        
        return JsonSerializer.Serialize(commitsContainer);
    }
    
    /// <summary>
    /// Represents a Git tag with detailed information.
    /// </summary>
    public class GitTag
    {
        /// <summary>
        /// The name of the tag.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";
        
        /// <summary>
        /// The date of the tag in ISO format.
        /// </summary>
        [JsonPropertyName("date")]
        public string Date { get; set; } = "";
        
        /// <summary>
        /// The commit hash that the tag points to.
        /// </summary>
        [JsonPropertyName("hash")]
        public string Hash { get; set; } = "";
        
        /// <summary>
        /// The tag message.
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; } = "";
        
        /// <summary>
        /// The author of the commit that the tag points to.
        /// </summary>
        [JsonPropertyName("commitAuthor")]
        public string CommitAuthor { get; set; } = "";
        
        /// <summary>
        /// The person who created the tag.
        /// </summary>
        [JsonPropertyName("tagger")]
        public string Tagger { get; set; } = "";
        
        /// <summary>
        /// The type of tag (lightweight or annotated).
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = "";
    }
    
    /// <summary>
    /// Container class for tags.
    /// </summary>
    public class GitTagsContainer
    {
        /// <summary>
        /// List of tag objects.
        /// </summary>
        [JsonPropertyName("items")]
        public List<GitTag> Items { get; set; } = new List<GitTag>();
    }
    
    /// <summary>
    /// Gets detailed information about Git tags from a repository.
    /// </summary>
    /// <param name="repoPath">Path to the Git repository</param>
    /// <param name="limit">Maximum number of tags to return (0 for all)</param>
    /// <returns>A JSON string containing tag information matching the sectionDataObject.json format</returns>
    public static string GetGitTagsDetailed(string repoPath, int limit = 0)
    {
        var tagsContainer = new GitTagsContainer();
        
        try
        {
            // First get all tag names
            var limitArg = limit > 0 ? $"-n {limit}" : "";
            var startInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = $"tag --sort=-creatordate {limitArg}",
                WorkingDirectory = repoPath,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            var process = Process.Start(startInfo);
            if (process == null)
            {
                Console.WriteLine("Failed to start git process for tags");
                return JsonSerializer.Serialize(tagsContainer);
            }
            
            var tagNames = new List<string>();
            using (process)
            {
                string line;
                while ((line = process.StandardOutput.ReadLine()) != null)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        tagNames.Add(line.Trim());
                    }
                }
                process.WaitForExit();
                
                if (process.ExitCode != 0)
                {
                    Console.WriteLine($"Error getting Git tags. Exit code: {process.ExitCode}");
                    return JsonSerializer.Serialize(tagsContainer);
                }
            }
            
            // For each tag, get detailed information
            foreach (var tagName in tagNames)
            {
                var tag = new GitTag { Name = tagName };
                
                // Check if the tag is lightweight or annotated
                var verifyAnnotatedInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = $"cat-file -t {tagName}",
                    WorkingDirectory = repoPath,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                
                using (var verifyProcess = Process.Start(verifyAnnotatedInfo))
                {
                    if (verifyProcess != null)
                    {
                        string tagType = verifyProcess.StandardOutput.ReadToEnd().Trim();
                        verifyProcess.WaitForExit();
                        
                        // If it's a tag object, it's an annotated tag, otherwise it's a lightweight tag (commit)
                        tag.Type = tagType == "tag" ? "annotated" : "lightweight";
                        
                        // Get different details based on tag type
                        if (tag.Type == "annotated")
                        {
                            // For annotated tags, we can get tagger, message, and date directly from the tag object
                            var tagInfoProcess = new ProcessStartInfo
                            {
                                FileName = "git",
                                Arguments = $"for-each-ref refs/tags/{tagName} --format='%(taggername)^^^%(taggerdate:iso8601)^^^%(contents:subject)'",
                                WorkingDirectory = repoPath,
                                RedirectStandardOutput = true,
                                UseShellExecute = false,
                                CreateNoWindow = true
                            };
                            
                            using (var infoProcess = Process.Start(tagInfoProcess))
                            {
                                if (infoProcess != null)
                                {
                                    string tagInfo = infoProcess.StandardOutput.ReadToEnd().Trim();
                                    infoProcess.WaitForExit();
                                    
                                    string[] parts = tagInfo.Split(new[] { "^^^" }, StringSplitOptions.None);
                                    if (parts.Length >= 3)
                                    {
                                        tag.Tagger = parts[0].Trim('\'', ' '); // Remove surrounding quotes
                                        tag.Date = parts[1].Trim('\'', ' ');   // Remove surrounding quotes
                                        tag.Message = parts[2].Trim('\'', ' '); // Remove surrounding quotes
                                    }
                                }
                            }
                        }
                        
                        // For both annotated and lightweight tags, get the commit hash and commit author
                        var commitInfoProcess = new ProcessStartInfo
                        {
                            FileName = "git",
                            Arguments = $"rev-list -n 1 {tagName}",
                            WorkingDirectory = repoPath,
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };
                        
                        using (var hashProcess = Process.Start(commitInfoProcess))
                        {
                            if (hashProcess != null)
                            {
                                tag.Hash = hashProcess.StandardOutput.ReadToEnd().Trim();
                                hashProcess.WaitForExit();
                                
                                if (!string.IsNullOrEmpty(tag.Hash))
                                {
                                    // Get author and date for lightweight tags or as fallback for commit info
                                    var authorProcess = new ProcessStartInfo
                                    {
                                        FileName = "git",
                                        Arguments = $"show --no-patch --format='%an^^^%aI' {tag.Hash}",
                                        WorkingDirectory = repoPath,
                                        RedirectStandardOutput = true,
                                        UseShellExecute = false,
                                        CreateNoWindow = true
                                    };
                                    
                                    using (var showProcess = Process.Start(authorProcess))
                                    {
                                        if (showProcess != null)
                                        {
                                            string commitDetails = showProcess.StandardOutput.ReadToEnd().Trim();
                                            showProcess.WaitForExit();
                                            
                                            string[] parts = commitDetails.Split(new[] { "^^^" }, StringSplitOptions.None);
                                            if (parts.Length >= 2)
                                            {
                                                tag.CommitAuthor = parts[0].Trim('\'', ' '); // Remove surrounding quotes
                                                
                                                // For lightweight tags, use commit date as tag date
                                                if (tag.Type == "lightweight")
                                                {
                                                    tag.Date = parts[1].Trim('\'', ' ');
                                                    tag.Tagger = tag.CommitAuthor; // For lightweight tags, tagger = commit author
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                
                // Add to container
                tagsContainer.Items.Add(tag);
            }
            
            Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Got {tagsContainer.Items.Count} Git tags with details");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception getting Git tags detailed: {ex.Message}");
        }
        
        return JsonSerializer.Serialize(tagsContainer);
    }
    
    /// <summary>
    /// Represents a Git release with detailed information.
    /// </summary>
    public class GitRelease
    {
        /// <summary>
        /// The name of the release tag.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";
        
        /// <summary>
        /// The date of the release in ISO format.
        /// </summary>
        [JsonPropertyName("date")]
        public string Date { get; set; } = "";
        
        /// <summary>
        /// The commit hash that the release tag points to.
        /// </summary>
        [JsonPropertyName("hash")]
        public string Hash { get; set; } = "";
        
        /// <summary>
        /// The release message.
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; } = "";
        
        /// <summary>
        /// The author of the commit that the release tag points to.
        /// </summary>
        [JsonPropertyName("commitAuthor")]
        public string CommitAuthor { get; set; } = "";
        
        /// <summary>
        /// The person who created the release.
        /// </summary>
        [JsonPropertyName("releaser")]
        public string Releaser { get; set; } = "";
        
        /// <summary>
        /// The type of release tag (lightweight or annotated).
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = "";
    }
    
    /// <summary>
    /// Container class for releases.
    /// </summary>
    public class GitReleasesContainer
    {
        /// <summary>
        /// List of release objects.
        /// </summary>
        [JsonPropertyName("items")]
        public List<GitRelease> Items { get; set; } = new List<GitRelease>();
    }
    
    /// <summary>
    /// Gets detailed information about Git release tags from a repository.
    /// Release tags are defined as tags that start with 'v' followed by a number or tags that start with a number.
    /// </summary>
    /// <param name="repoPath">Path to the Git repository</param>
    /// <param name="limit">Maximum number of releases to return (0 for all)</param>
    /// <returns>A JSON string containing release information matching the sectionDataObject.json format</returns>
    public static string GetGitReleasesDetailed(string repoPath, int limit = 0)
    {
        var releasesContainer = new GitReleasesContainer();
        
        try
        {
            // First get all tag names
            var limitArg = limit > 0 ? $"-n {limit}" : "";
            var startInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = $"tag --sort=-creatordate {limitArg}",
                WorkingDirectory = repoPath,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            var process = Process.Start(startInfo);
            if (process == null)
            {
                Console.WriteLine("Failed to start git process for tags");
                return JsonSerializer.Serialize(releasesContainer);
            }
            
            var tagNames = new List<string>();
            using (process)
            {
                string line;
                while ((line = process.StandardOutput.ReadLine()) != null)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        var tagName = line.Trim();
                        // Only add tags that are releases
                        if (IsReleaseTag(tagName))
                        {
                            tagNames.Add(tagName);
                        }
                    }
                }
                process.WaitForExit();
                
                if (process.ExitCode != 0)
                {
                    Console.WriteLine($"Error getting Git tags. Exit code: {process.ExitCode}");
                    return JsonSerializer.Serialize(releasesContainer);
                }
            }
            
            // Apply the limit after filtering, if needed
            if (limit > 0 && tagNames.Count > limit)
            {
                tagNames = tagNames.Take(limit).ToList();
            }
            
            // For each release tag, get detailed information
            foreach (var tagName in tagNames)
            {
                var release = new GitRelease { Name = tagName };
                
                // Check if the tag is lightweight or annotated
                var verifyAnnotatedInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = $"cat-file -t {tagName}",
                    WorkingDirectory = repoPath,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                
                using (var verifyProcess = Process.Start(verifyAnnotatedInfo))
                {
                    if (verifyProcess != null)
                    {
                        string tagType = verifyProcess.StandardOutput.ReadToEnd().Trim();
                        verifyProcess.WaitForExit();
                        
                        // If it's a tag object, it's an annotated tag, otherwise it's a lightweight tag (commit)
                        release.Type = tagType == "tag" ? "annotated" : "lightweight";
                        
                        // Get different details based on tag type
                        if (release.Type == "annotated")
                        {
                            // For annotated tags, we can get releaser, message, and date directly from the tag object
                            var tagInfoProcess = new ProcessStartInfo
                            {
                                FileName = "git",
                                Arguments = $"for-each-ref refs/tags/{tagName} --format='%(taggername)^^^%(taggerdate:iso8601)^^^%(contents:subject)'",
                                WorkingDirectory = repoPath,
                                RedirectStandardOutput = true,
                                UseShellExecute = false,
                                CreateNoWindow = true
                            };
                            
                            using (var infoProcess = Process.Start(tagInfoProcess))
                            {
                                if (infoProcess != null)
                                {
                                    string tagInfo = infoProcess.StandardOutput.ReadToEnd().Trim();
                                    infoProcess.WaitForExit();
                                    
                                    string[] parts = tagInfo.Split(new[] { "^^^" }, StringSplitOptions.None);
                                    if (parts.Length >= 3)
                                    {
                                        release.Releaser = parts[0].Trim('\'', ' '); // Remove surrounding quotes
                                        release.Date = parts[1].Trim('\'', ' ');   // Remove surrounding quotes
                                        release.Message = parts[2].Trim('\'', ' '); // Remove surrounding quotes
                                    }
                                }
                            }
                        }
                        
                        // For both annotated and lightweight tags, get the commit hash and commit author
                        var commitInfoProcess = new ProcessStartInfo
                        {
                            FileName = "git",
                            Arguments = $"rev-list -n 1 {tagName}",
                            WorkingDirectory = repoPath,
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };
                        
                        using (var hashProcess = Process.Start(commitInfoProcess))
                        {
                            if (hashProcess != null)
                            {
                                release.Hash = hashProcess.StandardOutput.ReadToEnd().Trim();
                                hashProcess.WaitForExit();
                                
                                if (!string.IsNullOrEmpty(release.Hash))
                                {
                                    // Get author and date for lightweight tags or as fallback for commit info
                                    var authorProcess = new ProcessStartInfo
                                    {
                                        FileName = "git",
                                        Arguments = $"show --no-patch --format='%an^^^%aI' {release.Hash}",
                                        WorkingDirectory = repoPath,
                                        RedirectStandardOutput = true,
                                        UseShellExecute = false,
                                        CreateNoWindow = true
                                    };
                                    
                                    using (var showProcess = Process.Start(authorProcess))
                                    {
                                        if (showProcess != null)
                                        {
                                            string commitDetails = showProcess.StandardOutput.ReadToEnd().Trim();
                                            showProcess.WaitForExit();
                                            
                                            string[] parts = commitDetails.Split(new[] { "^^^" }, StringSplitOptions.None);
                                            if (parts.Length >= 2)
                                            {
                                                release.CommitAuthor = parts[0].Trim('\'', ' '); // Remove surrounding quotes
                                                
                                                // For lightweight tags, use commit date as tag date
                                                if (release.Type == "lightweight")
                                                {
                                                    release.Date = parts[1].Trim('\'', ' ');
                                                    release.Releaser = release.CommitAuthor; // For lightweight tags, releaser = commit author
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                
                // Add to container
                releasesContainer.Items.Add(release);
            }
            
            Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Got {releasesContainer.Items.Count} Git releases with details");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception getting Git releases detailed: {ex.Message}");
        }
        
        return JsonSerializer.Serialize(releasesContainer);
    }
    
    /// <summary>
    /// Commit information class that matches the structure in sectionDataObject.json
    /// </summary>
    public class CommitInfo
    {
        [JsonPropertyName("date")]
        public string Date { get; set; } = "";
        
        [JsonPropertyName("title")]
        public string Title { get; set; } = "";
        
        [JsonPropertyName("body")]
        public string Body { get; set; } = "";
        
        [JsonPropertyName("author")]
        public string Author { get; set; } = "";
        
        [JsonPropertyName("hash")]
        public string Hash { get; set; } = "";
    }
    
    /// <summary>
    /// Gets a list of Git commits from a repository in the format needed for sectionDataObject.json
    /// </summary>
    /// <param name="repoPath">Path to the Git repository</param>
    /// <param name="branch">Branch name (defaults to "master")</param>
    /// <param name="limit">Maximum number of commits to return</param>
    /// <returns>A JSON string containing commit information</returns>
    public static string GetGitCommits(string repoPath, string branch = "master", int limit = 10)
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
            
            // Get commits
            var startInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = $"log {branch} --pretty=format:%H|%an|%ad|%s|%b --date=iso -n {limit} --no-merges",
                WorkingDirectory = repoPath,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            var commits = new List<CommitInfo>();
            var process = Process.Start(startInfo);
            if (process == null)
            {
                Console.WriteLine("Failed to start git process for commits");
                return JsonSerializer.Serialize(commits);
            }
                
            using (process)
            {
                string line;
                while ((line = process.StandardOutput.ReadLine()) != null)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        var parts = line.Split('|', 5);
                        if (parts.Length >= 4)
                        {
                            var commit = new CommitInfo
                            {
                                Hash = parts[0],
                                Author = parts[1],
                                Date = parts[2],
                                Title = parts[3],
                                Body = parts.Length > 4 ? parts[4].Trim() : ""
                            };
                            commits.Add(commit);
                        }
                    }
                }
                process.WaitForExit();
                
                if (process.ExitCode == 0)
                {
                    Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Got {commits.Count} Git commits");
                    return JsonSerializer.Serialize(commits);
                }
                else
                {
                    Console.WriteLine($"Error getting Git commits. Exit code: {process.ExitCode}");
                    return JsonSerializer.Serialize(new List<CommitInfo>());
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception getting Git commits: {ex.Message}");
            return JsonSerializer.Serialize(new List<CommitInfo>());
        }
    }
}
