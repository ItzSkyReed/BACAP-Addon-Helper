using System.Collections.Frozen;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using BacapGenerator.Converters;
using BacapGenerator.Utils;

namespace BacapGenerator.Models.Advancements;

/// <summary>
/// Represents a predefined BACAP advancement tab containing its folder identifier and display name.
/// </summary>
[TypeConverter(typeof(BacapAdvancementTabConverter))]
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

    /// <summary>
    /// Gets the color associated with the advancement.
    /// </summary>
    public string Color { get; }

    private BacapAdvancementTab(string folderName, string displayName, string color)
    {
        FolderName = folderName;
        DisplayName = displayName;
        Color = color;
    }

    public static readonly BacapAdvancementTab Adventure = new("adventure", "Adventure", "#FFD966");
    public static readonly BacapAdvancementTab Animal = new("animal", "Animals", "#6AA84F");
    public static readonly BacapAdvancementTab Bacap = new("bacap", "B&C Advancements", "#F6B26B");
    public static readonly BacapAdvancementTab Biomes = new("biomes", "Biomes", "#6AA84F");
    public static readonly BacapAdvancementTab Building = new("building", "Building", "#E69138");
    public static readonly BacapAdvancementTab Challenges = new("challenges", "Super Challenges", "#FF0003");
    public static readonly BacapAdvancementTab Enchanting = new("enchanting", "Enchanting", "#5B2AFF");
    public static readonly BacapAdvancementTab End = new("end", "The End", "#FFF2CC");
    public static readonly BacapAdvancementTab Farming = new("farming", "Farming", "#CCAC66");
    public static readonly BacapAdvancementTab Mining = new("mining", "Mining", "#999999");
    public static readonly BacapAdvancementTab Monsters = new("monsters", "Monsters", "#93AF90");
    public static readonly BacapAdvancementTab Nether = new("nether", "Nether", "#E06666");
    public static readonly BacapAdvancementTab Potion = new("potion", "Potions", "#FFD966");
    public static readonly BacapAdvancementTab Redstone = new("redstone", "Redstone", "#CC0000");
    public static readonly BacapAdvancementTab Statistics = new("statistics", "Statistics", "#E69138");
    public static readonly BacapAdvancementTab Weaponry = new("weaponry", "Weaponry", "#9D7F56");

    /// <summary>
    /// Gets the collection of all predefined tabs.
    /// </summary>
    public static IReadOnlyCollection<BacapAdvancementTab> All { get; } =
    [
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

    /// <summary>
    /// Extracts and resolves a <see cref="BacapAdvancementTab"/> from a parent McPath.
    /// </summary>
    /// <param name="mcPath">The parent McPath (e.g. <c>"minecraft:adventure/root"</c> or <c>"bacap:nether/explore"</c>).</param>
    /// <param name="tab">When this method returns, contains the resolved <see cref="BacapAdvancementTab"/> if successful; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if a valid tab was resolved; otherwise, <see langword="false"/>.</returns>
    public static bool TryExtractTabFromMcPath(string mcPath, [NotNullWhen(true)] out BacapAdvancementTab? tab)
    {
        tab = null;
        if (string.IsNullOrWhiteSpace(mcPath))
        {
            return false;
        }

        var stripped = MinecraftUtils.StripNamespace(mcPath.Trim());
        var slashIndex = stripped.IndexOf('/');
        var folderName = slashIndex > 0 ? stripped[..slashIndex] : stripped;

        return TryFromFolderName(folderName, out tab)
               || (BacapUtils.TryExtractTab(mcPath, out var tabName) && TryFromFolderName(tabName, out tab));
    }

    public override string ToString() => DisplayName;
}