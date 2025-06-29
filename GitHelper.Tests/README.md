# GitHelper Tests

This directory contains unit tests for the GitHelper class, which provides Git-related functionality for the TIA Project Manager.

## Test Overview

The test suite includes the following tests:

### GetGitBuildNumber Tests

1. **GetGitBuildNumber_WithValidRepo_ReturnsNumber**: Tests that the GetGitBuildNumber method returns a valid build number when given a valid Git repository path.

2. **GetGitBuildNumber_WithNonExistentBranch_FallsBackToHead**: Tests that the method falls back to HEAD when given a non-existent branch name.

3. **GetGitBuildNumber_WithInvalidRepo_ReturnsDefault**: Tests that the method returns the default value "1" when given an invalid repository path.

### GetGitCommitInfo Tests

4. **GetGitCommitInfo_WithValidRepo_ReturnsInfo**: Tests that the GetGitCommitInfo method returns valid commit information (hash, timestamp, and message) when given a valid Git repository path.

5. **GetGitCommitInfo_WithNonExistentBranch_FallsBackToHead**: Tests that the method falls back to HEAD when given a non-existent branch name and still returns valid commit information.

6. **GetGitCommitInfo_WithInvalidRepo_ReturnsDefaults**: Tests that the method returns the default values ("Unknown" for hash, timestamp, and message) when given an invalid repository path.

### GetGitTags Tests

7. **GetGitTags_WithValidRepo_ReturnsTags**: Tests that the GetGitTags method returns a valid JSON array of tags when given a valid Git repository path.

8. **GetGitTags_WithLimit_ReturnsLimitedTags**: Tests that the method respects the limit parameter and returns at most the specified number of tags.

9. **GetGitTags_WithInvalidRepo_ReturnsEmptyList**: Tests that the method returns an empty JSON array when given an invalid repository path.

### GetGitTagsDetailed Tests

10. **GetGitTagsDetailed_WithValidRepo_ReturnsTags**: Verifies that the GetGitTagsDetailed method returns valid tag information from a Git repository.
11. **GetGitTagsDetailed_WithLimit_ReturnsLimitedTags**: Tests that the GetGitTagsDetailed method respects the limit parameter.
12. **GetGitTagsDetailed_WithInvalidRepo_ReturnsEmptyList**: Confirms that an empty list of tags is returned when an invalid repository path is provided.

### GetGitReleasesDetailed Tests

13. **GetGitReleasesDetailed_WithValidRepo_ReturnsReleases**: Verifies that the GetGitReleasesDetailed method returns valid release information from a Git repository.
14. **GetGitReleasesDetailed_WithLimit_ReturnsLimitedReleases**: Tests that the GetGitReleasesDetailed method respects the limit parameter.
15. **GetGitReleasesDetailed_WithInvalidRepo_ReturnsEmptyList**: Confirms that an empty list of releases is returned when an invalid repository path is provided.

### GetGitReleases Tests

16. **GetGitReleases_WithValidRepo_ReturnsReleases**: Tests that the GetGitReleases method returns a valid JSON array of releases when given a valid Git repository path, and verifies that each release follows the expected format.

17. **GetGitReleases_WithLimit_ReturnsLimitedReleases**: Tests that the method respects the limit parameter and returns at most the specified number of releases.

18. **GetGitReleases_WithInvalidRepo_ReturnsEmptyList**: Tests that the method returns an empty JSON array when given an invalid repository path.

### GetGitCommitsDetailed Tests

13. **GetGitCommitsDetailed_WithValidRepo_ReturnsCommits**: Tests that the GetGitCommitsDetailed method returns valid commit data (hash, author, date, title, and body) when given a valid Git repository path.

14. **GetGitCommitsDetailed_WithLimit_ReturnsLimitedCommits**: Tests that the method respects the limit parameter and returns at most the specified number of commits.

15. **GetGitCommitsDetailed_WithInvalidRepo_ReturnsEmptyList**: Tests that the method returns an empty list of commits when given an invalid repository path.

### GetGitTagsDetailed Tests

16. **GetGitTagsDetailed_WithValidRepo_ReturnsTags**: Tests that the GetGitTagsDetailed method returns valid tag data (name, date, hash, message, commitAuthor, tagger, and type) when given a valid Git repository path.

17. **GetGitTagsDetailed_WithLimit_ReturnsLimitedTags**: Tests that the method respects the limit parameter and returns at most the specified number of tags.

18. **GetGitTagsDetailed_WithInvalidRepo_ReturnsEmptyList**: Tests that the method returns an empty list of tags when given an invalid repository path.

## Running the Tests

### Using .NET CLI

You can run the tests from the command line using the `dotnet test` command:

```powershell
# Navigate to the solution directory
cd path\to\TIA-Project-Manager

# Run all tests in the GitHelper.Tests project
dotnet test GitHelper.Tests\GitHelper.Tests.csproj

# Run with detailed output
dotnet test GitHelper.Tests\GitHelper.Tests.csproj --logger "console;verbosity=detailed"

# Run a specific test
dotnet test GitHelper.Tests\GitHelper.Tests.csproj --filter "FullyQualifiedName=GitHelper.Tests.GitHelperTests.GetGitBuildNumber_WithValidRepo_ReturnsNumber"

# Run all GetGitCommitInfo tests
dotnet test GitHelper.Tests\GitHelper.Tests.csproj --filter "FullyQualifiedName~GetGitCommitInfo"
```

### Using Visual Studio

1. Open the solution in Visual Studio
2. Right-click on the GitHelper.Tests project in Solution Explorer
3. Select "Run Tests"

Alternatively, use the Test Explorer:
1. Open Test Explorer (View → Test Explorer)
2. Click "Run All" or right-click on specific tests to run them individually

### Using Visual Studio Code

If you're using VS Code with the C# extension:
1. Open the test file
2. Click on the "Run Test" or "Debug Test" links that appear above each test method

## Test Requirements

The tests are designed to be robust and will automatically detect if they're running in a Git repository. If not, certain tests will be skipped with appropriate messages.

## Test Output

The tests use `ITestOutputHelper` to output information during test execution. When running with detailed logging, you'll see:
- Whether a Git repository was found
- The path to the Git repository
- The build number retrieved (for GetGitBuildNumber tests)
- Commit hash, timestamp, and message (for GetGitCommitInfo tests)
- List of tags found (for GetGitTags tests)
- List of releases found (for GetGitReleases tests)
- Detailed commit information including hash, author, date, title, and body (for GetGitCommitsDetailed tests)
- Any branch fallback messages

## Troubleshooting

If tests are failing, check:
1. That Git is installed and available in your PATH
2. That you're running the tests from within a Git repository (or a parent directory of one)
3. That the Git repository has at least one commit
