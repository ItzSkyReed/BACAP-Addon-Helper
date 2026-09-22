using System.Text.RegularExpressions;
using BacapGenerator.Configuration.Exceptions;
using Core.Registries;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;

namespace BacapGenerator.Datapacks.Models.Settings.Checklists;

/// <summary>
/// Defines styling and visual formatting options for checklist tellraw output.
/// </summary>
public sealed partial class ChecklistStyleSettings
{
    [GeneratedRegex("^#[0-9a-fA-F]{6}$")]
    private static partial Regex HexColorRegex();

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

    /// <summary>
    /// Validates checklist colors against registered Minecraft text colors and hex formats.
    /// </summary>
    /// <param name="datapackId">The parent datapack identifier.</param>
    /// <param name="checklistId">The owning checklist identifier.</param>
    /// <param name="minecraftData">The static Minecraft registries container.</param>
    /// <exception cref="DatapackConfigurationException">Thrown when a color is unrecognized or empty.</exception>
    /// <example>
    /// <code>
    /// style.Validate("bacaped", "mob_universe", minecraftData);
    /// </code>
    /// </example>
    public void Validate(string datapackId, string checklistId, MinecraftData minecraftData)
    {
        ArgumentNullException.ThrowIfNull(minecraftData);

        ValidateColor(CompleteColor, nameof(CompleteColor), datapackId, checklistId, minecraftData);
        ValidateColor(IncompleteColor, nameof(IncompleteColor), datapackId, checklistId, minecraftData);
        ValidateColor(SeparatorColor, nameof(SeparatorColor), datapackId, checklistId, minecraftData);
        ValidateColor(DividerColor, nameof(DividerColor), datapackId, checklistId, minecraftData);

        if (string.IsNullOrEmpty(DividerText))
            throw new DatapackConfigurationException(
                datapackId,
                DatapackErrorKind.InvalidChecklistConfiguration,
                $"Checklist '{checklistId}' in datapack '{datapackId}' has an empty '{nameof(DividerText)}'.",
                nameof(DividerText));
    }

    /// <summary>
    /// Verifies if a color string matches a 6-digit hex format or exists in Minecraft's text color registry.
    /// </summary>
    /// <param name="color">The color token to validate.</param>
    /// <param name="propertyName">The property name for diagnostic output.</param>
    /// <param name="datapackId">The parent datapack identifier.</param>
    /// <param name="checklistId">The owning checklist identifier.</param>
    /// <param name="minecraftData">The static Minecraft registries container.</param>
    /// <exception cref="DatapackConfigurationException">Thrown when the color is invalid.</exception>
    internal static void ValidateColor(
        string? color,
        string propertyName,
        string datapackId,
        string checklistId,
        MinecraftData minecraftData)
    {
        if (string.IsNullOrWhiteSpace(color))
        {
            throw new DatapackConfigurationException(
                datapackId,
                DatapackErrorKind.InvalidChecklistConfiguration,
                $"Color property '{propertyName}' in checklist '{checklistId}' of datapack '{datapackId}' cannot be empty.",
                propertyName);
        }

        var trimmed = color.Trim();

        // Valid hex color (#RRGGBB)
        if (HexColorRegex().IsMatch(trimmed))
            return;

        // Named color in MinecraftData registry
        if (minecraftData.TextColors.ContainsKey(trimmed.ToLowerInvariant()))
            return;

        throw new DatapackConfigurationException(
            datapackId,
            DatapackErrorKind.InvalidChecklistConfiguration,
            $"Color '{color}' for '{propertyName}' in checklist '{checklistId}' is not a valid hex code ('#RRGGBB') and was not found in Minecraft text colors registry.",
            propertyName);
    }
}