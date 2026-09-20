using BacapGenerator.Advancements.Models;

namespace BacapGenerator.Advancements.Filtering;

/// <summary>
/// Provides extension methods to filter advancements based on specific rules.
/// </summary>
public static class AdvancementFilters
{
    extension(IEnumerable<BacapAdvancement> advancements)
    {
        /// <summary>
        /// Gets advancements eligible for the update_score function (excludes hidden advancements).
        /// </summary>
        public IEnumerable<BacapAdvancement> GetForUpdateScore()
        {
            return advancements.Where(a => a.Tier != BacapAdvancementTier.Hidden && a.Tier != BacapAdvancementTier.Root);
        }

        /// <summary>
        /// Gets advancements that grant trophy rewards.
        /// </summary>
        public IEnumerable<BacapAdvancement> GetWithTrophies()
        {
            return advancements.Where(a => a.TrophyRewardFunction.Trophies.Count > 0);
        }

        /// <summary>
        /// Gets valid advancements excluding milestones, root, and legend.
        /// </summary>
        public IEnumerable<BacapAdvancement> GetValidPlayable(string? legendPath)
        {
            return advancements.Where(a =>
                a.Tier is not (BacapAdvancementTier.Root or BacapAdvancementTier.Hidden) &&
                a.McPath != legendPath);
        }
    }
}