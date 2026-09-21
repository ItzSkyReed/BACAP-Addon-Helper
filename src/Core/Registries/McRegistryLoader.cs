using System.Collections.Frozen;
using System.Text.Json;
using Core.Registries.Exceptions;
using JetBrains.Annotations;

namespace Core.Registries;

/// <summary>
/// Universal loader for Minecraft JSON registries.
/// </summary>
/// <param name="basePath">The directory where registry JSON files reside.</param>
public class McRegistryLoader(string basePath)
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Loads a registry JSON file into a frozen dictionary for optimized read access.
    /// </summary>
    /// <typeparam name="TEntry">The model class for the JSON values.</typeparam>
    /// <param name="fileName">The JSON file name (e.g., "items.json").</param>
    /// <returns>A read-only frozen dictionary of registry keys to their data entries.</returns>
    /// <exception cref="RegistryLoadException">Thrown when the file is missing or contains invalid JSON.</exception>
    /// <example>
    /// <code>
    /// var items = loader.LoadRegistry&lt;ItemRegistryEntry&gt;("items.json");
    /// </code>
    /// </example>
    [PublicAPI]
    public FrozenDictionary<string, TEntry> LoadRegistry<TEntry>(string fileName)
    {
        EnsureBaseDirectoryExists();

        var filePath = Path.Combine(basePath, fileName);
        EnsureFileExists(fileName, filePath);

        try
        {
            using var stream = File.OpenRead(filePath);
            var registry = JsonSerializer.Deserialize<Dictionary<string, TEntry>>(stream, _jsonOptions);

            if (registry is null || registry.Count == 0)
            {
                throw new RegistryLoadException(
                    $"Registry file '{fileName}' is empty or deserialized to null.",
                    RegistryErrorKind.EmptyPayload,
                    fileName,
                    filePath);
            }

            return registry.ToFrozenDictionary();
        }
        catch (JsonException ex)
        {
            throw new RegistryLoadException(
                $"Failed to parse registry file '{fileName}'. Invalid JSON syntax at line {ex.LineNumber}, byte {ex.BytePositionInLine}.",
                RegistryErrorKind.InvalidJson,
                fileName,
                filePath,
                ex);
        }
    }

    /// <summary>
    /// Loads a JSON array of strings into a frozen set for fast lookups.
    /// </summary>
    /// <param name="fileName">The JSON file name (e.g., "containers.json").</param>
    /// <returns>A read-only frozen set of strings.</returns>
    /// <exception cref="RegistryLoadException">Thrown when the file is missing or contains invalid JSON.</exception>
    /// <example>
    /// <code>
    /// var containers = loader.LoadRegistry("containers.json");
    /// </code>
    /// </example>
    [PublicAPI]
    public FrozenSet<string> LoadRegistry(string fileName)
    {
        EnsureBaseDirectoryExists();

        var filePath = Path.Combine(basePath, fileName);
        EnsureFileExists(fileName, filePath);

        try
        {
            using var stream = File.OpenRead(filePath);
            var items = JsonSerializer.Deserialize<HashSet<string>>(stream, _jsonOptions);

            if (items is null || items.Count == 0)
            {
                throw new RegistryLoadException(
                    $"Registry list file '{fileName}' is empty or deserialized to null.",
                    RegistryErrorKind.EmptyPayload,
                    fileName,
                    filePath);
            }

            return items.ToFrozenSet();
        }
        catch (JsonException ex)
        {
            throw new RegistryLoadException(
                $"Failed to parse registry list file '{fileName}'. Invalid JSON syntax at line {ex.LineNumber}, byte {ex.BytePositionInLine}.",
                RegistryErrorKind.InvalidJson,
                fileName,
                filePath,
                ex);
        }
    }

    private void EnsureBaseDirectoryExists()
    {
        if (!Directory.Exists(basePath))
        {
            throw new RegistryLoadException(
                "Registry directory was not found.",
                RegistryErrorKind.DirectoryNotFound,
                targetPath: Path.GetFullPath(basePath));
        }
    }

    private static void EnsureFileExists(string fileName, string fullPath)
    {
        if (!File.Exists(fullPath))
        {
            throw new RegistryLoadException(
                "Required registry file was not found.",
                RegistryErrorKind.FileNotFound,
                fileName,
                Path.GetFullPath(fullPath));
        }
    }
}