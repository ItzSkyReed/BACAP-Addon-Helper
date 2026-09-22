using System.Text.Json.Serialization;

namespace Core.Registries.Models;

/// <summary>
/// Represents the available variants for a specific item type.
/// </summary>
public record EntityRegistryEntry
{
    [JsonPropertyName("display_name")] public required string DisplayName { get; init; }

    [JsonPropertyName("translation_key")] public required string TranslationKey { get; init; }

    [JsonPropertyName("category")] public required string Category { get; init; }

    [JsonPropertyName("summonable")] public required bool Summonable { get; init; }
}