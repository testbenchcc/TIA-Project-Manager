# ConfigHelper Library

## Overview

The ConfigHelper library provides a simple and efficient way to manage configuration data in JSON format for the TIA-Project-Manager application. It handles loading, saving, and manipulation of configuration data through strongly-typed C# objects.

## Key Features

- Automatic JSON serialization and deserialization
- Strongly-typed configuration objects
- Automatic file creation if configuration doesn't exist
- Pretty-printed JSON output

## Main Components

### ConfigManager

A static class that provides methods for loading and saving configuration data.

#### Methods

- `LoadOrCreateAsync(string filePath)`: Loads an existing configuration file or creates a new one if it doesn't exist.
- `LoadAsync(string filePath)`: Loads an existing JSON configuration file.
- `SaveAsync(RepoConfig config, string filePath)`: Saves the configuration data to a JSON file.

### RepoConfig

The root configuration class that represents the entire configuration file.

#### Properties

- `Repo`: A list of repository configurations.

### Repo

Represents a single repository configuration.

#### Properties

- `Name`: The name of the repository.
- `Path`: The file path to the repository.
- `Branch`: The current branch name.
- `BuildNumber`: The current build number.
- `Sections`: A list of sections within the repository.

### Section

Represents a section within a repository.

#### Properties

- `Name`: The name of the section.
- `Icon`: The icon associated with the section.
- `Commits`: (Optional) Commit information for the section.
- `Tags`: (Optional) Tags associated with the section.
- `Devices`: (Optional) Devices associated with the section.
- `DNumber`: (Optional) The D-number for the section.
- `TNumber`: (Optional) The T-number for the section.
- `ProjectName`: (Optional) The project name for the section.
- `Datablocks`: (Optional) Datablocks associated with the section.

## Usage Example

```csharp
using ConfigHelper;
using System;
using System.Threading.Tasks;

// Path to the configuration file
string configPath = Path.Combine(AppContext.BaseDirectory, "sectionDataObject.json");

// Load or create the configuration file
var config = await ConfigManager.LoadOrCreateAsync(configPath);

// Access repository information
foreach (var repo in config.Repo)
{
    Console.WriteLine($"Repository: {repo.Name}");
    Console.WriteLine($"Location: {repo.Path}");
    Console.WriteLine($"Branch: {repo.Branch}");
    
    // Access sections
    foreach (var section in repo.Sections)
    {
        Console.WriteLine($"Section: {section.Name}");
    }
}

// Modify configuration
if (config.Repo.Count > 0)
{
    // Increment build number
    config.Repo[0].BuildNumber++;
    
    // Save changes
    await ConfigManager.SaveAsync(config, configPath);
}
```

## Dependencies

- Microsoft.Extensions.Configuration.Json (9.0.6)
- Microsoft.Extensions.Configuration.Binder (9.0.6)

## License

[Specify your license information here]
