using BacapGenerator.Models.Advancements;
using Core.Advancements.Models;

namespace BacapGenerator.Models;

/// <summary>
/// Represents a technical advancement used only as a trigger (no display data).
/// </summary>
/// <param name="File">The physical file information.</param>
/// <param name="Data">The parsed Core JSON model.</param>
public class TechnicalAdvancement : ManagedAdvancement
{
    public TechnicalAdvancement(FileInfo file, Advancement advancement) : base(file, advancement)
    {
        ArgumentNullException.ThrowIfNull(advancement);
    }
}