using System.Collections.Frozen;
using BacapGenerator.Utils;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;

namespace BacapGenerator.Datapacks.Models.Settings.Validation.Rules;

/// <summary>
/// Configuration options for advancement title formatting validation.
/// </summary>
public sealed class TitleFormatRuleOptions : GenericRuleOptions
{
    // === Minor Words (Articles/Prepositions) ===

    [PublicAPI]
    [ConfigurationKeyName("use_default_minor_words")]
    public bool UseDefaultMinorWords { get; init; } = true;

    [PublicAPI]
    [ConfigurationKeyName("minor_words")]
    public List<string>? MinorWords { get; init; }

    [PublicAPI]
    public FrozenSet<string> MinorWordsSet => field ??= BuildMinorWordsSet();


    /// <summary>
    /// Gets the list of exact advancement titles that bypass the Title Case validation entirely.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("allowed_titles")]
    public List<string>? AllowedTitles { get; init; }

    /// <summary>
    /// Gets an immutable frozen set lookup for case-sensitive strict matches of exact allowed titles.
    /// </summary>
    [PublicAPI]
    public FrozenSet<string> AllowedTitlesSet => field ??= AllowedTitles is { Count: > 0 }
        ? AllowedTitles.ToFrozenSet(StringComparer.Ordinal)
        : FrozenSet<string>.Empty;

    private FrozenSet<string> BuildMinorWordsSet()
    {
        var combined = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (UseDefaultMinorWords)
            combined.UnionWith(StringExtensions.DefaultMinorWords);

        if (MinorWords is { Count: > 0 })
            combined.UnionWith(MinorWords);

        return combined.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    }
}