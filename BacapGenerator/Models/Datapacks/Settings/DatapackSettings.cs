using JetBrains.Annotations;

namespace BacapGenerator.Models.Datapacks.Settings;

[UsedImplicitly]
public record  DatapackSettings
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
    /// Main namespace of the datapack
    /// </summary>
    public required string MainNamespace { get; init; }

    /// <summary>
    /// The raw string path to the resourcepack with translations.
    /// </summary>
    public string? LanguagePackPath { get; init; }

    /// <summary>
    /// Access mode (only parse or also generate files)
    /// </summary>
    public required DatapackAccess Access { get; init; } = DatapackAccess.ReadOnly;

    public required AdvancementMessageSettings? AdvancementMessageSettings { get; init; }

    /// <summary>
    /// Gets the precalculated macro command name.
    /// Computed once on first access and cached.
    /// </summary>
    public string MacroCommandName => field ??= $"{RewardNamespace}:advancement_made_macro";

    /// <summary>
    /// Gets or sets the ID of the parent datapack (e.g., "bacaped").
    /// If specified, this datapack will act as a compatibility addon, overriding only modified components.
    /// </summary>
    [PublicAPI]
    public string? ParentDatapackId { get; init; }

    /// <summary>
    /// Gets or sets the mapping of tab folder names to their milestone advancement Minecraft paths.
    /// </summary>
    [PublicAPI]
    public Dictionary<string, string>? MilestoneMcPaths { get; init; }

    [PublicAPI]
    public string? AdvancementLegendMcPath { get; init; }

    /// <summary>
    /// Validates the current settings object based on the access mode.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if validation fails.</exception>
    public void Validate()
    {
        if (Access != DatapackAccess.ReadWrite)
            return;

        if (string.IsNullOrWhiteSpace(LanguagePackPath))
            throw new InvalidOperationException($"'{nameof(LanguagePackPath)}' must be provided when Access is set to ReadWrite.");

        if (AdvancementMessageSettings == null)
            throw new InvalidOperationException($"'{nameof(AdvancementMessageSettings)}' must be provided when Access is set to ReadWrite.");
    }
}