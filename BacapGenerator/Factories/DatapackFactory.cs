using BacapGenerator.Models.Datapacks;
using BacapGenerator.Models.Datapacks.Settings;
using Core.Registries;

namespace BacapGenerator.Factories;

public interface IDatapackFactory
{
    Datapack Create(DatapackSettings settings);
}

/// <summary>
/// Factory responsible for validating settings, reading file system data,
/// and assembling fully initialized Datapacks.
/// </summary>
public class DatapackFactory(MinecraftData minecraftData) : IDatapackFactory
{
    /// <summary>
    /// Validates the provided settings, reads advancement files from disk,
    /// and returns a fully constructed Datapack.
    /// </summary>
    /// <param name="settings">The settings parsed from configuration.</param>
    /// <returns>A fully initialized Datapack.</returns>
    /// <exception cref="DirectoryNotFoundException">Thrown if the data folder does not exist.</exception>
    public Datapack Create(DatapackSettings settings)
    {
        // Validate inputs
        settings.Validate();

        // Create the empty model
        var datapack = new Datapack(settings, minecraftData);

        // Perform I/O and orchestration
        if (!datapack.DatapackDataPath.Exists)
            throw new DirectoryNotFoundException($"Directory {datapack.DatapackDataPath.FullName} does not exist.");

        // Gather all file paths synchronously
        var jsonFiles = datapack.DatapackDataPath.EnumerateDirectories()
            .Select(namespaceDir => Path.Combine(namespaceDir.FullName, "advancement"))
            .Where(Directory.Exists)
            .SelectMany(advDir => Directory.EnumerateFiles(advDir, "*.json", SearchOption.AllDirectories));

        // Process the files in parallel, passing the datapack reference downwards
        var parsedAdvancements = jsonFiles
            .AsParallel()
            .Select(filePath =>
            {
                var fileInfo = new FileInfo(filePath);
                var jsonContent = File.ReadAllText(filePath);

                // Creates the model safely since the empty Datapack object already exists in memory
                return AdvancementFactory.Create(fileInfo, jsonContent, datapack);
            });

        // Inject the parsed data back into the model
        datapack.InitializeAdvancements(parsedAdvancements);

        return datapack;
    }
}