using System.Collections.Frozen;
using System.Text.Json;

namespace Core.Registries;

/// <summary>
/// Universal loader for Minecraft JSON registries.
/// </summary>
public class McRegistryLoader(string basePath)
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true // So that "display_name" in JSON maps to DisplayName in C#
    };


    /// <summary>
    /// Loads a registry JSON file into a frozen dictionary for optimized read access.
    /// </summary>
    /// <typeparam name="TEntry">The model class for the JSON values.</typeparam>
    /// <param name="fileName">The JSON file name (e.g., "items.json").</param>
    /// <returns>A highly optimized, read-only frozen dictionary of registry keys to their data entries.</returns>
    public FrozenDictionary<string, TEntry> LoadRegistry<TEntry>(string fileName)
    {
        var filePath = Path.Combine(basePath, fileName);

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Registry file '{fileName}' not found at '{filePath}'.");

        using var stream = File.OpenRead(filePath);

        // Deserialize directly into a standard dictionary first
        var registry = JsonSerializer.Deserialize<Dictionary<string, TEntry>>(stream, _jsonOptions);

        // Freeze the dictionary for optimized reads
        return registry?.ToFrozenDictionary() ?? FrozenDictionary<string, TEntry>.Empty;
    }

    /// <summary>
    /// Loads a JSON array of strings into a frozen set for optimized fast-lookup.
    /// </summary>
    /// <param name="fileName">The JSON file name (e.g., "containers.json").</param>
    /// <returns>A highly optimized, read-only frozen set of strings.</returns>
    public FrozenSet<string> LoadRegistry(string fileName)
    {
        var filePath = Path.Combine(basePath, fileName);

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Registry array file '{fileName}' not found at '{filePath}'.");

        using var stream = File.OpenRead(filePath);

        // Deserialize JSON array directly into a List<string> or HashSet<string>
        var items = JsonSerializer.Deserialize<HashSet<string>>(stream, _jsonOptions);

        // Freeze the set for optimized read and Contains() checks
        return items?.ToFrozenSet() ?? FrozenSet<string>.Empty;
    }
}