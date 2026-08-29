using Core.Advancements.Models;
using JetBrains.Annotations;

namespace BacapGenerator.Models.Advancements;

/// <summary>
/// Base class for all loaded advancements in the workspace.
/// </summary>
public abstract class ManagedAdvancement
{
    private Advancement _advancement;

    /// <summary>
    /// Indicates whether the advancement is read-only (e.g. loaded from a ReadOnly datapack).
    /// </summary>
    public bool IsReadOnly { get; private set; }


    /// <summary>
    /// Locks the advancement instance, preventing any further property mutations.
    /// </summary>
    public void Freeze() => IsReadOnly = true;

    /// <summary>
    /// Checks if the object is mutable, throwing an exception if it's frozen.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when attempting to modify a read-only instance.</exception>
    public void EnsureMutable()
    {
        if (IsReadOnly)
            throw new InvalidOperationException($"Cannot modify {GetType().Name} because its parent datapack is in ReadOnly mode.");
    }

    // TODO: Setter
    [PublicAPI] public FileInfo File { get; set; }

    /// <summary>
    /// Gets or sets the parsed Core advancement data model.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when setting a null value.</exception>
    [PublicAPI]
    public virtual Advancement? Advancement
    {
        get => _advancement;
        set
        {
            EnsureMutable();
            ArgumentNullException.ThrowIfNull(value);
            _advancement = value;
        }
    }

    protected ManagedAdvancement(FileInfo file, Advancement advancement, bool readOnly = true)
    {
        ArgumentNullException.ThrowIfNull(file);

        File = file;

        IsReadOnly = readOnly;
        _advancement = advancement;
    }

    public override string ToString() => $"{GetType().Name}({File.Name})";
}