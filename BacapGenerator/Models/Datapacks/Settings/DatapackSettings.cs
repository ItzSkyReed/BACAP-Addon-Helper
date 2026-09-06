using BacapGenerator.Models.Advancements;
using JetBrains.Annotations;

namespace BacapGenerator.Models.Datapacks.Settings;

/// <summary>
/// Represents the configuration, filesystem paths, and domain metadata for a datapack.
/// Supports direct binding from configuration providers as well as manual factory instantiation.
/// </summary>
public sealed class DatapackSettings
{
    /// <summary>
    /// Gets or sets the filesystem path to the root folder of the datapack.
    /// Bound directly from the configuration file.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional filesystem path to the language resource pack folder.
    /// Bound directly from the configuration file.
    /// </summary>
    public string? LanguagePackPath { get; set; }

    /// <summary>
    /// Gets the primary namespace containing the advancements.
    /// </summary>
    public string MainNamespace { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the namespace used for custom rewards, functions, and internal macros.
    /// </summary>
    public string RewardNamespace { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the access permission mode determining whether file modifications are permitted.
    /// </summary>
    public DatapackAccess Access { get; private set; } = DatapackAccess.ReadOnly;

    /// <summary>
    /// Gets the message announcement visual configuration by advancement tier.
    /// </summary>
    public AdvancementMessageSettings? AdvancementMessageSettings { get; private set; }

    /// <summary>
    /// Gets the precalculated macro command identifier.
    /// </summary>
    public string MacroCommandName => field ??= $"{RewardNamespace}:advancement_made_macro";

    /// <summary>
    /// Gets the optional identifier of the parent datapack when acting as an override addon.
    /// </summary>
    [PublicAPI]
    public DatapackId? ParentDatapackId { get; private set; }

    /// <summary>
    /// Gets the mapping of advancement tabs to their milestone advancement Minecraft paths.
    /// </summary>
    [PublicAPI]
    public IReadOnlyDictionary<BacapAdvancementTab, string>? MilestoneMcPaths { get; private set; }

    /// <summary>
    /// Gets the Minecraft resource path for the advancement legend reward.
    /// </summary>
    [PublicAPI]
    public string? AdvancementLegendMcPath { get; private set; }

    /// <summary>
    /// Applies domain-specific presets and hardcoded constants based on the datapack identifier.
    /// </summary>
    /// <param name="id">The strongly-typed datapack identifier.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when an unsupported <see cref="DatapackId"/> is provided.</exception>
    /// <example>
    /// <code>
    /// var settings = new DatapackSettings { Path = "./datapacks/bacaped_datapack" };
    /// settings.ApplyPreset(DatapackId.Bacaped);
    /// </code>
    /// </example>
    public void ApplyPreset(DatapackId id)
    {
        switch (id)
        {
            case DatapackId.Bacap:
                MainNamespace = "blazeandcave";
                RewardNamespace = "bacap_rewards";
                Access = DatapackAccess.ReadOnly;
                AdvancementMessageSettings = null;
                ParentDatapackId = null;
                MilestoneMcPaths = null;
                AdvancementLegendMcPath = null;
                break;

            case DatapackId.Bacaped:
                MainNamespace = "bacaped";
                RewardNamespace = "bacaped_rewards";
                Access = DatapackAccess.ReadWrite;
                AdvancementMessageSettings = new AdvancementMessageSettings
                {
                    Entries = DatapackDefaults.DefaultTierMessages
                };
                ParentDatapackId = null;
                MilestoneMcPaths = DatapackDefaults.CreateDefaultTypedMilestones();
                AdvancementLegendMcPath = "bacaped:bacap/enhanced_legend";
                break;

            case DatapackId.BacapedHardcore:
                MainNamespace = "bacaped";
                RewardNamespace = "bacaped_rewards";
                Access = DatapackAccess.ReadWrite;
                AdvancementMessageSettings = new AdvancementMessageSettings
                {
                    Entries = DatapackDefaults.DefaultTierMessages
                };
                ParentDatapackId = DatapackId.Bacaped;
                MilestoneMcPaths = DatapackDefaults.CreateDefaultTypedMilestones();
                AdvancementLegendMcPath = "bacaped:bacap/enhanced_legend";
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(id), id, "Unsupported datapack identifier.");
        }
    }

    /// <summary>
    /// Validates the current settings object based on the configured access mode and paths.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if validation constraints are violated.</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Path))
            throw new InvalidOperationException($"'{nameof(Path)}' must be provided for the datapack.");

        if (Access != DatapackAccess.ReadWrite)
            return;

        if (string.IsNullOrWhiteSpace(LanguagePackPath))
            throw new InvalidOperationException($"'{nameof(LanguagePackPath)}' must be provided when Access is set to ReadWrite.");

        if (AdvancementMessageSettings is null)
            throw new InvalidOperationException($"'{nameof(AdvancementMessageSettings)}' must be provided when Access is set to ReadWrite.");
    }
}