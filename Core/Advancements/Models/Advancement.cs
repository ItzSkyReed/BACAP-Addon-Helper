using System.Text.Json;
using System.Text.Json.Serialization;
using Core.Serialization;

namespace Core.Advancements.Models;

/// <summary>
/// Represents the root of a Minecraft Advancement JSON file.
/// </summary>
public record Advancement
{
    [JsonPropertyName("display")] public AdvancementDisplay? Display { get; init; }

    [JsonPropertyName("parent")] public string? Parent { get; init; }

    /// <summary>
    /// A dictionary where the Key is the Criterion Name, and the Value is the raw JSON of the trigger/conditions.
    /// </summary>
    [JsonPropertyName("criteria")]
    public Dictionary<string, JsonElement> Criteria { get; init; } = new();

    /// <summary>
    /// Defines how the criteria are completed to grant the advancement.
    /// Contains sublists of criterion names. The advancement is granted if all sublists have at least one completed criterion.
    /// If absent, defaults to requiring all criteria to be completed.
    /// </summary>
    [JsonPropertyName("requirements")]
    public List<List<string>>? Requirements { get; init; }

    /// <summary>
    /// An object representing the rewards provided when this advancement is obtained.
    /// </summary>
    [JsonPropertyName("rewards")]
    public AdvancementRewards? Rewards { get; init; }

    [JsonPropertyName("sends_telemetry_event")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool SendsTelemetryEvent { get; init; }


    /// <summary>
    /// Deserializes a JSON string into an <see cref="Advancement"/> using standard Minecraft JSON formatting rules.
    /// </summary>
    /// <param name="json">The raw JSON string.</param>
    /// <returns>A deserialized <see cref="Advancement"/> instance.</returns>
    public static Advancement? Parse(string json)
    {
        return JsonSerializer.Deserialize<Advancement>(json, MinecraftDatapackJsonOptions.Default);
    }

    /// <summary>
    /// Serializes this advancement into a JSON string using standard Minecraft JSON formatting rules.
    /// </summary>
    /// <returns>A formatted JSON string.</returns>
    public string ToJson()
    {
        return JsonSerializer.Serialize(this, MinecraftDatapackJsonOptions.Default);
    }
}