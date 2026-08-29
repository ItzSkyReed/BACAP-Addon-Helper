using BacapGenerator.Models.Advancements;

namespace BacapGenerator.Models.Datapacks.Settings;

using System.Collections.Generic;

/// <summary>
/// Settings for generating advancement completion messages.
/// </summary>
public class AdvancementMessageSettings
{
    /// <summary>
    /// A mapping of advancement tiers to their specific message configurations.
    /// O(1) lookup time.
    /// </summary>
    public required IReadOnlyDictionary<BacapAdvancementTier, AdvancementMessageSettingsEntry> Entries { get; init; }
}