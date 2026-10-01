using Microsoft.Extensions.Configuration;

namespace BacapGenerator.Configuration;

public class DocumentGeneratorConfig
{
    /// <summary>
    /// Gets the filesystem path to the directory actual containing requirements for advancements.
    /// </summary>
    [ConfigurationKeyName("requirements_directory")]
    public string? RequirementsDirectory { get; init; }

    /// <summary>
    /// Gets the filesystem path to the directory that contains generated documentation.
    /// </summary>
    [ConfigurationKeyName("output_directory")]
    public string? OutputDirectory { get; init; }

    /// <summary>
    /// Validates all document generator configuration settings.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when validation constraints are violated.</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(RequirementsDirectory))
            throw new InvalidOperationException("'requirements_directory' must not be null or empty if specified.");

        if (string.IsNullOrWhiteSpace(OutputDirectory))
            throw new InvalidOperationException("'output_directory' must not be null or empty if specified.");
    }
}