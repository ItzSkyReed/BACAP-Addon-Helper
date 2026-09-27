using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Core.Registries.Models;

/// <summary>
/// Represents the hex, argb variant of dye color
/// </summary>
[UsedImplicitly]
public sealed record DyeColorRegistryEntry
{
    [JsonPropertyName("hex")] public required string HexVariant { get; init; }

    [JsonPropertyName("argb")] public required string ArgbVariant { get; init; }
}