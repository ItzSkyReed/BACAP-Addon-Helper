using BacapGenerator.Models.Advancements;

namespace BacapGenerator.Models.Datapacks;

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
/// Provides default tier announcements and domain metadata for datapacks.
/// </summary>
public static class DatapackDefaults
{
    public const string AdvancementsScoreboard = "bac_advancements";
    public const string PointsScoreboard = "bac_advancements_points";
    public const string CoopBaseScoreboard = "bac_obtained";


    private static readonly AdvancementMessageSettingsEntry AdvancementLegendEntry = new()
    {
        TranslationKey = "%1$s has completed every advancement in the %2$s tab, earning them the advancement %3$s%4$s%5$s",
        TitleColor = "gold",
        DescriptionColor = "gold"
    };

    private static readonly AdvancementMessageSettingsEntry MilestoneEntry = new()
    {
        TranslationKey = "%1$s has completed every advancement in the %2$s tab, earning them the advancement %3$s%4$s%5$s",
        TitleColor = "yellow",
        DescriptionColor = "#E5E74F"
    };

    private static readonly AdvancementMessageSettingsEntry SuperChallengeEntry = new()
    {
        TranslationKey = "%1$s has completed the super challenge %2$s%3$s%4$s",
        TitleColor = "#FF2A2A",
        DescriptionColor = "#DC2727"
    };

    private static readonly AdvancementMessageSettingsEntry HiddenEntry = new()
    {
        TranslationKey = "%1$s has found the hidden advancement %2$s%3$s%4$s",
        TitleColor = "light_purple",
        DescriptionColor = "#DE4ADC"
    };

    private static readonly AdvancementMessageSettingsEntry ChallengeEntry = new()
    {
        TranslationKey = "%1$s has completed the challenge %2$s%3$s%4$s",
        TitleColor = "dark_purple",
        DescriptionColor = "#C900C7"
    };

    private static readonly AdvancementMessageSettingsEntry GoalEntry = new()
    {
        TranslationKey = "%1$s has reached the goal %2$s%3$s%4$s",
        TitleColor = "#75E1FF",
        DescriptionColor = "#63BDD7"
    };

    private static readonly AdvancementMessageSettingsEntry TaskEntry = new()
    {
        TranslationKey = "%1$s has made the advancement %2$s%3$s%4$s",
        TitleColor = "green",
        DescriptionColor = "#49DB49"
    };

    /// <summary>
    /// Resolves default message settings for the specified advancement tier via a jump-table switch.
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
}