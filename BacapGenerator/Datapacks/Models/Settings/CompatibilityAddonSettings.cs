using Microsoft.Extensions.Configuration;

namespace BacapGenerator.Datapacks.Models.Settings;

/// <summary>
/// Configures override behavior for rewards and announcements in compatibility addon datapacks.
/// </summary>
public sealed class CompatibilityAddonSettings
{
    /// <summary>
    /// Gets or sets a value indicating whether advancement completion messages should be overridden.
    /// Defaults to <see langword="true"/>.
    /// </summary>
    [ConfigurationKeyName("override_msg")]
    public bool OverrideMsg { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether experience rewards should be overridden.
    /// Defaults to <see langword="false"/>.
    /// </summary>
    [ConfigurationKeyName("override_exp_rewards")]
    public bool OverrideExpRewards { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether item loot rewards should be overridden.
    /// Defaults to <see langword="false"/>.
    /// </summary>
    [ConfigurationKeyName("override_item_rewards")]
    public bool OverrideItemRewards { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether trophy rewards should be overridden.
    /// </summary>
    [ConfigurationKeyName("override_trophy_rewards")]
    public bool OverrideTrophyRewards { get; set; } = false;

    /// <summary>
    /// Gets a value indicating whether at least one override option is enabled.
    /// </summary>
    public bool HasAnyOverride => OverrideMsg || OverrideExpRewards || OverrideItemRewards || OverrideTrophyRewards;

    /// <summary>
    /// Gets a value indicating whether at least one reward (exp, items, or trophies) is overridden.
    /// </summary>
    public bool HasAnyRewardOverride => OverrideExpRewards || OverrideItemRewards || OverrideTrophyRewards;
}