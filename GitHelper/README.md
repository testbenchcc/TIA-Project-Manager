# GitHelper

GitHelper is a utility class that provides Git-related functionality for the TIA Project Manager. It enables interaction with Git repositories through a simple C# API.

## Features

GitHelper offers the following features:

- **Build Number Retrieval**: Get the commit count for a branch as a build number
- **Commit Information**: Retrieve hash, timestamp, and message for the latest commit in a branch
- **Tag Management**: Get a list of tags from a repository with optional limiting
- **Release Identification**: Get a list of release tags (tags that follow the versioning format)
- **Detailed Commit History**: Retrieve detailed commit information including author, date, title, and body
- **Detailed Tag Information**: Get comprehensive information about tags including name, date, hash, message, authors, and type
- **Detailed Release Information**: Get comprehensive information about release tags including name, date, hash, message, authors, and type

## Usage

### Getting Build Number

Retrieves the number of commits in a branch as a build number:

```csharp
// Using default branch (master)
string buildNumber = GitHelper.GetGitBuildNumber(@"C:\path\to\repository");

// Specifying a branch
string buildNumber = GitHelper.GetGitBuildNumber(@"C:\path\to\repository", "develop");
```

If the specified branch doesn't exist, it will fall back to HEAD. If an error occurs, it returns "1" as the default value.

### Getting Commit Information

Retrieves detailed information about the latest commit in a branch:

```csharp
// Using default branch (master)
var (commitHash, timestamp, message) = GitHelper.GetGitCommitInfo(@"C:\path\to\repository");

// Specifying a branch
var (commitHash, timestamp, message) = GitHelper.GetGitCommitInfo(@"C:\path\to\repository", "develop");
```

If the specified branch doesn't exist, it will fall back to HEAD. If an error occurs, the values will be "Unknown".

### Getting Tags

Retrieves a list of tags from the repository as a JSON string:

```csharp
// Get all tags
string tagsJson = GitHelper.GetGitTags(@"C:\path\to\repository");

// Limit to a specific number of tags
string tagsJson = GitHelper.GetGitTags(@"C:\path\to\repository", 10);

// Deserializing the returned JSON
using System.Text.Json;
var tags = JsonSerializer.Deserialize<List<string>>(tagsJson);
```

Tags are sorted by creation date, with the newest first. If an error occurs, an empty list is returned.

### Getting Releases

Retrieves a list of release tags from the repository as a JSON string:

```csharp
// Get all releases
string releasesJson = GitHelper.GetGitReleases(@"C:\path\to\repository");

// Limit to a specific number of releases
string releasesJson = GitHelper.GetGitReleases(@"C:\path\to\repository", 5);

// Deserializing the returned JSON
using System.Text.Json;
var releases = JsonSerializer.Deserialize<List<string>>(releasesJson);
```

Release tags are identified as:
- Tags that start with 'v' followed by a number (e.g., "v1.0.0")
- Tags that start with a number (e.g., "1.0.0")

If an error occurs, an empty list is returned.

### Getting Detailed Commit History

Retrieves detailed information about multiple commits as a JSON string structured for use with the sectionDataObject.json file:

```csharp
// Get commits from the default branch with default limit (10)
string commitsJson = GitHelper.GetGitCommitsDetailed(@"C:\path\to\repository");

// Specify branch and limit
string commitsJson = GitHelper.GetGitCommitsDetailed(@"C:\path\to\repository", "develop", 5);

// Deserializing the returned JSON
using System.Text.Json;
var commitsContainer = JsonSerializer.Deserialize<GitHelper.GitCommitsContainer>(commitsJson);

// Accessing the commit information
foreach (var commit in commitsContainer.Items)
{
    Console.WriteLine($"Hash: {commit.Hash}");
    Console.WriteLine($"Author: {commit.Author}");
    Console.WriteLine($"Date: {commit.Date}");
    Console.WriteLine($"Title: {commit.Title}");
    Console.WriteLine($"Body: {commit.Body}");
}
```

