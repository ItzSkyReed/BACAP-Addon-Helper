using System.Text.Json.Serialization;

namespace BacapGenerator.McMeta;

/// <summary>
/// A pattern used in the pack filter.
/// </summary>
public record PackFilterPattern
{
    /// <summary>
    /// A regular expression for the namespace of files to be filtered out.
    /// If unspecified, it applies to every namespace.
    /// </summary>
    [JsonPropertyName("namespace")]
    public string? Namespace { get; init; }

    /// <summary>
    /// A regular expression for the paths of files to be filtered out.
    /// If unspecified, it applies to every file.
    /// </summary>
    [JsonPropertyName("path")]
    public string? Path { get; init; }
}