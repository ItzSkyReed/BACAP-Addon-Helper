using System.Text.Json;
using System.Text.Json.Serialization;
using BacapGenerator.Advancements.Models;
using Core.Advancements.Models;

namespace BacapGenerator.Generation;

public static partial class MilestoneGenerator
{
    /// <summary>
    /// Generates milestone advancement criteria from a pre-filtered list of tab advancements.
    /// </summary>
    /// <param name="milestone">The milestone advancement wrapper to update.</param>
    /// <param name="tabAdvancements">The pre-filtered list of advancements belonging to this tab.</param>
    /// <returns>A modified <see cref="Advancement"/> instance containing all generated criteria.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="milestone"/> or <paramref name="tabAdvancements"/> is null.</exception>
    /// <example>
    /// <code>
    /// Advancement updated = MilestoneGenerator.GenerateMilestone(milestoneAdv, tabAdvancements);
    /// </code>
    /// </example>
    public static Advancement GenerateMilestone(BacapAdvancement milestone, IReadOnlyList<BacapAdvancement> tabAdvancements)
    {
        ArgumentNullException.ThrowIfNull(milestone);
        ArgumentNullException.ThrowIfNull(tabAdvancements);

        return milestone.Advancement with { Criteria = BuildCriteria(tabAdvancements) };
    }

    /// <summary>
    /// Generates the advancement legend criteria using all eligible advancements.
    /// </summary>
    /// <param name="legend">The legend advancement wrapper to update.</param>
    /// <param name="allValidAdvancements">The pre-filtered list of all advancements in the datapack.</param>
    /// <returns>A modified <see cref="Advancement"/> instance containing all generated criteria.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="legend"/> or <paramref name="allValidAdvancements"/> is null.</exception>
    public static Advancement GenerateAdvancementLegend(BacapAdvancement legend, IReadOnlyList<BacapAdvancement> allValidAdvancements) =>
        GenerateMilestone(legend, allValidAdvancements);

    /// <summary>
    /// Builds a dictionary of criteria by serializing conditions for each specified advancement.
    /// </summary>
    /// <param name="advancements">The source advancements to convert into criteria.</param>
    /// <returns>A dictionary mapping advancement file names without extension to their serialized <see cref="JsonElement"/> conditions.</returns>
    /// <exception cref="ArgumentException">Thrown when an advancement's <c>McPath</c> is null or whitespace.</exception>
    private static Dictionary<string, JsonElement> BuildCriteria(IReadOnlyList<BacapAdvancement> advancements)
    {
        var criteria = new Dictionary<string, JsonElement>(advancements.Count);
        var advMap = new Dictionary<string, bool>(1);

        foreach (var adv in advancements)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(adv.McPath);

            advMap.Clear();
            advMap[adv.McPath] = true;

            var payload = new CriterionPayload(
                "minecraft:location",
                new ConditionsNode(
                    new PlayerNode("minecraft:entity_properties", "this",
                        new PredicateNode(
                            new TypeSpecificNode(advMap)))));

            var key = Path.GetFileNameWithoutExtension(adv.File.Name);
            criteria[key] = JsonSerializer.SerializeToElement(payload, CriterionJsonContext.Default.CriterionPayload);
        }

        return criteria;
    }

    internal readonly record struct CriterionPayload(
        [property: JsonPropertyName("trigger")]
        string Trigger,
        [property: JsonPropertyName("conditions")]
        ConditionsNode Conditions);

    internal readonly record struct ConditionsNode(
        [property: JsonPropertyName("player")] PlayerNode Player);

    internal readonly record struct PlayerNode(
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("entity")] string Entity,
        [property: JsonPropertyName("predicate")]
        PredicateNode Predicate);

    internal readonly record struct PredicateNode(
        [property: JsonPropertyName("minecraft:type_specific/player")]
        TypeSpecificNode TypeSpecific);

    internal readonly record struct TypeSpecificNode(
        [property: JsonPropertyName("advancements")]
        Dictionary<string, bool> Advancements);

    [JsonSerializable(typeof(CriterionPayload))]
    private partial class CriterionJsonContext : JsonSerializerContext;
}