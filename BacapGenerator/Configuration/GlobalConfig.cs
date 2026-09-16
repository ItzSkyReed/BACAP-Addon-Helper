using Microsoft.Extensions.Configuration;

namespace BacapGenerator.Configuration;

/// <summary>
/// Represents the root application configuration mapped directly from the YAML settings file.
/// </summary>
public sealed class GlobalConfig
{

    /// <summary>
    /// Gets the filesystem path to the directory containing extracted Minecraft registry JSON files.
    /// </summary>
    [ConfigurationKeyName("registry_base_path")]
    public string? RegistryBasePath { get; init; }

    /// <summary>
    /// Gets the target filesystem path where generated release archives will be written.
    /// </summary>
    [ConfigurationKeyName("release_path")]
    public string? ReleasePath { get; init; }


    /// <summary>
    /// Validates all root configuration settings as well as each registered datapack.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when validation constraints are violated.</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(RegistryBasePath))
            throw new InvalidOperationException($"'{nameof(RegistryBasePath)}' must be provided.");

        if (string.IsNullOrWhiteSpace(ReleasePath))
            throw new InvalidOperationException($"'{nameof(ReleasePath)}' must be provided.");
    }
}