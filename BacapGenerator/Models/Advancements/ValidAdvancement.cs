using BacapGenerator.Models.Interfaces;
using Core.Advancements.Models;
using JetBrains.Annotations;

namespace BacapGenerator.Models.Advancements;

/// <summary>
/// Represents a syntactically valid advancement backed by a fully parsed in-memory <see cref="Advancement"/> model.
/// </summary>
public abstract class ValidAdvancement : ManagedAdvancement
{
    private Advancement _advancement;

    /// <summary>
    /// Gets or sets the parsed Minecraft advancement data model.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when setting a <see langword="null"/> value.</exception>
    /// <exception cref="InvalidOperationException">Thrown when attempting to modify a read-only instance.</exception>
    [PublicAPI]
    public virtual Advancement Advancement
    {
        get => _advancement;
        set
        {
            EnsureMutable();
            ArgumentNullException.ThrowIfNull(value);
            _advancement = value;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidAdvancement"/> class.
    /// </summary>
    /// <param name="file">The physical file information.</param>
    /// <param name="advancement">The non-null parsed advancement model.</param>
    /// <param name="datapack">The owning datapack.</param>
    /// <exception cref="ArgumentNullException">Thrown when any argument is <see langword="null"/>.</exception>
    protected ValidAdvancement(FileInfo file, Advancement advancement, IReadOnlyDatapack datapack)
        : base(file, datapack)
    {
        ArgumentNullException.ThrowIfNull(advancement);
        _advancement = advancement;
    }
}