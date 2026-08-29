using BacapGenerator.Models.Advancements;
using Core.Advancements.Models;

namespace BacapGenerator.Services;

public static class BacapTierResolver
{
    /// <summary>
    /// Attempts to resolve the advancement tier based on its file and display properties without throwing exceptions.
    /// </summary>
    /// <param name="filename">The base name of the advancement file without extension.</param>
    /// <param name="tab">The resolved BACAP tab model.</param>
    /// <param name="hidden">Indicates whether the advancement is marked as hidden.</param>
    /// <param name="frame">The optional advancement display frame type.</param>
    /// <param name="descriptionColor">The optional title color identifier.</param>
    /// <param name="advancementTier">When this method returns, contains the resolved <see cref="BacapAdvancementTier"/> if successful; otherwise, the default value.</param>
    /// <returns><see langword="true"/> if the tier was successfully resolved; otherwise, <see langword="false"/>.</returns>
    /// <example>
    /// <code>
    /// if (BacapTierResolver.TryResolve("root", BacapAdvancementTab.Adventure, false, null, null, out var tier))
    /// {
    ///     Console.WriteLine(tier); // BacapTier.Root
    /// }
    /// </code>
    /// </example>
    public static bool TryResolve(
        string filename,
        BacapAdvancementTab tab,
        bool hidden,
        AdvancementFrame? frame,
        string? descriptionColor,
        out BacapAdvancementTier advancementTier)
    {
        if (hidden)
        {
            advancementTier = BacapAdvancementTier.Hidden;
            return true;
        }

        var actualFrame = frame ?? AdvancementFrame.Task;

        switch (filename, tab, actualFrame, color: descriptionColor)
        {
            // Root advancement
            case ("root", _, _, _):
                advancementTier = BacapAdvancementTier.Root;
                return true;

            // Bacap tab specific tiers
            case (_, var t, _, "gold") when t == BacapAdvancementTab.Bacap:
                advancementTier = BacapAdvancementTier.AdvancementLegend;
                return true;

            case (_, var t, _, "yellow") when t == BacapAdvancementTab.Bacap:
                advancementTier = BacapAdvancementTier.Milestone;
                return true;

            // Challenges tab
            case (_, var t, _, "yellow") when t == BacapAdvancementTab.Challenges:
                advancementTier = BacapAdvancementTier.SuperChallenge;
                return true;

            // Standard frame mappings
            case (_, var t, AdvancementFrame.Challenge, _):
                advancementTier = BacapAdvancementTier.Challenge;
                return true;

            case (_, _, AdvancementFrame.Goal, _):
                advancementTier = BacapAdvancementTier.Goal;
                return true;

            case (_, _, AdvancementFrame.Task, _):
                advancementTier = BacapAdvancementTier.Task;
                return true;

            default:
                advancementTier = default;
                return false;
        }
    }
}