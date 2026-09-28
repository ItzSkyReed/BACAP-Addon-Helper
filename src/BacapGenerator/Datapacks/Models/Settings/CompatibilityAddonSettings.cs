using BacapGenerator.Configuration;
using BacapGenerator.Configuration.Exceptions;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;

namespace BacapGenerator.Datapacks.Models.Settings;

/// <summary>
/// Configures override behavior for rewards and announcements in compatibility addon datapacks.
/// </summary>
public sealed class CompatibilityAddonSettings
{
    /// <summary>
    /// Gets the raw string representation indicating whether advancement completion messages should be overridden.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("override_msg")]
    public string? RawOverrideMsg { get; init; }

    /// <summary>
    /// Gets the raw string representation indicating whether experience rewards should be overridden.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("override_exp_rewards")]
    public string? RawOverrideExpRewards { get; init; }

    /// <summary>
    /// Gets the raw string representation indicating whether item loot rewards should be overridden.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("override_item_rewards")]
    public string? RawOverrideItemRewards { get; init; }

    /// <summary>
    /// Gets the raw string representation indicating whether trophy rewards should be overridden.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("override_trophy_rewards")]
    public string? RawOverrideTrophyRewards { get; init; }

    /// <summary>
    /// Gets a value indicating whether advancement completion messages should be overridden.
    /// Defaults to <see langword="true"/>.
    /// </summary>
    [PublicAPI]
    public bool IsOverridedMsg { get; private set; } = true;

    /// <summary>
    /// Gets a value indicating whether experience rewards should be overridden.
    /// Defaults to <see langword="false"/>.
    /// </summary>
    [PublicAPI]
    public bool IsOverridedExpRewards { get; private set; }

    /// <summary>
    /// Gets a value indicating whether item loot rewards should be overridden.
    /// Defaults to <see langword="false"/>.
    /// </summary>
    [PublicAPI]
    public bool IsOverridedItemRewards { get; private set; }

    /// <summary>
    /// Gets a value indicating whether trophy rewards should be overridden.
    /// Defaults to <see langword="false"/>.
    /// </summary>
    [PublicAPI]
    public bool IsOverridedTrophyRewards { get; private set; }

    /// <summary>
    /// Gets a value indicating whether at least one override option is enabled.
    /// </summary>
    [PublicAPI]
    public bool HasAnyOverride => IsOverridedMsg || IsOverridedExpRewards || IsOverridedItemRewards || IsOverridedTrophyRewards;

    /// <summary>
    /// Gets a value indicating whether at least one reward (exp, items, or trophies) is overridden.
    /// </summary>
    [PublicAPI]
    public bool HasAnyRewardOverride => IsOverridedExpRewards || IsOverridedItemRewards || IsOverridedTrophyRewards;

    /// <summary>
    /// Validates and parses the raw string override values into their boolean representations.
    /// </summary>
    /// <param name="datapackId">The owning datapack identifier for error reporting.</param>
    /// <exception cref="DatapackConfigurationException">
    /// Thrown when any override setting string cannot be parsed as a valid boolean value.
    /// </exception>
    /// <example>
    /// <code>
    /// var settings = new CompatibilityAddonSettings();
    /// settings.Validate("bacaped_compat");
    /// </code>
    /// </example>
    public void Validate(string datapackId)
    {
        IsOverridedMsg = ConfigParser.ParseBool(
            RawOverrideMsg,
            defaultValue: true,
            datapackId,
            DatapackErrorKind.InvalidCompatibilityAddonSettings,
            $"Invalid boolean value '{RawOverrideMsg}' for 'override_msg' in datapack '{datapackId}'.");

        IsOverridedExpRewards = ConfigParser.ParseBool(
            RawOverrideExpRewards,
            defaultValue: false,
            datapackId,
            DatapackErrorKind.InvalidCompatibilityAddonSettings,
            $"Invalid boolean value '{RawOverrideExpRewards}' for 'override_exp_rewards' in datapack '{datapackId}'.");

        IsOverridedItemRewards = ConfigParser.ParseBool(
            RawOverrideItemRewards,
            defaultValue: false,
            datapackId,
            DatapackErrorKind.InvalidCompatibilityAddonSettings,
            $"Invalid boolean value '{RawOverrideItemRewards}' for 'override_item_rewards' in datapack '{datapackId}'.");

        IsOverridedTrophyRewards = ConfigParser.ParseBool(
            RawOverrideTrophyRewards,
            defaultValue: false,
            datapackId,
            DatapackErrorKind.InvalidCompatibilityAddonSettings,
            $"Invalid boolean value '{RawOverrideTrophyRewards}' for 'override_trophy_rewards' in datapack '{datapackId}'.");
    }
}