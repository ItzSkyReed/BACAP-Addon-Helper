using BacapGenerator.Advancements;
using BacapGenerator.Datapacks.Models;
using BacapGenerator.Datapacks.Models.Settings;
using Core.Registries;

namespace BacapGenerator.Datapacks;

public interface IDatapackFactory
{
    /// <summary>
    /// Validates settings, loads advancement files from disk, and constructs a populated <see cref="Datapack"/>.
    /// </summary>
    /// <param name="id">The strongly-typed datapack identifier.</param>
    /// <param name="settings">The preconfigured datapack settings.</param>
    /// <returns>A fully initialized <see cref="Datapack"/> instance containing parsed advancements.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="settings"/> is null.</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown if the data directory does not exist on disk.</exception>
    Datapack Create(string id, DatapackSettings settings);
}

/// <summary>
/// Factory responsible for reading advancement JSON files from disk and assembling fully initialized <see cref="Datapack"/> instances.
/// </summary>
/// <param name="minecraftData">Global Minecraft registry data.</param>
public class DatapackFactory(MinecraftData minecraftData) : IDatapackFactory
{
    /// <inheritdoc/>
    public Datapack Create(string id, DatapackSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var datapack = new Datapack(id, settings, minecraftData);

        if (!datapack.DatapackDataPath.Exists)
            throw new DirectoryNotFoundException($"Directory '{datapack.DatapackDataPath.FullName}' does not exist.");

        var jsonFiles = datapack.DatapackDataPath.EnumerateDirectories()
            .Select(namespaceDir => Path.Combine(namespaceDir.FullName, "advancement"))
            .Where(Directory.Exists)
            .SelectMany(advDir => Directory.EnumerateFiles(advDir, "*.json", SearchOption.AllDirectories));

        var parsedAdvancements = jsonFiles
            .AsParallel()
            .Select(filePath =>
            {
                var fileInfo = new FileInfo(filePath);
                var jsonContent = File.ReadAllText(filePath);
                return AdvancementFactory.Create(fileInfo, jsonContent, datapack);
            });

        datapack.InitializeAdvancements(parsedAdvancements);

        return datapack;
    }
}