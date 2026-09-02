using System.Diagnostics;
using BacapGenerator.Factories;
using BacapGenerator.Models.Datapacks;
using BacapGenerator.Models.Datapacks.Settings;

using Microsoft.Extensions.Options;

namespace BacapGenerator.Services;

/// <summary>
/// The main application service that drives the datapack generation process.
/// </summary>
public class DatapackLoaderService(
    IOptions<Dictionary<string, DatapackSettings>> options,
    IDatapackFactory datapackFactory,
    DatapackRegistry datapackRegistry)
{
    private readonly Dictionary<string, DatapackSettings> _datapackConfigs = options.Value;

    /// <summary>
    /// Executes the main logic for loading and processing datapacks.
    /// </summary>
    public void LoadAll()
    {
        Console.WriteLine($"Found {_datapackConfigs.Count} datapacks to process.");

        // Load everything into memory
        foreach (var (id, config) in _datapackConfigs)
        {
            try
            {
                Console.WriteLine($"\nProcessing datapack '{id}' at: {config.DatapackPath} (Mode: {config.Access})");

                var sw = Stopwatch.StartNew();

                var datapack = datapackFactory.Create(id, config);

                // Store the loaded datapack in the global registry
                datapackRegistry.Register(id, datapack);

                sw.Stop();

                Console.WriteLine($"Successfully loaded {datapack.Advancements.Count} advancements!");
                Console.WriteLine($"Elapsed time: {sw.ElapsedMilliseconds} ms ({sw.Elapsed.TotalSeconds:F2} seconds)");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error processing datapack '{id}':\n{ex}");
                Console.ResetColor();
            }
        }

        // Resolve cross-datapack dependencies (overrides)
        Console.WriteLine("\nResolving datapack overrides...");

        foreach (var childPack in datapackRegistry.All.Values)
        {
            if (string.IsNullOrEmpty(childPack.Settings.ParentDatapackId))
                continue;

            if (datapackRegistry.All.TryGetValue(childPack.Settings.ParentDatapackId, out var parentPack))
            {
                DatapackResolver.ResolveOverrides(childPack, parentPack);
                Console.WriteLine($"Successfully linked '{childPack.Id}' as an addon to '{parentPack.Id}'.");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Warning: Parent datapack '{childPack.Settings.ParentDatapackId}' for addon '{childPack.Id}' was not found in the registry.");
                Console.ResetColor();
            }
        }
    }
}