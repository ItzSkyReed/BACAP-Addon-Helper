using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;

namespace BacapGenerator.Datapacks.Models.Settings.Checklists;

/// <summary>
/// Configuration for a specific sub-category or dimension group within a checklist.
/// </summary>
public sealed class ChecklistCategorySettings
{
    /// <summary>
    /// Gets the unique category identifier (e.g., "overworld", "nether", "baby_zoo").
    /// Used for file naming.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// Gets the Minecraft resource path of the target advancement granted upon completion
    /// (e.g., "bacaped:challenges/mob_universe overworld").
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("advancement")]
    public required string Advancement { get; init; }

    /// <summary>
    /// Gets the list of entity IDs to track for this category (e.g. "cow", "pig").
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("entities")]
    public List<string> Entities { get; init; } = [];

    /// <summary>
    /// Gets the optional dimension ID required for displaying this category (e.g., "minecraft:the_nether").
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("dimension_requirement")]
    public string? DimensionRequirement { get; init; }

    /// <summary>
    /// Gets the optional category title/prefix shown before the entity list.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("prefix_text")]
    public string? PrefixText { get; init; }

    /// <summary>
    /// Gets the color for the category prefix label.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("prefix_color")]
    public string? PrefixColor { get; init; }

    /// <summary>
    /// Gets an optional category-level selector override to append to the entity query.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("extra_selector")]
    public string? ExtraSelector { get; init; }
}