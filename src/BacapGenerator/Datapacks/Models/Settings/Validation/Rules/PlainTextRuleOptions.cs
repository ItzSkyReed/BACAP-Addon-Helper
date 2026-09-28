using System.Collections.Frozen;
using BacapGenerator.Configuration;
using BacapGenerator.Configuration.Exceptions;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;

namespace BacapGenerator.Datapacks.Models.Settings.Validation.Rules;

/// <summary>
/// Configuration options controlling the validation of plain text usage in advancement text components.
/// </summary>
public sealed class PlainTextRuleOptions : GenericRuleOptions
{
    public static readonly string[] DefaultAllowedStrings =
    [
        "/trigger bac_timers", "/trigger bac_progress", "/trigger bac_dragon", "/trigger bac_statistics"
    ];

    /// <summary>
    /// Gets the raw string representation indicating whether default bypass strings should be included.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("include_default_strings")]
    public string? RawIncludeDefaultStrings { get; init; }

    /// <summary>
    /// Gets a value indicating whether default bypass strings should be included.
    /// Defaults to <see langword="true"/>.
    /// </summary>
    [PublicAPI]
    public bool IsDefaultStringsIncluded { get; private set; } = true;

    [PublicAPI]
    [ConfigurationKeyName("allowed_strings")]
    public List<string>? AllowedStrings { get; init; }

    [PublicAPI]
    public FrozenSet<string> AllowedStringsSet => field ??= BuildAllowedStringsSet();

    /// <summary>
    /// Validates plain text rule options, parsing raw boolean values and ensuring no empty bypass strings are registered.
    /// </summary>
    /// <param name="datapackId">The parent datapack identifier.</param>
    /// <param name="ruleName">The configuration key of this rule.</param>
    /// <exception cref="DatapackConfigurationException">
    /// Thrown when <see cref="RawIncludeDefaultStrings"/> cannot be parsed as a boolean, or when <see cref="AllowedStrings"/> contains null or whitespace-only elements.
    /// </exception>
    /// <example>
    /// <code>
    /// var plainTextOptions = new PlainTextRuleOptions
    /// {
    ///     RawIncludeDefaultStrings = "false"
    /// };
    /// plainTextOptions.Validate("bacaped", "plain_text_usage");
    /// </code>
    /// </example>
    public override void Validate(string datapackId, string ruleName)
    {
        base.Validate(datapackId, ruleName);

        IsDefaultStringsIncluded = ConfigParser.ParseBool(
            RawIncludeDefaultStrings,
            defaultValue: true,
            datapackId,
            DatapackErrorKind.InvalidValidationRuleConfiguration,
            $"Invalid boolean value '{RawIncludeDefaultStrings}' for 'include_default_strings' in rule '{ruleName}' of datapack '{datapackId}'.");

        if (AllowedStrings is not null && AllowedStrings.Any(string.IsNullOrWhiteSpace))
        {
            throw new DatapackConfigurationException(
                datapackId,
                DatapackErrorKind.InvalidValidationRuleConfiguration,
                $"Rule '{ruleName}' in datapack '{datapackId}' contains empty or whitespace entries in '{nameof(AllowedStrings)}'.",
                nameof(AllowedStrings));
        }
    }

    private FrozenSet<string> BuildAllowedStringsSet()
    {
        var combined = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (IsDefaultStringsIncluded)
            combined.UnionWith(DefaultAllowedStrings);

        if (AllowedStrings is not { Count: > 0 })
            return combined.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

        foreach (var str in AllowedStrings.Where(str => !string.IsNullOrWhiteSpace(str)))
            combined.Add(str.Trim());

        return combined.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    }
}