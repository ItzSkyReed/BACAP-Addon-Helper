using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;

namespace BacapGenerator.Datapacks.Models.Settings.Checklists;

/// <summary>
/// Defines styling and visual formatting options for checklist tellraw output.
/// </summary>
public sealed class ChecklistStyleSettings
{
    /// <summary>
    /// Gets the hex or named color for completed entities.
    /// Defaults to <c>#00a523</c> (green).
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("complete_color")]
    public string CompleteColor { get; init; } = "#00a523";

    /// <summary>
    /// Gets the hex or named color for missing/incomplete entities.
    /// Defaults to <c>#c2c2c2</c> (light grey).
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("incomplete_color")]
    public string IncompleteColor { get; init; } = "#c2c2c2";

    /// <summary>
    /// Gets the color of delimiter tokens (commas, "and").
    /// Defaults to <c>#c2c2c2</c>.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("separator_color")]
    public string SeparatorColor { get; init; } = "#c2c2c2";

    /// <summary>
    /// Gets the color used for upper and lower strikethrough divider lines.
    /// Defaults to <c>dark_gray</c>.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("divider_color")]
    public string DividerColor { get; init; } = DatapackDefaults.DefaultDividerColor;

    /// <summary>
    /// Gets the string used as visual header/footer divider.
    /// </summary>
    [PublicAPI]
    [ConfigurationKeyName("divider_text")]
    public string DividerText { get; init; } = DatapackDefaults.DefaultDivider;
}