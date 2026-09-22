using BacapGenerator.Configuration.Exceptions;
using BacapGenerator.Utils;
using Core.Registries;
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

    /// <summary>
    /// Validates category identifiers, targets, selectors, and verifies entities and prefix colors against registries.
    /// </summary>
    /// <param name="datapackId">The parent datapack identifier.</param>
    /// <param name="checklistId">The owning checklist identifier.</param>
    /// <param name="minecraftData">The static Minecraft registries container.</param>
    /// <exception cref="DatapackConfigurationException">Thrown when category settings or registry references are invalid.</exception>
    /// <example>
    /// <code>
    /// category.Validate("bacaped", "mob_universe", minecraftData);
    /// </code>
    /// </example>
    public void Validate(string datapackId, string checklistId, MinecraftData minecraftData)
    {
        ArgumentNullException.ThrowIfNull(minecraftData);

        if (string.IsNullOrWhiteSpace(Name))
        {
            throw new DatapackConfigurationException(
                datapackId,
                DatapackErrorKind.InvalidChecklistConfiguration,
                $"Checklist '{checklistId}' contains a category with an empty or missing '{nameof(Name)}'.",
                nameof(Name));
        }

        if (Name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            throw new DatapackConfigurationException(
                datapackId,
                DatapackErrorKind.InvalidChecklistConfiguration,
                $"Category name '{Name}' in checklist '{checklistId}' contains invalid filesystem characters.",
                Name);
        }

        if (string.IsNullOrWhiteSpace(Advancement))
        {
            throw new DatapackConfigurationException(
                datapackId,
                DatapackErrorKind.InvalidChecklistConfiguration,
                $"Category '{Name}' in checklist '{checklistId}' must specify an '{nameof(Advancement)}'.",
                nameof(Advancement));
        }

        if (Entities.Count == 0)
        {
            throw new DatapackConfigurationException(
                datapackId,
                DatapackErrorKind.InvalidChecklistConfiguration,
                $"Category '{Name}' in checklist '{checklistId}' does not define any tracked entities.",
                nameof(Entities));
        }

        foreach (var entity in Entities)
        {
            if (string.IsNullOrWhiteSpace(entity))
            {
                throw new DatapackConfigurationException(
                    datapackId,
                    DatapackErrorKind.InvalidChecklistConfiguration,
                    $"Category '{Name}' in checklist '{checklistId}' contains empty or whitespace entity identifiers.",
                    nameof(Entities));
            }

            var trimmed = entity.Trim();

            // Skip preset shortcuts (e.g. "@all_overworld", "@baby_zoo")
            if (trimmed.StartsWith('@'))
                continue;

            // Normalize namespace: "minecraft:cow" -> "cow"
            var namespaceStrippedEntity = MinecraftUtils.StripNamespace(entity);

            if (!minecraftData.Entities.ContainsKey(namespaceStrippedEntity) && !minecraftData.Entities.ContainsKey(trimmed))
            {
                throw new DatapackConfigurationException(
                    datapackId,
                    DatapackErrorKind.InvalidChecklistConfiguration,
                    $"Category '{Name}' in checklist '{checklistId}' references unknown entity '{trimmed}'. It is not registered in Minecraft data.",
                    trimmed);
            }
        }

        if (PrefixColor is not null)
            ChecklistStyleSettings.ValidateColor(PrefixColor, nameof(PrefixColor), datapackId, checklistId, minecraftData);

        if (ExtraSelector is not null)
            ValidateExtraSelector(ExtraSelector, Name, datapackId, checklistId);
    }

    private static void ValidateExtraSelector(string selector, string categoryName, string datapackId, string checklistId)
    {
        if (string.IsNullOrWhiteSpace(selector))
        {
            throw new DatapackConfigurationException(
                datapackId,
                DatapackErrorKind.InvalidChecklistConfiguration,
                $"Category '{categoryName}' in checklist '{checklistId}' has an empty or whitespace '{nameof(ExtraSelector)}'.",
                nameof(ExtraSelector));
        }

        var trimmed = selector.Trim();
        if (trimmed.StartsWith(',') || trimmed.StartsWith('@') || trimmed.StartsWith('['))
        {
            throw new DatapackConfigurationException(
                datapackId,
                DatapackErrorKind.InvalidChecklistConfiguration,
                $"Extra selector '{selector}' in category '{categoryName}' of checklist '{checklistId}' must not start with ',', '@' or '['.",
                nameof(ExtraSelector));
        }
    }
}