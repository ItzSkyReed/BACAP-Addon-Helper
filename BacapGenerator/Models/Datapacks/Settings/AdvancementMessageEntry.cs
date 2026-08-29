namespace BacapGenerator.Models.Datapacks.Settings;

/// <summary>
/// Represents the visual settings for a specific advancement tier announcement.
/// </summary>
public record AdvancementMessageSettingsEntry
{
    /// <summary>
    /// Completion message translation key.
    /// </summary>
    /// <example>
    /// %1$s has found the hidden advancement %2$s%3$s%4$s
    /// </example>
    public required string TranslationKey { get; init; }

    /// <summary>
    /// Color of the title of advancement.
    /// </summary>
    public required string TitleColor { get; init; }

    /// <summary>
    /// Color of the description of advancement.
    /// </summary>
    public required string DescriptionColor { get; init; }
}