using System.Text.Json.Serialization;

namespace BacapGenerator.DocGen.Documentation.Models;

public class ItemRewardDocEntry
{
    [JsonPropertyName("id")] public required string Id { get; init; }
    [JsonPropertyName("count")] public required int Count { get; init; }
    [JsonPropertyName("enchantments")] public Dictionary<string, int>? Enchantments { get; init; }
    [JsonPropertyName("custom_name")] public string? CustomName { get; init; }
}