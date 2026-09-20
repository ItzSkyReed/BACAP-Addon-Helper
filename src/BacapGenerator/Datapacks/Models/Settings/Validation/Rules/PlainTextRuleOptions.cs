using System.Collections.Frozen;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;

namespace BacapGenerator.Datapacks.Models.Settings.Validation.Rules;

/// <summary>
/// Configuration options controlling the validation of plain text usage in advancement text components.
/// </summary>
public sealed class PlainTextRuleOptions : GenericRuleOptions
{
    /// <summary>
    /// The default baseline of widely accepted plain text acronyms and version strings.
    /// </summary>
    public static readonly string[] DefaultAllowedStrings =
    [
        "/trigger bac_timers", "/trigger bac_progress", "/trigger bac_dragon", "/trigger bac_statistics"
    ];

    /// <summary>
    /// Gets a value indicating whether the built-in <see cref="DefaultAllowedStrings"/>
    /// should be automatically included in the allowed list.
    /// Defaults to <see langword="true"/>.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("include_default_strings")]
    public bool IncludeDefaultStrings { get; init; } = true;

    /// <summary>
    /// Gets the optional list of custom plain text values explicitly permitted in titles and descriptions.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("allowed_strings")]
    public List<string>? AllowedStrings { get; init; }

    /// <summary>
    /// Gets an immutable frozen set lookup for case-insensitive checks against allowed plain text literals.
    /// Automatically merges defaults with user-defined strings based on configuration.
    /// </summary>
    [PublicAPI]
    public FrozenSet<string> AllowedStringsSet => field ??= BuildAllowedStringsSet();

    private FrozenSet<string> BuildAllowedStringsSet()
    {
        // Using a HashSet first ensures no duplicates when merging the two lists
        var combined = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (IncludeDefaultStrings)
        {
            combined.UnionWith(DefaultAllowedStrings);
        }

        if (AllowedStrings is { Count: > 0 })
        {
            combined.UnionWith(AllowedStrings);
        }

        return combined.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    }
}