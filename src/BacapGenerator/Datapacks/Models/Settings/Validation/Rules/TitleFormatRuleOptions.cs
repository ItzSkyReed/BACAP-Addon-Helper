using System.Collections.Frozen;
using BacapGenerator.Configuration.Exceptions;
using BacapGenerator.Utils;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;

namespace BacapGenerator.Datapacks.Models.Settings.Validation.Rules;

/// <summary>
/// Configuration options for advancement title formatting validation.
/// </summary>
public sealed class TitleFormatRuleOptions : GenericRuleOptions
{
    [PublicAPI]
    [ConfigurationKeyName("use_default_minor_words")]
    public bool UseDefaultMinorWords { get; init; } = true;

    [PublicAPI]
    [ConfigurationKeyName("minor_words")]
    public List<string>? MinorWords { get; init; }

    [PublicAPI]
    public FrozenSet<string> MinorWordsSet => field ??= BuildMinorWordsSet();

    [PublicAPI]
    [ConfigurationKeyName("allowed_titles")]
    public List<string>? AllowedTitles { get; init; }

    [PublicAPI]
    public FrozenSet<string> AllowedTitlesSet => field ??= BuildAllowedTitlesSet();

    /// <summary>
    /// Validates Title Case rule options ensuring no empty items exist in exclusion lists.
    /// </summary>
    /// <param name="datapackId">The parent datapack identifier.</param>
    /// <param name="ruleName">The configuration key of this rule.</param>
    /// <exception cref="DatapackConfigurationException">Thrown when lists contain null or whitespace-only elements.</exception>
    /// <example>
    /// <code>
    /// titleOptions.Validate("bacaped", "title_case_formatting");
    /// </code>
    /// </example>
    public override void Validate(string datapackId, string ruleName)
    {
        base.Validate(datapackId, ruleName);

        if (MinorWords is not null && MinorWords.Any(string.IsNullOrWhiteSpace))
        {
            throw new DatapackConfigurationException(
                datapackId,
                DatapackErrorKind.InvalidValidationRuleConfiguration,
                $"Rule '{ruleName}' in datapack '{datapackId}' contains empty or whitespace entries in '{nameof(MinorWords)}'.",
                nameof(MinorWords));
        }

        if (AllowedTitles is not null && AllowedTitles.Any(string.IsNullOrWhiteSpace))
        {
            throw new DatapackConfigurationException(
                datapackId,
                DatapackErrorKind.InvalidValidationRuleConfiguration,
                $"Rule '{ruleName}' in datapack '{datapackId}' contains empty or whitespace entries in '{nameof(AllowedTitles)}'.",
                nameof(AllowedTitles));
        }
    }

    private FrozenSet<string> BuildMinorWordsSet()
    {
        var combined = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (UseDefaultMinorWords)
            combined.UnionWith(StringExtensions.DefaultMinorWords);

        if (MinorWords is not { Count: > 0 })
            return combined.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

        foreach (var word in MinorWords.Where(word => !string.IsNullOrWhiteSpace(word)))
            combined.Add(word.Trim());

        return combined.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    }

    private FrozenSet<string> BuildAllowedTitlesSet()
    {
        if (AllowedTitles is not { Count: > 0 })
            return FrozenSet<string>.Empty;

        var combined = new HashSet<string>(StringComparer.Ordinal);
        foreach (var title in AllowedTitles.Where(title => !string.IsNullOrWhiteSpace(title)))
            combined.Add(title.Trim());

        return combined.ToFrozenSet(StringComparer.Ordinal);
    }
}