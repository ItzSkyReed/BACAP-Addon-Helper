using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Core.Registries.Models;

/// <summary>
/// Represents the available variants for a specific potion type.
/// </summary>
[UsedImplicitly]
public sealed record PotionRegistryEntry
{
    [JsonPropertyName("long")] public required bool HasLongVariant { get; init; }

    [JsonPropertyName("strong")] public required bool HasStrongVariant { get; init; }
}