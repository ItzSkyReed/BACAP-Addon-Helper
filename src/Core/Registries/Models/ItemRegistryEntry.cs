using System.Text.Json.Serialization;

namespace Core.Registries.Models;

/// <summary>
/// Represents the available variants for a specific item type.
/// </summary>
public record ItemRegistryEntry
{
    [JsonPropertyName("display_name")] public required string DisplayName { get; init; }

    [JsonPropertyName("translation_key")] public required string TranslationKey { get; init; }

    [JsonPropertyName("stack_size")] public required int StackSize { get; init; }
}