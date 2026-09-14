using System.Diagnostics.CodeAnalysis;
using BacapGenerator.Models.Advancements;
using BacapGenerator.Models.Datapacks.Settings;
using Core.Registries;

namespace BacapGenerator.Models.Interfaces;

public interface IReadOnlyDatapack
{
    /// <summary>
    /// Gets the unique identifier of the datapack as defined in the configuration.
    /// </summary>
    public string Id { get; }

    public IReadOnlyList<ManagedAdvancement> Advancements { get; }

    public IReadOnlyDictionary<string, ManagedAdvancement> AdvancementsById { get; }
    DatapackSettings Settings { get; }

    public DirectoryInfo DatapackDataPath { get; }

    public MinecraftData MinecraftData { get; }

    public bool TryGetAdvancement(string mcPath, [NotNullWhen(true)] out ManagedAdvancement? advancement);

}