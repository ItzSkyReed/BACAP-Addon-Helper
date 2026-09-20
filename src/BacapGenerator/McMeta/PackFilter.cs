using System.Text.Json.Serialization;

namespace BacapGenerator.McMeta;

/// <summary>
/// Represents filters for excluding files from lower-priority packs.
/// </summary>
public record PackFilter
{
    /// <summary>
    /// List of patterns to treat as if they were not present in the pack.
    /// </summary>
    [JsonPropertyName("block")]
    public required List<PackFilterPattern> Block { get; init; }
}