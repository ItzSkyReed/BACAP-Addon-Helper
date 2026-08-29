using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;

namespace BacapGenerator.Models.Advancements;

/// <summary>
/// Represents a predefined BACAP advancement tab containing its folder identifier and display name.
/// </summary>
public sealed record BacapAdvancementTab
{
    /// <summary>
    /// Gets the folder name / identifier used in the data pack file structure.
    /// </summary>
    public string FolderName { get; }

    /// <summary>
    /// Gets the human-readable display title of the tab.
    /// </summary>
    public string DisplayName { get; }

    private BacapAdvancementTab(string folderName, string displayName)
    {
        FolderName = folderName;
        DisplayName = displayName;
    }

    public static readonly BacapAdvancementTab Adventure = new("adventure", "Adventure");
    public static readonly BacapAdvancementTab Animal = new("animal", "Animals");
    public static readonly BacapAdvancementTab Bacap = new("bacap", "B&C Advancements");
    public static readonly BacapAdvancementTab Biomes = new("biomes", "Biomes");
    public static readonly BacapAdvancementTab Building = new("building", "Building");
    public static readonly BacapAdvancementTab Challenges = new("challenges", "Super Challenges");
    public static readonly BacapAdvancementTab Enchanting = new("enchanting", "Enchanting");
    public static readonly BacapAdvancementTab End = new("end", "The End");
    public static readonly BacapAdvancementTab Farming = new("farming", "Farming");
    public static readonly BacapAdvancementTab Mining = new("mining", "Mining");
    public static readonly BacapAdvancementTab Monsters = new("monsters", "Monsters");
    public static readonly BacapAdvancementTab Nether = new("nether", "Nether");
    public static readonly BacapAdvancementTab Potion = new("potion", "Potions");
    public static readonly BacapAdvancementTab Redstone = new("redstone", "Redstone");
    public static readonly BacapAdvancementTab Statistics = new("statistics", "Statistics");
    public static readonly BacapAdvancementTab Weaponry = new("weaponry", "Weaponry");

    /// <summary>
    /// Gets the collection of all predefined tabs.
    /// </summary>
    public static IReadOnlyCollection<BacapAdvancementTab> All { get; } = [
        Adventure, Animal, Bacap, Biomes, Building, Challenges,
        Enchanting, End, Farming, Mining, Monsters, Nether,
        Potion, Redstone, Statistics, Weaponry
    ];

    private static readonly FrozenDictionary<string, BacapAdvancementTab> Lookup =
        All.ToFrozenDictionary(tab => tab.FolderName, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Attempts to resolve a <see cref="BacapAdvancementTab"/> by its folder name.
    /// </summary>
    /// <param name="folderName">The folder name to search for (case-insensitive).</param>
    /// <param name="tab">When this method returns, contains the tab if found; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if the tab was found; otherwise, <c>false</c>.</returns>
    public static bool TryFromFolderName(string? folderName, [NotNullWhen(true)] out BacapAdvancementTab? tab)
    {
        if (!string.IsNullOrWhiteSpace(folderName))
            return Lookup.TryGetValue(folderName, out tab);

        tab = null;
        return false;

    }

    /// <summary>
    /// Resolves a <see cref="BacapAdvancementTab"/> by its folder name or throws an exception if not found.
    /// </summary>
    /// <param name="folderName">The folder name to search for (case-insensitive).</param>
    /// <returns>The resolved <see cref="BacapAdvancementTab"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when the folder name is null, whitespace, or does not match any known tab.</exception>
    /// <example>
    /// <code>
    /// BacapAdvancementTab tab = BacapAdvancementTab.FromFolderName("adventure");
    /// Console.WriteLine(tab.DisplayName); // "Adventure"
    /// </code>
    /// </example>
    public static BacapAdvancementTab FromFolderName(string folderName)
    {
        return !TryFromFolderName(folderName, out var tab)
            ? throw new ArgumentException($"Unknown BACAP advancement tab: '{folderName}'", nameof(folderName))
            : tab;
    }

    public override string ToString() => DisplayName;
}