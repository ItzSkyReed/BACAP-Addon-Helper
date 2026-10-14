using System.Text.Json.Serialization;

namespace BacapGenerator.DocGen.Documentation.Models;

public class RewardsDocEntry
{
    [JsonPropertyName("experience")] public int? Experience { get; init; }
    [JsonPropertyName("items")] public List<ItemRewardDocEntry>? Items { get; init; }
    [JsonPropertyName("trophies")] public List<TrophyDocEntry>? Trophies { get; init; }
}