namespace BacapGenerator.Configuration.Exceptions;

/// <summary>
/// Specifies the specific violation kind in datapack settings.
/// </summary>
public enum DatapackErrorKind
{
    MissingPath,
    DirectoryNotFound,
    MissingNamespace,
    MissingParentDatapack,
    InvalidChecklistConfiguration,
    MissingLanguagePackPath,
    InvalidLanguagePackConfiguration,
    InvalidValidationRuleConfiguration
}

/// <summary>
/// Exception thrown when datapack configuration is semantically invalid or points to non-existent directories.
/// </summary>
public sealed class DatapackConfigurationException : Exception
{
    /// <summary>
    /// Gets the identifier/alias of the datapack in the configuration.
    /// </summary>
    public string DatapackId { get; }

    /// <summary>
    /// Gets the specific validation error kind.
    /// </summary>
    public DatapackErrorKind Kind { get; }

    /// <summary>
    /// Gets an optional detail payload (e.g. invalid path or missing field name).
    /// </summary>
    public string? Detail { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DatapackConfigurationException"/> class.
    /// </summary>
    /// <param name="datapackId">The datapack key from config.yaml.</param>
    /// <param name="kind">The failure category.</param>
    /// <param name="message">The readable error message.</param>
    /// <param name="detail">Specific problematic path or property value.</param>
    public DatapackConfigurationException(string datapackId, DatapackErrorKind kind, string message, string? detail = null)
        : base(message)
    {
        DatapackId = datapackId;
        Kind = kind;
        Detail = detail;
    }
}