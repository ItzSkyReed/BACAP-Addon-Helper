using System.Text.Json.Serialization;
using Core.TextComponents.Components;

namespace BacapGenerator.Datapacks.McMeta;

/// <summary>
/// Core information about the pack.
/// </summary>
public record PackInformation
{
    /// <summary>
    /// A text component that appears when hovering over the pack's name.
    /// Can be a plain string, a JSON object, or a JSON array.
    /// </summary>
    [JsonPropertyName("description")]
    public required TextComponent? Description { get; init; }

    /// <summary>
    /// Specifies the minimum version supported.
    /// </summary>
    [JsonPropertyName("min_format")]
    [JsonConverter(typeof(PackVersionConverter))]
    public required PackVersion MinFormat { get; init; }

    /// <summary>
    /// Specifies the maximum version supported.
    /// </summary>
    [JsonPropertyName("max_format")]
    [JsonConverter(typeof(PackVersionConverter))]
    public required PackVersion MaxFormat { get; init; }
}