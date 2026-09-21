using JetBrains.Annotations;

namespace Core.Registries.Exceptions;

/// <summary>
/// Specifies the specific failure reason when loading Minecraft registry data.
/// </summary>
public enum RegistryErrorKind
{
    /// <summary>The base directory containing registry files does not exist.</summary>
    DirectoryNotFound,

    /// <summary>A required registry JSON file is missing on disk.</summary>
    FileNotFound,

    /// <summary>The registry JSON file contains invalid or corrupted JSON syntax.</summary>
    InvalidJson,

    /// <summary>The JSON file was parsed but produced an empty or null collection.</summary>
    EmptyPayload
}

/// <summary>
/// Exception thrown when Minecraft registry data fails to load or deserialize properly.
/// </summary>
[PublicAPI]
public class RegistryLoadException : Exception
{
    /// <summary>
    /// Gets the name of the problematic file (e.g., "items.json").
    /// </summary>
    public string? FileName { get; }

    /// <summary>
    /// Gets the full filesystem path to the file or directory that caused the failure.
    /// </summary>
    public string? TargetPath { get; }

    /// <summary>
    /// Gets the categorized failure classification.
    /// </summary>
    public RegistryErrorKind Kind { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RegistryLoadException"/> class.
    /// </summary>
    /// <param name="message">The human-readable description of the error.</param>
    /// <param name="kind">The failure category.</param>
    /// <param name="fileName">The registry file name.</param>
    /// <param name="targetPath">The absolute path to the file.</param>
    /// <param name="innerException">The original caught exception, if any.</param>
    public RegistryLoadException(
        string message,
        RegistryErrorKind kind,
        string? fileName = null,
        string? targetPath = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        Kind = kind;
        FileName = fileName;
        TargetPath = targetPath;
    }
}