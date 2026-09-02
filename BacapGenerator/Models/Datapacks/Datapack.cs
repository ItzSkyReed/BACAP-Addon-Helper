
using BacapGenerator.Models.Advancements;
using BacapGenerator.Models.Datapacks.Settings;
using BacapGenerator.Models.Interfaces;
using Core.Registries;
using JetBrains.Annotations;

namespace BacapGenerator.Models.Datapacks;

/// <summary>
/// Represents a loaded Minecraft Datapack, containing its settings, global registry data,
/// and all parsed advancements.
/// </summary>
public class Datapack : IReadOnlyDatapack
{
    private readonly List<ManagedAdvancement> _advancements = [];

    /// <summary>
    /// Gets the unique identifier of the datapack as defined in the configuration.
    /// </summary>
    public string Id { get; }

    public DatapackSettings Settings { get; }

    public DirectoryInfo DatapackDataPath { get; }

    public MinecraftData MinecraftData { get; }

    public IReadOnlyList<ManagedAdvancement> Advancements => _advancements;

    /// <summary>
    /// Initializes a new instance of the <see cref="Datapack"/> model.
    /// Does not perform any file I/O.
    /// </summary>
    /// <param name="id">The unique identifier from the configuration.</param>
    /// <param name="settings">The datapack configuration.</param>
    /// <param name="minecraftData">The global Minecraft registry data.</param>
    /// <exception cref="ArgumentException">Thrown when the id is null or empty.</exception>
    public Datapack(string id, DatapackSettings settings, MinecraftData minecraftData)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);

        Id = id;
        Settings = settings;
        MinecraftData = minecraftData;
        DatapackDataPath = new DirectoryInfo(Path.Combine(Settings.DatapackPath, "data"));
    }

    /// <summary>
    /// Populates the datapack with loaded advancements.
    /// Marked as internal so only the Factory within the same assembly can populate it.
    /// </summary>
    /// <param name="parsedAdvancements">The collection of advancements to add.</param>
    internal void InitializeAdvancements(IEnumerable<ManagedAdvancement> parsedAdvancements)
    {
        _advancements.AddRange(parsedAdvancements);
    }
}