using System.Text.Json.Serialization;

namespace BacapGenerator.Datapacks.McMeta;

/// <summary>
/// Represents the sub-packs applied over the standard contents.
/// </summary>
public record PackOverlays
{
    /// <summary>
    /// List of overlays. The first in the list is applied first.
    /// </summary>
    [JsonPropertyName("entries")]
    public required List<PackOverlayEntry> Entries { get; init; }
}