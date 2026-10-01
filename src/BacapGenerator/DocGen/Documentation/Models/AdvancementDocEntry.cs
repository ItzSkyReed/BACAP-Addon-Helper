using System.Text.Json.Serialization;

namespace BacapGenerator.DocGen.Documentation.Models;

public class AdvancementDocEntry
{
    [JsonPropertyName("title")] public required string Title { get; init; }
    [JsonPropertyName("description")] public required string Description { get; init; }
    [JsonPropertyName("tier")] public required string Tier { get; init; }
    [JsonPropertyName("tab")] public required string Tab { get; init; }
    [JsonPropertyName("mc_path")] public required string McPath { get; init; }
    [JsonPropertyName("parent")] public string? Parent { get; init; }
    [JsonPropertyName("rewards")] public RewardsDocEntry? Rewards { get; init; }
    [JsonPropertyName("requirements")] public Dictionary<string, string>? Requirements { get; init; }
}