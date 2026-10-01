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
    [ConfigurationKeyName("registry_path")]
    public string RegistryPath { get; init; } = null!;

    /// <summary>
    /// Gets the target filesystem path where generated release archives will be written.
    /// </summary>
    [ConfigurationKeyName("release_path")]
    public string ReleasePath { get; init; } = null!;

    /// <summary>
    /// Gets the target filesystem path where generated release archives will be written.
    /// </summary>
    [ConfigurationKeyName("document_generator")]
    public DocumentGeneratorConfig? DocumentGenerator { get; init; }

    /// <summary>
    /// Validates all root configuration settings.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when validation constraints are violated.</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(RegistryPath))
            throw new InvalidOperationException("'registry_path' must be provided in config.yaml.");

        if (string.IsNullOrWhiteSpace(ReleasePath))
            throw new InvalidOperationException("'release_path' must be provided.");

        DocumentGenerator?.Validate();
    }
}