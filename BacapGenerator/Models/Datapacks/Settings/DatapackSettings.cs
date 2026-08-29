namespace BacapGenerator.Models.Datapacks.Settings;

public record DatapackSettings
{
    /// <summary>
    /// Path to the datapack
    /// </summary>
    public required string DatapackPath { get; init; }

    /// <summary>
    /// Name of the namespace for Rewards (trophies, msgs, items etc.)
    /// </summary>
    public required string RewardNamespace { get; init; }

    /// <summary>
    /// Path to the resourcepack with translations to find missing ones
    /// </summary>
    public FileInfo? LanguagePackPath { get; init; }

    /// <summary>
    /// Access mode (only parse or also generate files)
    /// </summary>
    public required DatapackAccess Access { get; init; } = DatapackAccess.ReadOnly;

    public required AdvancementMessageSettings? AdvancementMessageSettings { get; init; }


    /// <summary>
    /// Validates the current settings object based on the access mode.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if validation fails.</exception>
    public void Validate()
    {
        if (Access == DatapackAccess.ReadWrite)
        {
            if (LanguagePackPath == null)
                throw new InvalidOperationException($"'{nameof(LanguagePackPath)}' must be provided when Access is set to ReadWrite.");

            if (AdvancementMessageSettings == null)
                throw new InvalidOperationException($"'{nameof(AdvancementMessageSettings)}' must be provided when Access is set to ReadWrite.");
        }
    }
}