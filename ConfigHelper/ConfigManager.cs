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
        var builder = new ConfigurationBuilder()
                     .AddJsonFile(filePath, optional: false, reloadOnChange: false);
        var root = builder.Build();
        return root.Get<RepoConfig>() ?? new RepoConfig();
    }

    /// <summary>Saves the object back to disk.</summary>
    public static async Task SaveAsync(RepoConfig config, string filePath)
    {
        var json = JsonSerializer.Serialize(config, _jsonOpts);
        await File.WriteAllTextAsync(filePath, json);
    }
}
