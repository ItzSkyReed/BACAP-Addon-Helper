using BacapGenerator.Common;
using BacapGenerator.Configuration;
using BacapGenerator.Configuration.Exceptions;
using BacapGenerator.Datapacks.Models.Settings.Checklists;
using BacapGenerator.Datapacks.Models.Settings.Scoreboards;
using BacapGenerator.Datapacks.Models.Settings.Validation;
using Core.Registries;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;

namespace BacapGenerator.Datapacks.Models.Settings;

/// <summary>
/// Represents the configuration, filesystem paths, and domain metadata for a datapack.
/// Supports direct binding from configuration providers.
/// </summary>
public sealed class DatapackSettings
{
    /// <summary>
    /// Gets the filesystem path to the root folder of the datapack.
    /// </summary>
    [ConfigurationKeyName("path")]
    public string Path { get; init; } = string.Empty;

    /// <summary>
    /// Gets the optional language pack settings.
    /// </summary>
    [ConfigurationKeyName("language_pack")]
    public LanguagePackSettings? LanguagePackSettings { get; init; }

    /// <summary>
    /// Gets the primary namespace containing the advancements.
    /// </summary>
    [ConfigurationKeyName("main_namespace")]
    public string MainNamespace { get; init; } = string.Empty;

    /// <summary>
    /// Gets the namespace used for custom rewards, functions, and internal macros.
    /// </summary>
    [ConfigurationKeyName("reward_namespace")]
    public string RewardNamespace { get; init; } = string.Empty;

    /// <summary>
    /// Gets the raw string representation of the datapack type as defined in config.yaml.
    /// </summary>
    [ConfigurationKeyName("type")]
    public string? RawType { get; init; }

    /// <summary>
    /// Gets the parsed and verified datapack operational type.
    /// </summary>
    public DatapackType DatapackType { get; private set; }

    [PublicAPI]
    [ConfigurationKeyName("checklists")]
    public List<ChecklistDefinitionSettings> Checklists { get; init; } = [];

    /// <summary>
    /// Gets the precalculated macro command identifier.
    /// </summary>
    public string MacroCommandName => field ??= $"{RewardNamespace}:advancement_made_macro";

    /// <summary>
    /// Gets the optional identifier of the parent datapack when acting as an override addon.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("parent_datapack_id")]
    public string? ParentDatapackId { get; init; }

    /// <summary>
    /// Gets the optional identifier of the parent datapack when acting as an override addon.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("checklist_triggers_folder")]
    public string ChecklistTriggersFolder { get; init; } = string.Empty;

    [PublicAPI]
    [ConfigurationKeyName("custom_scores")]
    public List<ScoreboardScoreUpdateSettings> CustomScores { get; init; } = [];

    [PublicAPI]
    [ConfigurationKeyName("custom_points")]
    public List<ScoreboardPointUpdateSettings> CustomPoints { get; init; } = [];

    /// <summary>
    /// Gets the mapping of advancement tabs to their milestone advancement Minecraft paths.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("milestone_mc_paths")]
    public Dictionary<BacapTab, string>? MilestoneMcPaths { get; init; }

    /// <summary>
    /// Gets the Minecraft resource path for the advancement legend reward.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("advancement_legend_mc_path")]
    public string? AdvancementLegendMcPath { get; init; }

    [PublicAPI]
    [ConfigurationKeyName("compatibility_addon_settings")]
    public CompatibilityAddonSettings CompatibilityAddonSettings { get; init; } = new();

    /// <summary>
    /// Gets the validation rules and thresholds configured for this specific datapack.
    /// If omitted in configuration, default settings are used.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("validation")]
    public DatapackValidationSettings ValidationSettings { get; init; } = new();

    /// <summary>
    /// Gets the validation rules and thresholds configured for this specific datapack.
    /// If omitted in configuration, default settings are used.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("document_generator")]
    public DocumentGeneratorSettings? DocumentGeneratorSettings { get; init; }

