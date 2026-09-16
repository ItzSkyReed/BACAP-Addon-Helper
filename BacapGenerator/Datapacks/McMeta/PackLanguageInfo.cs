using System.Text.Json.Serialization;

namespace BacapGenerator.Datapacks.McMeta;

/// <summary>
/// Information for a custom language defined in a resource pack.
/// </summary>
public record PackLanguageInfo
{
    /// <summary>
    /// The full name of the language.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// The country or region name.
    /// </summary>
    [JsonPropertyName("region")]
    public required string Region { get; init; }

    /// <summary>
    /// If true, the language reads right to left.
    /// </summary>
    [JsonPropertyName("bidirectional")]
    public bool Bidirectional { get; init; }
}