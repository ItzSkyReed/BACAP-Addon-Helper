
using BacapGenerator.Models.Advancements;

namespace BacapGenerator.Models.Datapacks.Settings;

/// <summary>
/// Provides extension methods for <see cref="DatapackSettings"/> to evaluate reward capabilities and file requirements.
/// </summary>
public static class DatapackSettingsExtensions
{
    /// <param name="settings">The datapack configuration settings.</param>
    extension(DatapackSettings settings)
    {
        /// <summary>
        /// Determines whether experience rewards are active for this datapack.
        /// </summary>
        /// <returns><see langword="true"/> if experience rewards are supported and enabled; otherwise, <see langword="false"/>.</returns>
        public bool SupportsExpRewards() =>
            settings.Type == DatapackType.Addon
            || settings is { Type: DatapackType.CompatibilityAddon, CompatibilityAddonSettings.OverrideExpRewards: true };

        /// <summary>
        /// Determines whether item loot rewards are active for this datapack.
        /// </summary>
        /// <returns><see langword="true"/> if item loot rewards are supported and enabled; otherwise, <see langword="false"/>.</returns>
        public bool SupportsItemRewards() =>
            settings.Type == DatapackType.Addon
            || settings is { Type: DatapackType.CompatibilityAddon, CompatibilityAddonSettings.OverrideItemRewards: true };

        /// <summary>
        /// Determines whether trophy rewards are active for this datapack.
        /// </summary>
        /// <returns><see langword="true"/> if trophy rewards are supported and enabled; otherwise, <see langword="false"/>.</returns>
        public bool SupportsTrophyRewards() =>
            settings.Type == DatapackType.Addon
            || settings is { Type: DatapackType.CompatibilityAddon, CompatibilityAddonSettings.OverrideTrophyRewards: true };

        /// <summary>
        /// Determines whether the datapack permits reward modifications on disk.
        /// </summary>
        /// <returns><see langword="true"/> if the datapack is an addon with active reward operations; otherwise, <see langword="false"/>.</returns>
        public bool IsRewardModifiableAddon() =>
            settings.Type == DatapackType.Addon
            || settings is { Type: DatapackType.CompatibilityAddon, CompatibilityAddonSettings.HasAnyRewardOverride: true };

        /// <summary>
        /// Checks whether an advancement has at least one active reward function existing on disk.
        /// </summary>
        /// <param name="advancement">The BACAP advancement to inspect.</param>
        /// <returns><see langword="true"/> if at least one enabled reward file exists; otherwise, <see langword="false"/>.</returns>
        public bool HasAnyExistingReward(BacapAdvancement advancement) =>
            (settings.SupportsExpRewards() && advancement.ExpRewardFunction.File.Exists)
            || (settings.SupportsItemRewards() && advancement.ItemRewardFunction.File.Exists)
            || (settings.SupportsTrophyRewards() && advancement.TrophyRewardFunction.File.Exists);

        /// <summary>
        /// Checks whether an advancement is missing at least one active reward function on disk.
        /// </summary>
        /// <param name="advancement">The BACAP advancement to inspect.</param>
        /// <returns><see langword="true"/> if at least one enabled reward file is missing; otherwise, <see langword="false"/>.</returns>
        public bool HasAnyMissingReward(BacapAdvancement advancement) =>
            advancement.Tier != BacapAdvancementTier.Root
            && ((settings.SupportsExpRewards() && !advancement.ExpRewardFunction.File.Exists)
                || (settings.SupportsItemRewards() && !advancement.ItemRewardFunction.File.Exists)
                || (settings.SupportsTrophyRewards() && !advancement.TrophyRewardFunction.File.Exists));
    }
}