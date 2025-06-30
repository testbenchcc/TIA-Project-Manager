# ConfigHelper Usage Guide

This document provides detailed information on how to use the ConfigHelper library in your TIA-Project-Manager application.

## Basic Usage

### Loading Configuration

To load or create a configuration file:

```csharp
using ConfigHelper;

// Specify the path to your configuration file
string configPath = Path.Combine(AppContext.BaseDirectory, "sectionDataObject.json");

// Load or create the configuration file
RepoConfig config = await ConfigManager.LoadOrCreateAsync(configPath);
```

### Accessing Configuration Data

Once loaded, you can access the configuration data through the strongly-typed objects:

```csharp
// Access repositories
foreach (var repo in config.Repo)
{
    // Access repository properties
    string repoName = repo.Name;
    string repoPath = repo.Path;
    int buildNumber = repo.BuildNumber;
    
    // Access sections
    foreach (var section in repo.Sections)
    {
        // Access section properties
        string sectionName = section.Name;
        
        // Access section-specific data
        if (section.Name == "Dashboard")
        {
            // Access commits
            if (section.Commits != null)
            {
                int commitLimit = section.Commits.Limit;
                var commitItems = section.Commits.Items;
            }
            
            // Access tags
            if (section.Tags != null)
            {
                foreach (var tag in section.Tags)
                {
                    // Process tags
                }
            }
        }
        else if (section.Name == "Project Configuration")
        {
            // Access devices
            if (section.Devices != null)
            {
                foreach (var device in section.Devices)
                {
                    string deviceName = device.Name;
                    string deviceIp = device.Ip;
                }
            }
            
            // Access project details
            string dNumber = section.DNumber ?? "";
            string tNumber = section.TNumber ?? "";
            string projectName = section.ProjectName ?? "";
        }
        else if (section.Name == "Datablocks")
        {
            // Access datablocks
            if (section.Datablocks != null)
            {
                foreach (var datablock in section.Datablocks)
                {
                    string datablockName = datablock.Name;
                    string dbNumber = datablock.Db;
                    var tags = datablock.Tags;
                }
            }
        }
    }
}
```

### Modifying Configuration Data

You can modify the configuration data and save it back to disk:

```csharp
// Create a new repository
var newRepo = new Repo
{
    Name = "New Repository",
    Path = @"C:\Path\To\Repository",
    Branch = "main",
    BuildNumber = 1,
    Sections = new List<ConfigHelper.Section>
    {
        new ConfigHelper.Section 
        { 
            Name = "Dashboard", 
            Icon = "dashboard-icon"
        },
        new ConfigHelper.Section 
        { 
            Name = "Project Configuration",
            Icon = "config-icon",
            Devices = new List<Device>()
        }
    }
};

// Add the repository to the configuration
config.Repo.Add(newRepo);

// Save the updated configuration
await ConfigManager.SaveAsync(config, configPath);
```

### Error Handling

When working with the ConfigHelper library, it's important to handle exceptions that might occur during file operations:

```csharp
try
{
    // Load or create the configuration file
    RepoConfig config = await ConfigManager.LoadOrCreateAsync(configPath);
    
    // Work with the configuration...
    
    // Save changes
    await ConfigManager.SaveAsync(config, configPath);
}
catch (IOException ex)
{
    // Handle file access errors
    MessageBox.Show($"Error accessing configuration file: {ex.Message}");
}
catch (JsonException ex)
{
    // Handle JSON parsing errors
    MessageBox.Show($"Error parsing configuration file: {ex.Message}");
}
catch (Exception ex)
{
    // Handle other errors
    MessageBox.Show($"An unexpected error occurred: {ex.Message}");
}
```

## Advanced Usage

### Working with Sections

The ConfigHelper library allows for flexible management of different section types. Each section can contain specialized data relevant to its purpose:

- **Dashboard Section**: Contains commit history and tags
- **Project Configuration Section**: Contains device information and project details
- **Datablocks Section**: Contains datablock definitions
- **Memory Tags Section**: Contains memory tag definitions

To identify and work with specific sections:

```csharp
// Find a specific section by name
var dashboardSection = repo.Sections.FirstOrDefault(s => s.Name == "Dashboard");
if (dashboardSection != null)
{
    // Initialize commit information if it doesn't exist
    if (dashboardSection.Commits == null)
    {
        dashboardSection.Commits = new Commits { Limit = 10, Items = new List<CommitItem>() };
    }
    
    // Add a new commit
    dashboardSection.Commits.Items.Add(new CommitItem
    {
        Date = DateTime.Now.ToString("dd-MM-yy HH:mm"),
        Title = "New feature added",
        Body = "Added new functionality to the application",
        Author = "John Doe",
        Hash = "abc123def456"
    });
}
```

### Batch Updates

For performance reasons, it's best to make all your changes to the configuration object before saving it:

```csharp
// Load configuration
var config = await ConfigManager.LoadOrCreateAsync(configPath);

// Make multiple changes
config.Repo[0].BuildNumber++;

if (config.Repo[0].Sections.Any(s => s.Name == "Project Configuration"))
{
    var projConfigSection = config.Repo[0].Sections.First(s => s.Name == "Project Configuration");
    projConfigSection.ProjectName = "Updated Project Name";
    if (projConfigSection.Devices == null)
        projConfigSection.Devices = new List<Device>();
        
    projConfigSection.Devices.Add(new Device { Name = "new_device", Ip = "192.168.1.100" });
}

// Save all changes at once
await ConfigManager.SaveAsync(config, configPath);
```

## Best Practices

1. **Always check for null**: Many properties in the ConfigHelper objects are nullable. Always check for null before accessing these properties.

2. **Use async/await**: All file operations in ConfigHelper are asynchronous. Always use the `await` keyword when calling these methods.

3. **Handle exceptions**: File operations can throw exceptions. Always wrap your code in try-catch blocks to handle these exceptions gracefully.

4. **Keep the UI responsive**: When working with large configuration files, perform loading and saving operations asynchronously to avoid freezing the UI.

5. **Validate user input**: Always validate user input before adding it to the configuration to ensure data integrity.
