# ConfigHelper Troubleshooting Guide

This document provides solutions to common issues that you might encounter when using the ConfigHelper library.

## Common Issues and Solutions

### Configuration File Not Found

**Issue:** When trying to load a configuration file, you receive a `FileNotFoundException`.

**Solution:**
- Verify that the file path is correct.
- Use `LoadOrCreateAsync` instead of `LoadAsync` to automatically create the file if it doesn't exist.
- Check file permissions to ensure your application has access to read from the specified location.

```csharp
// Instead of:
var config = await ConfigManager.LoadAsync(path); // Throws if file doesn't exist

// Use:
var config = await ConfigManager.LoadOrCreateAsync(path); // Creates file if it doesn't exist
```

### Invalid JSON Format

**Issue:** When loading a configuration file, you receive a `JsonException`.

**Solution:**
- Check that the JSON in your configuration file is valid.
- Use a JSON validator to identify and fix syntax errors.
- Ensure your configuration file uses the correct structure according to the RepoConfig class.

### Null Reference Exceptions

**Issue:** When accessing properties of the configuration objects, you encounter `NullReferenceException`.

**Solution:**
- Always check for null before accessing nullable properties.
- Use the null-conditional operator (`?.`) when accessing properties that might be null.
- Initialize collections before using them.

```csharp
// Instead of:
foreach (var tag in section.Tags) // May throw if Tags is null

// Use:
if (section.Tags != null)
{
    foreach (var tag in section.Tags)
    {
        // Process tag
    }
}

// Or use null-conditional operator with null-coalescing operator:
foreach (var tag in section.Tags ?? Enumerable.Empty<Tag>())
{
    // Process tag
}
```

### Cannot Save Configuration

**Issue:** When trying to save the configuration, you encounter an `IOException`.

**Solution:**
- Check that the directory exists. If not, create it before saving.
- Verify that your application has write permissions to the specified location.
- Ensure that the file is not locked by another process.

```csharp
// Create directory if it doesn't exist
string directory = Path.GetDirectoryName(filePath);
if (!Directory.Exists(directory))
{
    Directory.CreateDirectory(directory);
}

// Then save the configuration
await ConfigManager.SaveAsync(config, filePath);
```

### Type Conversion Issues

**Issue:** When deserializing the JSON, properties have unexpected values or are null.

**Solution:**
- Ensure that the property names in your JSON match the property names in your C# classes (case-sensitive).
- Check that the JSON property values have the correct data types.
- Verify that your JsonSerializerOptions are configured correctly.

### Memory Usage Concerns

**Issue:** The application uses excessive memory when working with large configuration files.

**Solution:**
- Consider implementing pagination or lazy loading for large collections.
- Process sections one at a time instead of loading everything into memory.
- Use a streaming approach for very large files.

## Debugging Tips

### Enable Debug Logging

Add debug logging to help identify issues:

```csharp
try
{
    Console.WriteLine($"Attempting to load configuration from: {filePath}");
    var config = await ConfigManager.LoadOrCreateAsync(filePath);
    Console.WriteLine($"Successfully loaded configuration with {config.Repo.Count} repositories");
}
catch (Exception ex)
{
    Console.WriteLine($"Error loading configuration: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
}
```

### Check File Contents

If you suspect the JSON file is corrupted, inspect its contents:

```csharp
string jsonContent = File.ReadAllText(filePath);
Console.WriteLine($"File content: {jsonContent}");
```

### Verify ConfigHelper Version

Ensure you're using a compatible version of the library:

```csharp
var assembly = typeof(ConfigManager).Assembly;
var version = assembly.GetName().Version;
Console.WriteLine($"ConfigHelper version: {version}");
```

## How to Report Issues

If you encounter issues that aren't addressed in this guide:

1. Gather all relevant information (error message, stack trace, configuration file content).
2. Document the steps to reproduce the issue.
3. Check if the issue is already reported in the issue tracker.
4. If not, create a new issue with the gathered information.

## Performance Optimization

If you're experiencing performance issues:

1. Use asynchronous methods properly with `await` to avoid blocking threads.
2. Consider caching the configuration if it's accessed frequently.
3. Only save the configuration when necessary, not after every small change.
4. Use a memory profiler to identify bottlenecks in your code.
