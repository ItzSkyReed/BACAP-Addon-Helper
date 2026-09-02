using BacapGenerator.Models.Datapacks.Settings;
using BacapGenerator.Models.Interfaces;
using BacapGenerator.Utils;
using Core.Advancements.Models;
using JetBrains.Annotations;

namespace BacapGenerator.Models.Advancements;

/// <summary>
/// Base class for all loaded advancements in the workspace.
/// Contains purely in-memory state.
/// </summary>
public abstract class ManagedAdvancement
{
    private Advancement? _advancement;
    private FileInfo _file;

    public IReadOnlyDatapack Datapack { get; }

    /// <summary>
    /// Gets the precalculated macro command name.
    /// Computed once on first access and cached.
    /// </summary>
    public string DatapackRelativePath => field ??= Path.GetRelativePath(Datapack.DatapackDataPath.ToString(), _file.FullName);

    /// <summary>
    /// Gets or sets the Minecraft path of the advancement.
    /// </summary>
    /// <remarks>
    /// Setting this property automatically updates the associated physical <see cref="File"/>.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when attempting to modify a read-only instance.</exception>
    /// <example>
    /// <code>
    /// advancement.McPath = "bacap:adventure/root";
    /// </code>
    /// </example>
    [PublicAPI]
    public virtual string McPath
    {
        get => field ??= MinecraftUtils.ToMinecraftPath(DatapackRelativePath);
        set
        {
            EnsureMutable();
            if (field == value)
                return;

            field = value;

            // Resolve the new physical path based on the new McPath
            var physicalPath = MinecraftUtils.ResolvePhysicalPath(Datapack.DatapackDataPath.ToString(), "advancement", value);
            File = new FileInfo(physicalPath);
        }
    }
    /// <summary>
    /// Indicates whether the advancement is read-only (e.g., loaded from a ReadOnly datapack).
    /// </summary>
    public bool IsReadOnly => Datapack.Settings.Access == DatapackAccess.ReadOnly;

    /// <summary>
    /// Gets or sets a value indicating whether this advancement overrides an existing one
    /// from a parent compatibility datapack. If true, core rewards (macros, XP) should not be generated.
    /// </summary>
    [PublicAPI]
    public bool IsOverride { get; set; }

    /// <summary>
    /// Gets the original file path before any unsaved modifications.
    /// Used by the IO Manager to clean up old files when paths are changed.
    /// </summary>
    public FileInfo OriginalFile { get; internal set; }

    /// <summary>
    /// Gets or sets the current intended file path.
    /// Changing this does not affect the file system until the model is explicitly saved.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when setting a null value.</exception>
    /// <exception cref="InvalidOperationException">Thrown when attempting to modify a read-only instance.</exception>
    [PublicAPI] 
    public FileInfo File
    {
        get => _file;
        set
        {
            EnsureMutable();
            ArgumentNullException.ThrowIfNull(value);
            _file = value;
        }
    }

    /// <summary>
    /// Gets or sets the parsed Core advancement data model.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when setting a null value.</exception>
    /// <exception cref="InvalidOperationException">Thrown when attempting to modify a read-only instance.</exception>
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

    protected ManagedAdvancement(FileInfo file, Advancement? advancement, IReadOnlyDatapack datapack)
    {
        ArgumentNullException.ThrowIfNull(file);

        _file = file;
        OriginalFile = file;
        _advancement = advancement;
        Datapack = datapack;
    }


    /// <summary>
    /// Checks if the object is mutable, throwing an exception if it's frozen.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when attempting to modify a read-only instance.</exception>
    public void EnsureMutable()
    {
        if (Datapack.Settings.Access == DatapackAccess.ReadOnly)
            throw new InvalidOperationException($"Cannot modify {GetType().Name} because its parent datapack is in {DatapackAccess.ReadOnly} mode.");
    }

    public override string ToString() => $"{GetType().Name}({File.Name})";
}