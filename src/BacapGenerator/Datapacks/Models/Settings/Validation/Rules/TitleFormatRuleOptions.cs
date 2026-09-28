using System.Collections.Frozen;
using BacapGenerator.Configuration;
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
    /// <summary>
    /// Gets the raw string representation indicating whether default minor words should be included in Title Case evaluation.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("use_default_minor_words")]
    public string? RawUseDefaultMinorWords { get; init; }

    /// <summary>
    /// Gets a value indicating whether default minor words should be included in Title Case evaluation.
    /// Defaults to <see langword="true"/>.
    /// </summary>
    [PublicAPI]
    public bool IsDefaultMinorWordsUsed { get; private set; } = true;

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
    /// Validates Title Case rule options, parsing raw boolean values and ensuring no empty items exist in exclusion lists.
    /// </summary>
    /// <param name="datapackId">The parent datapack identifier.</param>
    /// <param name="ruleName">The configuration key of this rule.</param>
    /// <exception cref="DatapackConfigurationException">
    /// Thrown when <see cref="RawUseDefaultMinorWords"/> cannot be parsed as a boolean, or when exclusion lists contain null or whitespace-only elements.
    /// </exception>
    /// <example>
    /// <code>
    /// var titleOptions = new TitleFormatRuleOptions
    /// {
    ///     RawUseDefaultMinorWords = "false"
    /// };
    /// titleOptions.Validate("bacaped", "title_case_formatting");
    /// </code>
    /// </example>
    public override void Validate(string datapackId, string ruleName)
    {
        base.Validate(datapackId, ruleName);

        IsDefaultMinorWordsUsed = ConfigParser.ParseBool(
            RawUseDefaultMinorWords,
            defaultValue: true,
            datapackId,
            DatapackErrorKind.InvalidValidationRuleConfiguration,
            $"Invalid boolean value '{RawUseDefaultMinorWords}' for 'use_default_minor_words' in rule '{ruleName}' of datapack '{datapackId}'.");

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

        if (IsDefaultMinorWordsUsed)
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