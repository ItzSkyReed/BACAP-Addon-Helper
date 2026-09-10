using BacapGenerator.Models.Advancements;
using Core.Advancements.Models;

namespace BacapGenerator.Services;

/// <summary>
/// Provides diagnostic methods to resolve BACAP advancement tiers from raw Minecraft advancement data.
/// </summary>
public static class BacapTierResolver
{
    /// <summary>
    /// Attempts to resolve the advancement tier based on its file name, tab, and display properties.
    /// </summary>
    /// <param name="filename">The base name of the advancement file without extension.</param>
    /// <param name="tab">The resolved BACAP tab model.</param>
    /// <param name="hidden">Indicates whether the advancement is marked as hidden.</param>
    /// <param name="frame">The optional advancement display frame type.</param>
    /// <param name="descriptionColor">The optional description color identifier.</param>
    /// <param name="tier">When this method returns, contains the resolved <see cref="BacapAdvancementTier"/> if successful; otherwise, the default value.</param>
    /// <returns><see langword="true"/> if the tier was successfully resolved; otherwise, <see langword="false"/>.</returns>
    /// <example>
    /// <code>
    /// if (BacapTierResolver.TryResolve("root", BacapAdvancementTab.Adventure, false, null, "#CCCCCC", out var tier))
    /// {
    ///     Console.WriteLine(tier); // BacapAdvancementTier.Root
    /// }
    /// </code>
    /// </example>
    public static bool TryResolve(
        string filename,
        BacapAdvancementTab tab,
        bool hidden,
        AdvancementFrame? frame,
        string? descriptionColor,
        out BacapAdvancementTier tier)
    {
        var actualFrame = frame ?? AdvancementFrame.Task;

        var resolved = (hidden, filename, tab, actualFrame, descriptionColor) switch
        {
            (true, _, _, _, _) => BacapAdvancementTier.Hidden,

            (_, "root", _, _, "#CCCCCC") => BacapAdvancementTier.Root,

            (_, _, var t, _, "gold") when t == BacapAdvancementTab.Bacap => BacapAdvancementTier.AdvancementLegend,

            (_, _, var t, _, "yellow") when t == BacapAdvancementTab.Bacap => BacapAdvancementTier.Milestone,

            var (_, _, t, _, _) when t == BacapAdvancementTab.Challenges => BacapAdvancementTier.SuperChallenge,

            (_, _, _, AdvancementFrame.Challenge, _) => BacapAdvancementTier.Challenge,

            (_, _, _, AdvancementFrame.Goal, _) => BacapAdvancementTier.Goal,

            (_, _, _, AdvancementFrame.Task, _) => BacapAdvancementTier.Task,

            _ => (BacapAdvancementTier?)null
        };

        if (resolved.HasValue)
        {
            tier = resolved.Value;
            return true;
        }

        tier = default;
        return false;
    }
}