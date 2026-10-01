using BacapGenerator.Configuration;
using BacapGenerator.Configuration.Exceptions;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;

namespace BacapGenerator.Datapacks.Models.Settings;

/// <summary>
/// Configuration settings and translation discovery filters for a datapack's language resource pack.
/// </summary>
public class DocumentGeneratorSettings
{
    /// <summary>
    /// Gets the optional name of the requirements section name.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("section_name")]
    public string SectionName { get; init; } = "default";


    [PublicAPI]
    [ConfigurationKeyName("enable")]
    public string? RawEnabled { get; init; }

    /// <summary>
    /// Gets a value indicating whether document generator is enabled for this datapacks.
    /// </summary>
    [PublicAPI]
    public bool Enabled { get; private set; }

    /// <summary>
    /// Validates document validation settings for consistency and correct path definitions.
    /// </summary>
    /// <param name="datapackId">The parent datapack identifier associated with these document settings.</param>
    /// <exception cref="DatapackConfigurationException">
    /// Thrown when the target path is missing, whitespace, contains invalid characters,
    /// or when ignored keys contain empty entries.
    /// </exception>
    /// <example>
    /// <code>
    /// settings.DocumentGeneratorSettings?.Validate("bacaped");
    /// </code>
    /// </example>
    public void Validate(string datapackId)
    {
        Enabled = ConfigParser.ParseBool(
            RawEnabled,
            defaultValue: false,
            datapackId,
            DatapackErrorKind.InvalidDocumentGeneratorSettings,
            $"Invalid boolean value '{RawEnabled}' for 'enable' in datapack '{datapackId}'.");

        if (string.IsNullOrWhiteSpace(SectionName))
        {
            throw new DatapackConfigurationException(
                datapackId,
                DatapackErrorKind.MissingLanguagePackPath,
                $"Documentation section name is missing or empty in datapack '{datapackId}'.",
                nameof(Path));
        }
    }
}