    [PublicAPI]
    [ConfigurationKeyName("fanpacks_namespace")]
    public string? FanpacksNamespace { get; init; }

    /// <summary>
    /// Gets the optional name of the release.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("release_name")]
    public string? ReleaseName { get; init; }

    /// <summary>
    /// Validates the current datapack settings and verifies its integrity.
    /// </summary>
    /// <param name="datapackId">The key identifier of this datapack in the configuration dictionary.</param>
    /// <param name="minecraftData">The registry containing validated Minecraft entity and game data.</param>
    /// <exception cref="DatapackConfigurationException">Thrown when a setting rule is violated.</exception>
    /// <example>
    /// <code>
    /// datapackSettings.Validate("bacaped", minecraftData);
    /// </code>
    /// </example>
    public void Validate(string datapackId, MinecraftData minecraftData)
    {
        if (string.IsNullOrWhiteSpace(Path))
        {
            throw new DatapackConfigurationException(
                datapackId,
                DatapackErrorKind.MissingPath,
                $"Path is not defined for datapack '{datapackId}'.",
                nameof(Path));
        }

        DatapackType = ConfigParser.ParseRequiredEnum<DatapackType>(
            RawType,
            datapackId,
            DatapackErrorKind.InvalidDatapackType,
            missingErrorMessage: $"Missing 'type' property for datapack '{datapackId}'. Expected: reference, addon, compatibility_addon.",
            invalidErrorMessage: $"Invalid datapack type '{RawType}' in '{datapackId}'. Allowed values: reference, addon, compatibility_addon.",
            propertyName: "type");

        if (string.IsNullOrWhiteSpace(MainNamespace))
        {
            throw new DatapackConfigurationException(
                datapackId,
                DatapackErrorKind.MissingNamespace,
                $"Main namespace is missing for datapack '{datapackId}'.",
                "main_namespace");
        }

        if (string.IsNullOrWhiteSpace(RewardNamespace))
        {
            throw new DatapackConfigurationException(
                datapackId,
                DatapackErrorKind.MissingNamespace,
                $"Reward namespace is missing for datapack '{datapackId}'.",
                "reward_namespace");
        }

        if (DatapackType == DatapackType.CompatibilityAddon && string.IsNullOrWhiteSpace(ParentDatapackId))
        {
            throw new DatapackConfigurationException(
                datapackId,
                DatapackErrorKind.MissingParentDatapack,
                $"Compatibility addon '{datapackId}' must specify 'parent_datapack_id'.",
                "parent_datapack_id");
        }

        if (Checklists.Count > 0 && string.IsNullOrWhiteSpace(ChecklistTriggersFolder))
        {
            throw new DatapackConfigurationException(
                datapackId,
                DatapackErrorKind.MissingPath,
                $"Checklist triggers folder must be defined for datapack '{datapackId}' because checklists are present.",
                nameof(ChecklistTriggersFolder));
        }

        LanguagePackSettings?.Validate(datapackId);
        ValidationSettings.Validate(datapackId);

        foreach (var checklist in Checklists)
        {
            checklist.Validate(datapackId, minecraftData);
        }

        var seenScoreIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var score in CustomScores)
        {
            score.Validate(datapackId);
            if (!seenScoreIds.Add(score.Id))
            {
                throw new DatapackConfigurationException(
                    datapackId,
                    DatapackErrorKind.InvalidScoreConfiguration,
                    $"Duplicate score counter ID '{score.Id}' detected in datapack '{datapackId}'.",
                    score.Id);
            }
        }

        var seenPointIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var point in CustomPoints)
        {
            point.Validate(datapackId);
            if (!seenPointIds.Add(point.Id))
            {
                throw new DatapackConfigurationException(
                    datapackId,
                    DatapackErrorKind.InvalidScoreConfiguration,
                    $"Duplicate point counter ID '{point.Id}' detected in datapack '{datapackId}'.",
                    point.Id);
            }
        }

        CompatibilityAddonSettings.Validate(datapackId);
        DocumentGeneratorSettings?.Validate(datapackId);
    }
}