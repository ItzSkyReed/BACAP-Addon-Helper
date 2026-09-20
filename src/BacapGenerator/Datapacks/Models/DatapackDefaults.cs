using BacapGenerator.Advancements.Models;
using BacapGenerator.Common;
using Core.Advancements.Models;

namespace BacapGenerator.Datapacks.Models;

/// <summary>
/// Represents the visual settings for a specific advancement tier announcement.
/// </summary>
public record AdvancementMessageSettingsEntry
{
    /// <summary>
    /// Completion message translation key.
    /// </summary>
    /// <example>
    /// %1$s has found the hidden advancement %2$s%3$s%4$s
    /// </example>
    public required string TranslationKey { get; init; }

    /// <summary>
    /// Color of the title of advancement.
    /// </summary>
    public required string TitleColor { get; init; }

    /// <summary>
    /// Color of the description of advancement.
    /// </summary>
    public required string DescriptionColor { get; init; }
}

/// <summary>
/// Represents visual and structural constraints associated with a specific BACAP tier.
/// </summary>
/// <param name="Frame">The Minecraft display frame assigned to this tier.</param>
/// <param name="DescriptionColor">The color code applied to the description, or <see langword="null"/> if unstyled.</param>
/// <param name="IsHidden">Indicates whether the advancement should be visually hidden from the tree.</param>
/// <param name="RequiredTab">The specific tab required by this tier, or <see langword="null"/> if universal.</param>
public readonly record struct BacapTierProfile(
    string DescriptionColor,
    bool IsHidden = false,
    AdvancementFrame? Frame = null,
    BacapTab? RequiredTab = null
);

/// <summary>
/// Provides default tier announcements and domain metadata for datapacks.
/// </summary>
public static class DatapackDefaults
{
    public const string AdvancementsScoreboard = "bac_advancements";
    public const string PointsScoreboard = "bac_advancements_points";
    public const string CoopBaseScoreboard = "bac_obtained";

    // Dividers in timers, stat triggers etc
    public const string DefaultDivider = "                                             ";
    public const string DefaultDividerColor = "dark_gray";


    internal static readonly AdvancementMessageSettingsEntry AdvancementLegendEntry = new()
    {
        TranslationKey = "%1$s has completed every advancement in the %2$s tab, earning them the advancement %3$s%4$s%5$s",
        TitleColor = "gold",
        DescriptionColor = "gold"
    };

    internal static readonly AdvancementMessageSettingsEntry MilestoneEntry = new()
    {
        TranslationKey = "%1$s has completed every advancement in the %2$s tab, earning them the advancement %3$s%4$s%5$s",
        TitleColor = "yellow",
        DescriptionColor = "#E5E74F"
    };

    internal static readonly AdvancementMessageSettingsEntry SuperChallengeEntry = new()
    {
        TranslationKey = "%1$s has completed the super challenge %2$s%3$s%4$s",
        TitleColor = "#FF2A2A",
        DescriptionColor = "#DC2727"
    };

    internal static readonly AdvancementMessageSettingsEntry HiddenEntry = new()
    {
        TranslationKey = "%1$s has found the hidden advancement %2$s%3$s%4$s",
        TitleColor = "light_purple",
        DescriptionColor = "#DE4ADC"
    };

    internal static readonly AdvancementMessageSettingsEntry ChallengeEntry = new()
    {
        TranslationKey = "%1$s has completed the challenge %2$s%3$s%4$s",
        TitleColor = "dark_purple",
        DescriptionColor = "#C900C7"
    };

    internal static readonly AdvancementMessageSettingsEntry GoalEntry = new()
    {
        TranslationKey = "%1$s has reached the goal %2$s%3$s%4$s",
        TitleColor = "#75E1FF",
        DescriptionColor = "#63BDD7"
    };

    internal static readonly AdvancementMessageSettingsEntry TaskEntry = new()
    {
        TranslationKey = "%1$s has made the advancement %2$s%3$s%4$s",
        TitleColor = "green",
        DescriptionColor = "#49DB49"
    };

    /// <summary>
    /// Resolves default message settings for the specified advancement tier.
    /// </summary>
    /// <param name="tier">The advancement tier to resolve.</param>
    /// <returns>The pre-allocated <see cref="AdvancementMessageSettingsEntry"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="tier"/> contains an undefined enum value.</exception>
    /// <example>
    /// <code>
    /// AdvancementMessageSettingsEntry entry = BacapAdvancementTier.Challenge.GetDefaultMessage();
    /// </code>
    /// </example>
    public static AdvancementMessageSettingsEntry GetDefaultMessage(this BacapAdvancementTier tier) => tier switch
    {
        BacapAdvancementTier.AdvancementLegend => AdvancementLegendEntry,
        BacapAdvancementTier.Milestone => MilestoneEntry,
        BacapAdvancementTier.SuperChallenge => SuperChallengeEntry,
        BacapAdvancementTier.Hidden => HiddenEntry,
        BacapAdvancementTier.Challenge => ChallengeEntry,
        BacapAdvancementTier.Goal => GoalEntry,
        BacapAdvancementTier.Task => TaskEntry,
        _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, "Unsupported advancement tier.")
    };


    private static readonly BacapTierProfile AdvancementLegendTierProfile = new()
    {
        Frame = AdvancementFrame.Challenge,
        DescriptionColor = "gold",
        RequiredTab = BacapTab.Bacap

    };

    private static readonly BacapTierProfile MilestoneTierProfile = new()
    {
        Frame = AdvancementFrame.Challenge,
        DescriptionColor = "yellow",
        RequiredTab = BacapTab.Bacap

    };

    private static readonly BacapTierProfile SuperChallengeTierProfile = new()
    {
        Frame = AdvancementFrame.Challenge,
        DescriptionColor = "#FF2A2A",
        RequiredTab = BacapTab.Challenges

    };

    private static readonly BacapTierProfile HiddenTierProfile = new()
    {
        DescriptionColor = "light_purple"
    };

    private static readonly BacapTierProfile ChallengeTierProfile = new()
    {
        Frame = AdvancementFrame.Challenge,
        DescriptionColor = "dark_purple"
    };

    private static readonly BacapTierProfile GoalTierProfile = new()
    {
        Frame = AdvancementFrame.Goal,
        DescriptionColor = "#75E1FF"
    };

    private static readonly BacapTierProfile TaskTierProfile = new()
    {
        Frame = AdvancementFrame.Task,
        DescriptionColor = "green"
    };


    /// <summary>
    /// Resolves default tier profile for the specified advancement tier.
    /// </summary>
    /// <param name="tier">The advancement tier to resolve.</param>
    /// <returns>The pre-allocated <see cref="BacapTierProfile"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="tier"/> contains an undefined enum value.</exception>
    /// <example>
    /// <code>
    /// AdvancementMessageSettingsEntry entry = BacapAdvancementTier.Challenge.GetDefaultMessage();
    /// </code>
    /// </example>
    public static BacapTierProfile GetTierProfile(this BacapAdvancementTier tier) => tier switch
    {
        BacapAdvancementTier.AdvancementLegend => AdvancementLegendTierProfile,
        BacapAdvancementTier.Milestone => MilestoneTierProfile,
        BacapAdvancementTier.SuperChallenge => SuperChallengeTierProfile,
        BacapAdvancementTier.Hidden => HiddenTierProfile,
        BacapAdvancementTier.Challenge => ChallengeTierProfile,
        BacapAdvancementTier.Goal => GoalTierProfile,
        BacapAdvancementTier.Task => TaskTierProfile,
        _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, "Unsupported advancement tier.")
    };
}