This method returns a JSON string with the structure:
```json
{
  "limit": 10,
  "items": [
    {
      "date": "ISO date",
      "title": "Commit subject",
      "body": "Commit message body",
      "author": "Commit author name",
      "hash": "Commit hash"
    }
  ]
}
```

If the specified branch doesn't exist, it will fall back to HEAD. If an error occurs, an empty list of commits is returned.

### Getting Detailed Tag Information

Retrieves comprehensive information about tags in the repository as a JSON string:

```csharp
// Get all tags with detailed information
string tagsDetailedJson = GitHelper.GetGitTagsDetailed(@"C:\path\to\repository");

// Limit to a specific number of tags
string tagsDetailedJson = GitHelper.GetGitTagsDetailed(@"C:\path\to\repository", 5);

// Deserializing the returned JSON
using System.Text.Json;
var tagsContainer = JsonSerializer.Deserialize<GitHelper.GitTagsContainer>(tagsDetailedJson);

// Accessing the tag information
foreach (var tag in tagsContainer.Items)
{
    Console.WriteLine($"Name: {tag.Name}");
    Console.WriteLine($"Date: {tag.Date}");
    Console.WriteLine($"Hash: {tag.Hash}");
    Console.WriteLine($"Message: {tag.Message}");
    Console.WriteLine($"Commit Author: {tag.CommitAuthor}");
    Console.WriteLine($"Tagger: {tag.Tagger}");
    Console.WriteLine($"Type: {tag.Type}");
}
```

This method returns a JSON string with the structure:
```json
{
  "items": [
    {
      "name": "v1.0.0",
      "date": "2023-06-15T10:30:00Z",
      "hash": "abcdef123456",
      "message": "Version 1.0 release",
      "commitAuthor": "John Doe",
      "tagger": "Jane Smith",
      "type": "annotated"
    }
  ]
}
```

The method distinguishes between lightweight and annotated tags, providing the appropriate information for each type. If an error occurs, an empty list of tags is returned.

### Getting Detailed Release Information

Retrieves comprehensive information about release tags in the repository as a JSON string:

```csharp
// Get all release tags with detailed information
string releasesDetailedJson = GitHelper.GetGitReleasesDetailed(@"C:\path\to\repository");

// Limit to a specific number of release tags
string releasesDetailedJson = GitHelper.GetGitReleasesDetailed(@"C:\path\to\repository", 5);

// Deserializing the returned JSON
using System.Text.Json;
var releasesContainer = JsonSerializer.Deserialize<GitHelper.GitReleasesContainer>(releasesDetailedJson);

// Accessing the release information
foreach (var release in releasesContainer.Items)
{
    Console.WriteLine($"Name: {release.Name}");
    Console.WriteLine($"Date: {release.Date}");
    Console.WriteLine($"Hash: {release.Hash}");
    Console.WriteLine($"Message: {release.Message}");
    Console.WriteLine($"Commit Author: {release.CommitAuthor}");
    Console.WriteLine($"Releaser: {release.Releaser}");
    Console.WriteLine($"Type: {release.Type}");
}
```

This method returns a JSON string with the structure:
```json
{
  "items": [
    {
      "name": "v1.0.0",
      "date": "2023-06-15T10:30:00Z",
      "hash": "abcdef123456",
      "message": "Version 1.0 release",
      "commitAuthor": "John Doe",
      "releaser": "Jane Smith",
      "type": "annotated"
    }
  ]
}
```

A release tag is defined as a tag that starts with 'v' followed by a number or a tag that starts with a number. The method distinguishes between lightweight and annotated tags, providing the appropriate information for each type. If an error occurs, an empty list of releases is returned.

## Error Handling

All GitHelper methods include robust error handling:

1. If a specified branch doesn't exist, they fall back to HEAD
2. If a Git command fails, they return default or empty values
3. Console output is provided for troubleshooting

## Requirements

- Git must be installed and available in the PATH
- The provided repository paths must be valid Git repositories

## Testing

See the [GitHelper.Tests README](../GitHelper.Tests/README.md) for information on testing this class.
