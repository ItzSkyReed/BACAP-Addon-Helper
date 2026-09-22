using BacapGenerator.Advancements.Models;

namespace BacapGenerator.Datapacks.Models.Settings;

/// <summary>
/// Provides extension methods for <see cref="DatapackSettings"/> to evaluate reward capabilities,
/// requirement rules per advancement, and physical file existence.
/// </summary>
public static class DatapackSettingsExtensions
{
    /// <param name="settings">The datapack configuration settings.</param>
    extension(DatapackSettings settings)
    {
        /// <summary>
        /// Determines whether validation should actually run for this datapack,
        /// taking into account its <see cref="DatapackType"/> and explicit toggle.
        /// </summary>
        /// <returns><see langword="true"/> if validation is active; otherwise, <see langword="false"/>.</returns>
        public bool IsValidationActive()
        {
            // Reference datapacks should skip validation by default unless explicitly enabled
            return settings is not { DatapackType: DatapackType.Reference, Validation.Enabled: false } && settings.Validation.Enabled;
        }

        /// <summary>
        /// Determines whether missing reward functions on disk should be flagged as errors,
        /// respecting whether the pack actually supports rewards.
        /// </summary>
        /// <returns><see langword="true"/> if missing rewards should be verified.</returns>
        public bool ShouldValidateRewardFiles() =>
            settings.IsValidationActive()
            && settings.Validation.RewardPaths.Enabled
            && settings.SupportsAnyReward();

        private bool SupportsAnyReward() =>
            settings.SupportsExpRewards() || settings.SupportsItemRewards() || settings.SupportsTrophyRewards();

        /// <summary>
        /// Determines whether experience rewards are supported and enabled for this datapack.
        /// </summary>
        /// <returns><see langword="true"/> if experience rewards are active; otherwise, <see langword="false"/>.</returns>
        public bool SupportsExpRewards() =>
            settings.DatapackType == DatapackType.Addon
            || settings is { DatapackType: DatapackType.CompatibilityAddon, CompatibilityAddonSettings.OverrideExpRewards: true };

        /// <summary>
        /// Determines whether item loot rewards are supported and enabled for this datapack.
        /// </summary>
        /// <returns><see langword="true"/> if item loot rewards are active; otherwise, <see langword="false"/>.</returns>
        public bool SupportsItemRewards() =>
            settings.DatapackType == DatapackType.Addon
            || settings is { DatapackType: DatapackType.CompatibilityAddon, CompatibilityAddonSettings.OverrideItemRewards: true };

        /// <summary>
        /// Determines whether trophy rewards are supported and enabled for this datapack.
        /// </summary>
        /// <returns><see langword="true"/> if trophy rewards are active; otherwise, <see langword="false"/>.</returns>
        public bool SupportsTrophyRewards() =>
            settings.DatapackType == DatapackType.Addon
            || settings is { DatapackType: DatapackType.CompatibilityAddon, CompatibilityAddonSettings.OverrideTrophyRewards: true };

        /// <summary>
        /// Determines whether the datapack permits reward modifications on disk.
        /// </summary>
        /// <returns><see langword="true"/> if the datapack is an addon with active reward operations; otherwise, <see langword="false"/>.</returns>
        public bool IsRewardModifiableAddon() =>
            settings.DatapackType == DatapackType.Addon
            || settings is { DatapackType: DatapackType.CompatibilityAddon, CompatibilityAddonSettings.HasAnyRewardOverride: true };

        /// <summary>
        /// Evaluates whether a specific advancement is strictly required to have an experience reward function file in this datapack.
        /// Overridden advancements do not require a local file unless explicit override settings demand it.
        /// </summary>
        /// <param name="advancement">The target BACAP advancement.</param>
        /// <returns><see langword="true"/> if the experience file is mandatory; otherwise, <see langword="false"/>.</returns>
        public bool RequiresExpReward(BacapAdvancement advancement)
        {
            if (advancement.Tier == BacapAdvancementTier.Root || !settings.SupportsExpRewards())
                return false;

            return !advancement.IsOverride || (settings.CompatibilityAddonSettings?.OverrideExpRewards ?? false);
        }

        /// <summary>
        /// Evaluates whether a specific advancement is strictly required to have an item reward function file in this datapack.
        /// </summary>
        /// <param name="advancement">The target BACAP advancement.</param>
        /// <returns><see langword="true"/> if the item reward file is mandatory; otherwise, <see langword="false"/>.</returns>
        public bool RequiresItemReward(BacapAdvancement advancement)
        {
            if (advancement.Tier == BacapAdvancementTier.Root || !settings.SupportsItemRewards())
                return false;

            return !advancement.IsOverride || (settings.CompatibilityAddonSettings?.OverrideItemRewards ?? false);
        }

        /// <summary>
        /// Evaluates whether a specific advancement is strictly required to have a trophy reward function file in this datapack.
        /// </summary>
        /// <param name="advancement">The target BACAP advancement.</param>
        /// <returns><see langword="true"/> if the trophy file is mandatory; otherwise, <see langword="false"/>.</returns>
        public bool RequiresTrophyReward(BacapAdvancement advancement)
        {
            if (advancement.Tier == BacapAdvancementTier.Root || !settings.SupportsTrophyRewards())
                return false;

            return !advancement.IsOverride || (settings.CompatibilityAddonSettings?.OverrideTrophyRewards ?? false);
        }

        /// <summary>
        /// Checks whether an advancement has at least one active reward function existing physically on disk in this datapack.
        /// </summary>
        /// <param name="advancement">The BACAP advancement to inspect.</param>
        /// <returns><see langword="true"/> if at least one enabled reward file exists; otherwise, <see langword="false"/>.</returns>
        public bool HasAnyExistingReward(BacapAdvancement advancement)
        {
            advancement.ExpRewardFunction.File.Refresh();
            advancement.ItemRewardFunction.File.Refresh();
            advancement.TrophyRewardFunction.File.Refresh();

            return (settings.SupportsExpRewards() && advancement.ExpRewardFunction.File.Exists)
                   || (settings.SupportsItemRewards() && advancement.ItemRewardFunction.File.Exists)
                   || (settings.SupportsTrophyRewards() && advancement.TrophyRewardFunction.File.Exists);
        }

        /// <summary>
        /// Checks whether an advancement is missing any mandatory reward function on disk according to its tier and override status.
        /// </summary>
        /// <param name="advancement">The BACAP advancement to inspect.</param>
        /// <returns><see langword="true"/> if a mandatory reward file is missing; otherwise, <see langword="false"/>.</returns>
        public bool HasAnyMissingReward(BacapAdvancement advancement)
        {
            advancement.ExpRewardFunction.File.Refresh();
            advancement.ItemRewardFunction.File.Refresh();
            advancement.TrophyRewardFunction.File.Refresh();

            return (settings.RequiresExpReward(advancement) && !advancement.ExpRewardFunction.File.Exists)
                   || (settings.RequiresItemReward(advancement) && !advancement.ItemRewardFunction.File.Exists)
                   || (settings.RequiresTrophyReward(advancement) && !advancement.TrophyRewardFunction.File.Exists);
        }
    }
}