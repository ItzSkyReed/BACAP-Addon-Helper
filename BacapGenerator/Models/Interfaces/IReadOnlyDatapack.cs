using BacapGenerator.Models.Datapacks.Settings;
using Core.Registries;

namespace BacapGenerator.Models.Interfaces;

public interface IReadOnlyDatapack
{
    DatapackSettings Settings { get; }

    public DirectoryInfo DatapackDataPath { get;  }

    public MinecraftData MinecraftData { get; }
}