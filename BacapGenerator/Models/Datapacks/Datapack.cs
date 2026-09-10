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
    /// <summary>
    /// Gets the strongly-typed identifier of the datapack.
    /// </summary>
    /// <summary>
    /// Gets the string identifier of the datapack, loaded from the configuration.
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    /// Gets the settings and configuration associated with this datapack.
    /// </summary>
    public DatapackSettings Settings { get; init; }

    /// <summary>
    /// Gets the <see cref="DirectoryInfo"/> representing the data folder of the datapack.
    /// </summary>
    public DirectoryInfo DatapackDataPath { get;  init;}

    /// <summary>
    /// Gets the display name for distribution releases.
    /// Returns the explicitly configured <see cref="DatapackSettings.ReleaseName"/>,
    /// or falls back to a predefined value based on <see cref="Id"/>.
    /// </summary>
    public string ReleaseName => !string.IsNullOrWhiteSpace(Settings.ReleaseName)
        ? Settings.ReleaseName
        : $"{Id} Datapack";

    /// <summary>
    /// Gets the global Minecraft registry data.
    /// </summary>
    public MinecraftData MinecraftData { get;  init;}

    /// <summary>
    /// Gets the mutable collection of advancements belonging to this datapack.
    /// </summary>
    public List<ManagedAdvancement> Advancements { get; } = [];

    /// <inheritdoc cref="IReadOnlyDatapack.Advancements"/>
    IReadOnlyList<ManagedAdvancement> IReadOnlyDatapack.Advancements => Advancements.AsReadOnly();

    /// <summary>
    /// Initializes a new instance of the <see cref="Datapack"/> model.
    /// Does not perform file I/O operations.
    /// </summary>
    /// <param name="id">The strongly-typed identifier of the datapack.</param>
    /// <param name="settings">The datapack configuration.</param>
    /// <param name="minecraftData">The global Minecraft registry data.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="settings"/> or <paramref name="minecraftData"/> is null.</exception>
    public Datapack(string id, DatapackSettings settings, MinecraftData minecraftData)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(minecraftData);

        Id = id;
        Settings = settings;
        MinecraftData = minecraftData;
        DatapackDataPath = new DirectoryInfo(Path.Combine(Settings.Path, "data"));
    }

    /// <summary>
    /// Replaces an existing managed advancement with an updated or promoted instance.
    /// </summary>
    /// <param name="oldAdvancement">The current advancement instance to be replaced.</param>
    /// <param name="newAdvancement">The new advancement instance.</param>
    /// <returns><see langword="true"/> if the item was found and replaced; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
    [PublicAPI]
    public bool ReplaceAdvancement(ManagedAdvancement oldAdvancement, ManagedAdvancement newAdvancement)
    {
        ArgumentNullException.ThrowIfNull(oldAdvancement);
        ArgumentNullException.ThrowIfNull(newAdvancement);

        var index = Advancements.IndexOf(oldAdvancement);
        if (index < 0)
            return false;

        Advancements[index] = newAdvancement;
        return true;
    }

    /// <summary>
    /// Populates the datapack with loaded advancements.
    /// </summary>
    /// <param name="parsedAdvancements">The collection of advancements to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="parsedAdvancements"/> is null.</exception>
    internal void InitializeAdvancements(IEnumerable<ManagedAdvancement> parsedAdvancements)
    {
        ArgumentNullException.ThrowIfNull(parsedAdvancements);
        Advancements.AddRange(parsedAdvancements);
    }
}