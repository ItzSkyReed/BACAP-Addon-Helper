using BacapGenerator.Datapacks.Models;
using BacapGenerator.Datapacks.Models.Settings;
using BacapGenerator.Utils;
using JetBrains.Annotations;

namespace BacapGenerator.Advancements.Models;

/// <summary>
/// Base class representing a tracked advancement file within the workspace.
/// Encapsulates file system paths, mutability, and datapack association.
/// </summary>
public abstract class ManagedAdvancement
{
    private FileInfo _file;

    /// <summary>
    /// Gets the datapack that owns this advancement file.
    /// </summary>
    public IReadOnlyDatapack Datapack { get; }

    /// <summary>
    /// Gets the relative file path within the datapack data directory.
    /// Computed once on first access and cached.
    /// </summary>
    public string DatapackRelativePath => field ??= Path.GetRelativePath(Datapack.DatapackDataPath.FullName, _file.FullName);

    /// <summary>
    /// Gets or sets the Minecraft identifier path (e.g., <c>namespace:folder/name</c>).
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when modifying a read-only datapack instance.</exception>
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
    /// Gets a value indicating whether this advancement belongs to a reference (immutable) datapack.
    /// </summary>
    public bool IsReadOnly => Datapack.Settings.Type == DatapackType.Reference;

    /// <summary>
    /// Gets or sets a value indicating whether this advancement overrides an existing one from a parent datapack.
    /// </summary>
    [PublicAPI]
    public bool IsOverride { get; set; }

    /// <summary>
    /// Gets the initial file reference before any unsaved mutations took place.
    /// </summary>
    public FileInfo OriginalFile { get; internal set; }

    /// <summary>
    /// Gets or sets the current target file path.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when modifying a read-only instance.</exception>
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
    /// Initializes a new instance of the <see cref="ManagedAdvancement"/> class.
    /// </summary>
    /// <param name="file">The physical advancement file info.</param>
    /// <param name="datapack">The owning datapack.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="file"/> or <paramref name="datapack"/> is <see langword="null"/>.</exception>
    protected ManagedAdvancement(FileInfo file, IReadOnlyDatapack datapack)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentNullException.ThrowIfNull(datapack);

        _file = file;
        OriginalFile = file;
        Datapack = datapack;
    }

    /// <summary>
    /// Verifies that the instance can be modified, throwing an exception if it is frozen.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the parent datapack is in Reference mode.</exception>
    public void EnsureMutable()
    {
        if (Datapack.Settings.Type == DatapackType.Reference)
            throw new InvalidOperationException($"Cannot modify {GetType().Name} because its parent datapack is in {DatapackType.Reference} mode.");
    }

    public override string ToString() => $"{GetType().Name}({File.Name})";
}