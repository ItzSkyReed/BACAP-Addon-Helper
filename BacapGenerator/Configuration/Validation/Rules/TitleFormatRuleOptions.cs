using BacapGenerator.Utils;
using JetBrains.Annotations;

namespace BacapGenerator.Configuration.Validation.Rules;

using System.Collections.Frozen;
using BacapGenerator.Validation.Models;

/// <summary>
/// Configuration options for advancement title formatting validation.
/// </summary>
public sealed class TitleFormatRuleOptions
{
    /// <summary>
    /// Gets or sets whether this rule is executed during validation.
    /// </summary>
    [PublicAPI]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the severity level reported for violations.
    /// </summary>
    [PublicAPI]
    public ValidationSeverity Severity { get; set; } = ValidationSeverity.Warning;

    /// <summary>
    /// Gets or sets the list of minor words loaded from configuration.
    /// </summary>
    [PublicAPI]
    public List<string>? MinorWords { get; set; }

    /// <summary>
    /// Gets a frozen set lookup compiled from <see cref="MinorWords"/> or defaults to <see cref="StringExtensions.DefaultMinorWords"/>.
    /// </summary>
    [PublicAPI]
    public FrozenSet<string> MinorWordsSet => field ??= MinorWords is { Count: > 0 }
        ? MinorWords.ToFrozenSet(StringComparer.OrdinalIgnoreCase)
        : StringExtensions.DefaultMinorWords;
}