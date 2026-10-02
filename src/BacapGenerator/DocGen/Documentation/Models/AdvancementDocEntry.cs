using System.Text.Json.Serialization;

namespace BacapGenerator.DocGen.Documentation.Models;

public class AdvancementDocEntry
{
    [JsonPropertyName("title")] public required string Title { get; init; }
    [JsonPropertyName("description")] public required string Description { get; init; }
    [JsonPropertyName("icon_id")] public required string IconId { get; init; }
    [JsonPropertyName("player_head_data")] public PlayerHeadDocEntry? PlayerHeadData { get; init; }
    [JsonPropertyName("alternative_descriptions")] public Dictionary<string, string>? AlternativeDescriptions { get; init; }
    [JsonPropertyName("tier")] public required string Tier { get; init; }
    [JsonPropertyName("tab")] public required string Tab { get; init; }
    [JsonPropertyName("mc_path")] public required string McPath { get; init; }
    [JsonPropertyName("parent")] public string? Parent { get; init; }
    [JsonPropertyName("rewards")] public RewardsDocEntry? Rewards { get; init; }
    [JsonPropertyName("requirements")] public Dictionary<string, string>? Requirements { get; init; }
}