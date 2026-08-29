using System.Text.Json.Serialization;

namespace Core.Registries.Models;

/// <summary>
/// Represents the available variants for a specific block type.
/// </summary>
public record BlockRegistryEntry
{
    [JsonPropertyName("display_name")] public required string DisplayName { get; init; }
}