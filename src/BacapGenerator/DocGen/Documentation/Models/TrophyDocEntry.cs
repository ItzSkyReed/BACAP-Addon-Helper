using System.Text.Json.Serialization;

namespace BacapGenerator.DocGen.Documentation.Models;

public class TrophyDocEntry
{
    [JsonPropertyName("id")] public required string Id { get; init; }
    [JsonPropertyName("count")] public required int Count { get; init; }
    [JsonPropertyName("title")] public string? Title { get; init; }
    [JsonPropertyName("description")] public string? Description { get; init; }
    [JsonPropertyName("title_color")] public string? TitleColor { get; init; }
    [JsonPropertyName("enchantments")] public Dictionary<string, int>? Enchantments { get; init; }
    [JsonPropertyName("unbreakable")] public bool Unbreakable { get; init; }
}