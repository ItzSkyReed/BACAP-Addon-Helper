using System.Text.Json.Serialization;

namespace Core.Registries.Models;

/// <summary>
/// Represents the hex, argb variant of dye color
/// </summary>
public record DyeColorRegistryEntry
{
    [JsonPropertyName("hex")] public required string HexVariant { get; init; }

    [JsonPropertyName("argb")] public required string ArgbVariant { get; init; }
}