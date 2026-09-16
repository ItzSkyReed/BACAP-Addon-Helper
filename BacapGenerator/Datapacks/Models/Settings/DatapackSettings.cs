using BacapGenerator.Common;
using BacapGenerator.Datapacks.Models.Settings.Validation;
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
    /// Gets the optional filesystem path to the language resource pack folder.
    /// </summary>
    [ConfigurationKeyName("language_pack_path")]
    public string? LanguagePackPath { get; init; }

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
    /// Gets the access permission mode determining whether file modifications are permitted.
    /// </summary>
    [ConfigurationKeyName("type")]
    public DatapackType Type { get; init; } = DatapackType.Reference;

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
    public CompatibilityAddonSettings? CompatibilityAddonSettings { get; init; }

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
    /// Validates the current settings object based on the configured access mode and paths.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if validation constraints are violated.</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Path))
            throw new InvalidOperationException($"'{nameof(Path)}' must be provided for the datapack.");

        if (string.IsNullOrWhiteSpace(MainNamespace))
            throw new InvalidOperationException($"'{nameof(MainNamespace)}' must be provided.");

        if (string.IsNullOrWhiteSpace(RewardNamespace))
            throw new InvalidOperationException($"'{nameof(RewardNamespace)}' must be provided.");

        if (Type != DatapackType.Addon)
            return;

        if (string.IsNullOrWhiteSpace(LanguagePackPath))
            throw new InvalidOperationException($"'{nameof(LanguagePackPath)}' must be provided when Type is set to Addon.");

        if (Type == DatapackType.CompatibilityAddon && string.IsNullOrWhiteSpace(ParentDatapackId))
        {
            throw new InvalidOperationException($"'{nameof(ParentDatapackId)}' must be provided when Type is set to CompatibilityAddon.");
        }
    }
}