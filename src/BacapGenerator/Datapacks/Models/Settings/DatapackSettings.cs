using BacapGenerator.Common;
using BacapGenerator.Configuration.Exceptions;
using BacapGenerator.Datapacks.Models.Settings.Checklists;
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
    public LanguagePackSettings? LanguagePackSettigs { get; init; }

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
    /// Gets the mapping of advancement tabs to their milestone advancement Minecraft paths.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("milestone_mc_paths")]
    public Dictionary<BacapTab, string>? MilestoneMcPaths { get; init; }

    /// <summary>
    /// Gets  the Minecraft resource path for the advancement legend reward.
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
    public DatapackValidationSettings Validation { get; init; } = new();

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
    /// <param name="minecraftData">Minecraft data</param>
    /// <exception cref="DatapackConfigurationException">Thrown when a setting rule is violated.</exception>
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

        // Validate and parse type explicitly so bad YAML values don't get silently dropped
        if (string.IsNullOrWhiteSpace(RawType))
        {
            throw new DatapackConfigurationException(
                datapackId,
                DatapackErrorKind.InvalidDatapackType,
                $"Missing 'type' property for datapack '{datapackId}'. Expected: reference, addon, compatibility_addon.",
                nameof(DatapackType));
        }

        var normalizedType = RawType.Trim().Replace("_", string.Empty);
        if (Enum.TryParse<DatapackType>(normalizedType, ignoreCase: true, out var parsedType) && Enum.IsDefined(parsedType))
        {
            DatapackType = parsedType;
        }
        else
        {
            throw new DatapackConfigurationException(
                datapackId,
                DatapackErrorKind.InvalidDatapackType,
                $"Invalid datapack type '{RawType}' in '{datapackId}'. Allowed values: reference, addon, compatibility_addon.",
                RawType);
        }

        if (string.IsNullOrWhiteSpace(MainNamespace))
        {
            throw new DatapackConfigurationException(
                datapackId,
                DatapackErrorKind.MissingNamespace,
                $"Main namespace is missing for datapack '{datapackId}'.",
                nameof(MainNamespace));
        }

        if (string.IsNullOrWhiteSpace(RewardNamespace))
        {
            throw new DatapackConfigurationException(
                datapackId,
                DatapackErrorKind.MissingNamespace,
                $"Reward namespace is missing for datapack '{datapackId}'.",
                nameof(RewardNamespace));
        }

        if (DatapackType == DatapackType.CompatibilityAddon && string.IsNullOrWhiteSpace(ParentDatapackId))
        {
            throw new DatapackConfigurationException(
                datapackId,
                DatapackErrorKind.MissingParentDatapack,
                $"Compatibility addon '{datapackId}' must specify '{nameof(ParentDatapackId)}'.",
                nameof(ParentDatapackId));
        }

        // Validate nested language pack settings if configured
        LanguagePackSettigs?.Validate(datapackId);

        // Validate nested validation settings
        Validation.Validate(datapackId);

        foreach (var checklist in Checklists)
        {
            checklist.Validate(datapackId, minecraftData);
        }
    }
}