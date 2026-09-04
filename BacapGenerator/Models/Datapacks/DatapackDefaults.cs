using BacapGenerator.Models.Advancements;
using BacapGenerator.Models.Datapacks.Settings;

namespace BacapGenerator.Models.Datapacks;

/// <summary>
/// Provides domain-specific defaults, namespaces, and standard metadata for known datapacks.
/// </summary>
public static class DatapackDefaults
{
    /// <summary>
    /// Default message formatting entries by advancement tier according to BACAP standards.
    /// </summary>
    public static readonly IReadOnlyDictionary<BacapAdvancementTier, AdvancementMessageSettingsEntry> DefaultTierMessages =
        new Dictionary<BacapAdvancementTier, AdvancementMessageSettingsEntry>
        {
            [BacapAdvancementTier.AdvancementLegend] = new()
            {
                TranslationKey = "%1$s has completed every advancement in the %2$s tab, earning them the advancement %3$s%4$s%5$s",
                TitleColor = "gold",
                DescriptionColor = "gold"
            },
            [BacapAdvancementTier.Milestone] = new()
            {
                TranslationKey = "%1$s has completed every advancement in the %2$s tab, earning them the advancement %3$s%4$s%5$s",
                TitleColor = "yellow",
                DescriptionColor = "#E5E74F"
            },
            [BacapAdvancementTier.SuperChallenge] = new()
            {
                TranslationKey = "%1$s has completed the super challenge %2$s%3$s%4$s",
                TitleColor = "#FF2A2A",
                DescriptionColor = "#DC2727"
            },
            [BacapAdvancementTier.Hidden] = new()
            {
                TranslationKey = "%1$s has found the hidden advancement %2$s%3$s%4$s",
                TitleColor = "light_purple",
                DescriptionColor = "#DE4ADC"
            },
            [BacapAdvancementTier.Challenge] = new()
            {
                TranslationKey = "%1$s has completed the challenge %2$s%3$s%4$s",
                TitleColor = "dark_purple",
                DescriptionColor = "#C900C7"
            },
            [BacapAdvancementTier.Goal] = new()
            {
                TranslationKey = "%1$s has reached the goal %2$s%3$s%4$s",
                TitleColor = "#75E1FF",
                DescriptionColor = "#63BDD7"
            },
            [BacapAdvancementTier.Task] = new()
            {
                TranslationKey = "%1$s has made the advancement %2$s%3$s%4$s",
                TitleColor = "green",
                DescriptionColor = "#49DB49"
            }
        };

    /// <summary>
    /// Creates the standard mapping of milestone paths for all category tabs, excluding the root hub tab (<see cref="BacapAdvancementTab.Bacap"/>).
    /// </summary>
    /// <param name="mainNamespace">The primary datapack namespace (e.g., 'bacaped').</param>
    /// <returns>A dictionary mapping folder names to their milestone advancement Minecraft paths.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="mainNamespace"/> is null or whitespace.</exception>
    /// <example>
    /// <code>
    /// Dictionary&lt;string, string&gt; milestones = DatapackDefaults.CreateDefaultMilestones("bacaped");
    /// // milestones["adventure"] => "bacaped:bacap/enhanced_adventure_milestone"
    /// </code>
    /// </example>
    public static Dictionary<string, string> CreateDefaultMilestones(string mainNamespace = "bacaped")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mainNamespace);

        return BacapAdvancementTab.All
            .Where(tab => tab != BacapAdvancementTab.Bacap)
            .ToDictionary(
                tab => tab.FolderName,
                tab => $"{mainNamespace}:bacap/enhanced_{tab.FolderName}_milestone",
                StringComparer.OrdinalIgnoreCase
            );
    }

    /// <summary>
    /// Creates a strongly-typed mapping of milestone paths for all category tabs, excluding the root hub tab (<see cref="BacapAdvancementTab.Bacap"/>).
    /// </summary>
    /// <param name="mainNamespace">The primary datapack namespace (e.g., 'bacaped').</param>
    /// <returns>A dictionary mapping <see cref="BacapAdvancementTab"/> instances to advancement Minecraft paths.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="mainNamespace"/> is null or whitespace.</exception>
    public static Dictionary<BacapAdvancementTab, string> CreateDefaultTypedMilestones(string mainNamespace = "bacaped")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mainNamespace);

        return BacapAdvancementTab.All
            .Where(tab => tab != BacapAdvancementTab.Bacap)
            .ToDictionary(
                tab => tab,
                tab => $"{mainNamespace}:bacap/enhanced_{tab.FolderName}_milestone"
            );
    }
}