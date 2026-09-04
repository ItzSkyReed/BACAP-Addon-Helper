using BacapGenerator.Models.Advancements;
using BacapGenerator.Models.Datapacks.Settings;
using Core.Registries;

namespace BacapGenerator.Models.Interfaces;

public interface IReadOnlyDatapack
{
    /// <summary>
    /// Gets the unique identifier of the datapack as defined in the configuration.
    /// </summary>
    public DatapackId Id { get; }

    public IReadOnlyList<ManagedAdvancement> Advancements { get; }

    DatapackSettings Settings { get; }

    public DirectoryInfo DatapackDataPath { get; }

    public MinecraftData MinecraftData { get; }
}