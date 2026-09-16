using System.Text.Json.Serialization;

namespace BacapGenerator.Datapacks.McMeta;

/// <summary>
/// Defines a specific overlay sub-pack.
/// </summary>
public record PackOverlayEntry
{
    /// <summary>
    /// The directory to overlay (allowed characters: a-z, 0-9, _, -, and .).
    /// </summary>
    [JsonPropertyName("directory")]
    public required string Directory { get; init; }

    /// <summary>
    /// Specifies the minimum version to apply this overlay on.
    /// </summary>
    [JsonPropertyName("min_format")]
    [JsonConverter(typeof(PackVersionConverter))]
    public required PackVersion MinFormat { get; init; }

    /// <summary>
    /// Specifies the maximum version to apply this overlay on.
    /// </summary>
    [JsonPropertyName("max_format")]
    [JsonConverter(typeof(PackVersionConverter))]
    public required PackVersion MaxFormat { get; init; }
}