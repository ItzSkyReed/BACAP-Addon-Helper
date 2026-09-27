using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Core.Registries.Models;

/// <summary>
/// Represents the available variants for a specific block type.
/// </summary>
[UsedImplicitly]
public sealed record BlockRegistryEntry
{
    [JsonPropertyName("display_name")] public required string DisplayName { get; init; }
}