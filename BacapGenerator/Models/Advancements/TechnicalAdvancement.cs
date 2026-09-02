using BacapGenerator.Models.Interfaces;
using Core.Advancements.Models;

namespace BacapGenerator.Models.Advancements;

/// <summary>
/// Represents a technical advancement used only as a trigger (no display data).
/// </summary>
public class TechnicalAdvancement : ManagedAdvancement
{
    public TechnicalAdvancement(FileInfo file, Advancement advancement, IReadOnlyDatapack datapack) : base(file, advancement, datapack)
    {
        ArgumentNullException.ThrowIfNull(advancement);
    }
}