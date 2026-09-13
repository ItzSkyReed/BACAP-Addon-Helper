using System.Buffers;
using System.Text.Json;
using BacapGenerator.Models.Advancements;
using Core.Advancements.Models;

namespace BacapGenerator.Services.Generators;

public static class MilestoneGenerator
{
    /// <summary>
    /// Generates milestone advancement criteria from a pre-filtered list of tab advancements.
    /// </summary>
    /// <param name="milestone">The milestone advancement wrapper to update.</param>
    /// <param name="tabAdvancements">The pre-filtered list of advancements belonging to this tab.</param>
    /// <returns>A modified <see cref="Advancement"/> instance containing all generated criteria.</returns>
    /// <example>
    /// <code>
    /// var updatedMilestone = MilestoneGenerator.GenerateMilestone(milestoneAdv, tabAdvancements);
    /// </code>
    /// </example>
    public static Advancement GenerateMilestone(BacapAdvancement milestone, IReadOnlyList<BacapAdvancement> tabAdvancements)
    {
        return milestone.Advancement with
        {
            Criteria = BuildCriteria(tabAdvancements)
        };
    }

    /// <summary>
    /// Generates the advancement legend criteria using all eligible advancements.
    /// </summary>
    /// <param name="legend">The legend advancement wrapper to update.</param>
    /// <param name="allValidAdvancements">The pre-filtered list of all advancements in the datapack.</param>
    /// <returns>A modified <see cref="Advancement"/> instance containing all generated criteria.</returns>
    /// <example>
    /// <code>
    /// var updatedLegend = MilestoneGenerator.GenerateAdvancementLegend(legendAdv, allAdvancements);
    /// </code>
    /// </example>
    public static Advancement GenerateAdvancementLegend(BacapAdvancement legend, IReadOnlyList<BacapAdvancement> allValidAdvancements)
    {
        return legend.Advancement with
        {
            Criteria = BuildCriteria(allValidAdvancements)
        };
    }

    /// <summary>
    /// Builds a dictionary of criteria by serializing conditions for each specified advancement.
    /// </summary>
    /// <param name="advancements">The source advancements to convert into criteria.</param>
    /// <returns>A dictionary mapping advancement file names without extension to their parsed <see cref="JsonElement"/> conditions.</returns>
    private static Dictionary<string, JsonElement> BuildCriteria(IReadOnlyList<BacapAdvancement> advancements)
    {
        var criteria = new Dictionary<string, JsonElement>(advancements.Count);
        var bufferWriter = new ArrayBufferWriter<byte>();
        using var writer = new Utf8JsonWriter(bufferWriter);

        foreach (var adv in advancements)
        {
            bufferWriter.Clear();
            writer.Reset();

            WriteAdvancementCriterion(writer, adv.McPath);

            writer.Flush();

            using var doc = JsonDocument.Parse(bufferWriter.WrittenMemory);
            criteria[Path.GetFileNameWithoutExtension(adv.File.Name)] = doc.RootElement.Clone();
        }

        return criteria;
    }

    /// <summary>
    /// Writes the Minecraft location trigger criterion structure requiring a specific advancement to the JSON stream.
    /// </summary>
    /// <param name="writer">The active <see cref="Utf8JsonWriter"/> instance.</param>
    /// <param name="mcPath">The Minecraft namespaced ID of the required advancement.</param>
    private static void WriteAdvancementCriterion(Utf8JsonWriter writer, string mcPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mcPath);

        // Writes:
        // "trigger": "minecraft:location",
        // "conditions": {
        //     "player": {
        //         "type": "minecraft:entity_properties",
        //         "entity": "this",
        //         "predicate": {
        //             "minecraft:type_specific/player": {
        //                 "advancements": {
        //                     "<advancement_mcpath>": true
        //                 }
        //             }
        //         }
        //     }
        // }
        writer.WriteStartObject();
        writer.WriteString("trigger", "minecraft:location");

        writer.WriteStartObject("conditions");
        writer.WriteStartObject("player");
        writer.WriteString("type", "minecraft:entity_properties");
        writer.WriteString("entity", "this");
        writer.WriteStartObject("predicate");
        writer.WriteStartObject("minecraft:type_specific/player");
        writer.WriteStartObject("advancements");

        writer.WriteBoolean(mcPath, true);

        writer.WriteEndObject(); // advancements
        writer.WriteEndObject(); // minecraft:type_specific/player
        writer.WriteEndObject(); // predicate
        writer.WriteEndObject(); // player
        writer.WriteEndObject(); // conditions

        writer.WriteEndObject(); // root criterion object
    }
}