using System.Text.Json.Serialization;

namespace Core.Registries.Models;

/// <summary>
/// Represents the available variants for a specific potion type.
/// </summary>
public record PotionRegistryEntry
{
    [JsonPropertyName("long")] public required bool HasLongVariant { get; init; }

    [JsonPropertyName("strong")] public required bool HasStrongVariant { get; init; }
}