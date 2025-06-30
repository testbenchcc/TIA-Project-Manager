using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace ConfigHelper;

public static class ConfigManager
{
    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>Loads the config or creates a blank one if the file is missing.</summary>
    public static async Task<RepoConfig> LoadOrCreateAsync(string filePath)
    {
        if (File.Exists(filePath))
            return await LoadAsync(filePath);

        var cfg = new RepoConfig();          // empty default
        await SaveAsync(cfg, filePath);      // create the file
        return cfg;
    }

    /// <summary>Loads existing JSON into a strongly-typed object.</summary>
    public static async Task<RepoConfig> LoadAsync(string filePath)
    {
        try
        {
            // Read the file content directly
            string jsonContent = await File.ReadAllTextAsync(filePath);
            
            // Use System.Text.Json with custom options to handle potential issues
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };
            
            var config = JsonSerializer.Deserialize<RepoConfig>(jsonContent, options);
            return config ?? new RepoConfig();
        }
        catch (JsonException ex)
        {
            // If there's an issue with the JSON format, create a new config
            System.Diagnostics.Debug.WriteLine($"Error deserializing config: {ex.Message}");
            return new RepoConfig();
        }
        catch (Exception ex)
        {
            // For any other issues, return a new config
            System.Diagnostics.Debug.WriteLine($"Error loading config: {ex.Message}");
            return new RepoConfig();
        }
    }

    /// <summary>Saves the object back to disk.</summary>
    public static async Task SaveAsync(RepoConfig config, string filePath)
    {
        var json = JsonSerializer.Serialize(config, _jsonOpts);
        await File.WriteAllTextAsync(filePath, json);
    }
}
