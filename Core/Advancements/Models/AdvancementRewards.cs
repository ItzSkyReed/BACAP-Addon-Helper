using System.Text.Json.Serialization;

namespace Core.Advancements.Models;

/// <summary>
/// Represents the rewards provided when an advancement is obtained.
/// </summary>
public record AdvancementRewards
{
    /// <summary>
    /// The amount of experience to give. Defaults to 0.
    /// </summary>
    [JsonPropertyName("experience")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int Experience { get; init; } = 0;

    /// <summary>
    /// The identifiers of recipes to unlock.
    /// </summary>
    [JsonPropertyName("recipes")]
    public List<string>? Recipes { get; init; }

    /// <summary>
    /// The identifiers of loot tables to give items from.
    /// </summary>
    [JsonPropertyName("loot")]
    public List<string>? Loot { get; init; }

    /// <summary>
    /// The identifier of a function to run. Function tags are not allowed.
    /// </summary>
    [JsonPropertyName("function")]
    public string? Function { get; init; }
}