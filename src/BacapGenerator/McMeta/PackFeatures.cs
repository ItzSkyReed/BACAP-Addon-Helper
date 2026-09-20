using System.Text.Json.Serialization;

namespace BacapGenerator.McMeta;

/// <summary>
/// Represents experimental features enabled in the pack.
/// </summary>
public record PackFeatures
{
    /// <summary>
    /// List of enabled feature flags (Resource locations).
    /// </summary>
    [JsonPropertyName("enabled")]
    public required List<string> Enabled { get; init; }
}