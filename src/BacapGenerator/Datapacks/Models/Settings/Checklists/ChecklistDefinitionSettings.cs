using BacapGenerator.Configuration.Exceptions;
using Core.Registries;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;

namespace BacapGenerator.Datapacks.Models.Settings.Checklists;

/// <summary>
/// Configuration for a checklist generator, defining trigger callbacks and entity verification routines.
/// </summary>
public sealed class ChecklistDefinitionSettings
{
    /// <summary>
    /// Gets the unique identifier for this checklist.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("id")]
    public required string Id { get; init; }

    /// <summary>
    /// Gets the optional template name to inherit styling and formatting rules from.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("template")]
    public string? Template { get; init; }

    /// <summary>
    /// Gets the target namespace where functions will be generated.
    /// If omitted, defaults to the datapack's RewardNamespace.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("target_namespace")]
    public string? TargetNamespace { get; init; }

    /// <summary>
    /// Gets the storage identifier used for NBT data operations (e.g. "bacaped_mob_universe_storage").
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("storage_name")]
    public required string StorageName { get; init; }

    /// <summary>
    /// Gets the scoreboard objective name used to trigger execution (e.g. "bacaped_mob_universe").
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("trigger_scoreboard")]
    public required string TriggerScoreboard { get; init; }

    /// <summary>
    /// Gets the detection radius in blocks around the player. Defaults to 64.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("distance")]
    public int Distance { get; init; } = 64;

    /// <summary>
    /// Gets the optional extra selector arguments without leading commas (e.g. "predicate=bacaped:is_baby").
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("extra_selector")]
    public string? ExtraSelector { get; init; }

    /// <summary>
    /// Gets the translation key displayed as header in chat.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("header_translate_key")]
    public required string HeaderTranslateKey { get; init; }

    /// <summary>
    /// Gets the filename for the callback function (e.g. "mob_universe_trigger.mcfunction").
    /// Located inside data/{namespace}/function/triggers_callback/.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("trigger_filename")]
    public required string TriggerFilename { get; init; }

    /// <summary>
    /// Gets the optional subfolder for check functions relative to data/{namespace}/function/.
    /// If omitted or empty, files are generated directly in data/{namespace}/function/.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("check_subfolder")]
    public string? CheckSubfolder { get; init; }

    /// <summary>
    /// Gets the visual styling configuration.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("style")]
    public ChecklistStyleSettings Style { get; init; } = new();

    /// <summary>
    /// Gets the list of categories/dimensions configured for this checklist.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("categories")]
    public List<ChecklistCategorySettings> Categories { get; init; } = [];

    /// <summary>
    /// Validates checklist structural properties and passes registry data down to styles and categories.
    /// </summary>
    /// <param name="datapackId">The parent datapack identifier.</param>
    /// <param name="minecraftData">The static Minecraft registries container.</param>
    /// <exception cref="DatapackConfigurationException">Thrown when checklist constraints or registry entries are violated.</exception>
    /// <example>
    /// <code>
    /// checklist.Validate("bacaped", minecraftData);
    /// </code>
    /// </example>
    public void Validate(string datapackId, MinecraftData minecraftData)
    {
        ArgumentNullException.ThrowIfNull(minecraftData);

        if (string.IsNullOrWhiteSpace(Id))
            throw new DatapackConfigurationException(
                datapackId,
                DatapackErrorKind.InvalidChecklistConfiguration,
                $"Checklist definition in datapack '{datapackId}' is missing an '{nameof(Id)}'.",
                nameof(Id));

        if (string.IsNullOrWhiteSpace(StorageName))
            throw new DatapackConfigurationException(
                datapackId,
                DatapackErrorKind.InvalidChecklistConfiguration,
                $"Checklist '{Id}' in datapack '{datapackId}' must specify a '{nameof(StorageName)}'.",
                nameof(StorageName));

        if (string.IsNullOrWhiteSpace(TriggerScoreboard))
            throw new DatapackConfigurationException(
                datapackId,
                DatapackErrorKind.InvalidChecklistConfiguration,
                $"Checklist '{Id}' in datapack '{datapackId}' must specify a '{nameof(TriggerScoreboard)}'.",
                nameof(TriggerScoreboard));

        if (Distance <= 0)
            throw new DatapackConfigurationException(
                datapackId,
                DatapackErrorKind.InvalidChecklistConfiguration,
                $"Checklist '{Id}' in datapack '{datapackId}' has an invalid '{nameof(Distance)}' ({Distance}). It must be greater than 0.",
                nameof(Distance));

        if (string.IsNullOrWhiteSpace(HeaderTranslateKey))
            throw new DatapackConfigurationException(
                datapackId,
                DatapackErrorKind.InvalidChecklistConfiguration,
                $"Checklist '{Id}' in datapack '{datapackId}' must specify a '{nameof(HeaderTranslateKey)}'.",
                nameof(HeaderTranslateKey));

        if (string.IsNullOrWhiteSpace(TriggerFilename))
            throw new DatapackConfigurationException(
                datapackId,
                DatapackErrorKind.InvalidChecklistConfiguration,
                $"Checklist '{Id}' in datapack '{datapackId}' must specify a '{nameof(TriggerFilename)}'.",
                nameof(TriggerFilename));

        if (!TriggerFilename.EndsWith(".mcfunction", StringComparison.OrdinalIgnoreCase))
            throw new DatapackConfigurationException(
                datapackId,
                DatapackErrorKind.InvalidChecklistConfiguration,
                $"Trigger filename '{TriggerFilename}' in checklist '{Id}' must have a '.mcfunction' extension.",
                nameof(TriggerFilename));

        if (TriggerFilename.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            throw new DatapackConfigurationException(
                datapackId,
                DatapackErrorKind.InvalidChecklistConfiguration,
                $"Trigger filename '{TriggerFilename}' in checklist '{Id}' contains invalid filesystem characters.",
                nameof(TriggerFilename));

        if (!string.IsNullOrWhiteSpace(CheckSubfolder) && CheckSubfolder.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            throw new DatapackConfigurationException(
                datapackId,
                DatapackErrorKind.InvalidChecklistConfiguration,
                $"Subfolder path '{CheckSubfolder}' in checklist '{Id}' contains invalid filesystem characters.",
                nameof(CheckSubfolder));

        Style.Validate(datapackId, Id, minecraftData);

        if (Categories.Count == 0)
            throw new DatapackConfigurationException(
                datapackId,
                DatapackErrorKind.InvalidChecklistConfiguration,
                $"Checklist '{Id}' in datapack '{datapackId}' has no categories defined.",
                nameof(Categories));

        var seenCategoryNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var category in Categories)
        {
            category.Validate(datapackId, Id, minecraftData);

            if (!seenCategoryNames.Add(category.Name))
                throw new DatapackConfigurationException(
                    datapackId,
                    DatapackErrorKind.InvalidChecklistConfiguration,
                    $"Duplicate category name '{category.Name}' detected in checklist '{Id}' of datapack '{datapackId}'.",
                    category.Name);
        }
    }
}