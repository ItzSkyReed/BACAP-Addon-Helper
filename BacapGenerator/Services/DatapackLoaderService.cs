using System.Diagnostics;
using BacapGenerator.Factories;
using BacapGenerator.Models.Datapacks;
using BacapGenerator.Models.Datapacks.Settings;
using Microsoft.Extensions.Options;

namespace BacapGenerator.Services;

/// <summary>
/// The main application service that drives the datapack loading and generation process.
/// </summary>
/// <param name="options">Bound datapack settings mapped by configuration key.</param>
/// <param name="datapackFactory">Factory responsible for constructing populated datapack models.</param>
/// <param name="datapackRegistry">Central registry storing loaded datapack instances.</param>
public class DatapackLoaderService(
    IOptions<Dictionary<string, DatapackSettings>> options,
    IDatapackFactory datapackFactory,
    DatapackRegistry datapackRegistry)
{
    private readonly Dictionary<string, DatapackSettings> _datapackConfigs = options.Value;

    /// <summary>
    /// Executes the main logic for loading, validating, and resolving all configured datapacks.
    /// </summary>
    public void LoadAll()
    {
        Console.WriteLine($"Found {_datapackConfigs.Count} datapacks to process.");

        foreach (var (id, settings) in _datapackConfigs)
        {
            try
            {
                settings.Validate();

                Console.WriteLine($"\nProcessing datapack '{id}' at: {settings.Path} (Mode: {settings.Type})");

                var sw = Stopwatch.StartNew();

                var datapack = datapackFactory.Create(id, settings);
                datapackRegistry.Register(datapack);

                sw.Stop();

                Console.WriteLine($"Successfully loaded {datapack.Advancements.Count} advancements!");
                Console.WriteLine($"Elapsed time: {sw.ElapsedMilliseconds} ms ({sw.Elapsed.TotalSeconds:F2} seconds)");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error processing datapack '{id}':\n{ex}");
                Console.ResetColor();
                throw;
            }
        }

        ResolveOverrides();
    }

    /// <summary>
    /// Resolves cross-datapack parent-child links for addon overrides.
    /// </summary>
    private void ResolveOverrides()
    {
        Console.WriteLine("\nResolving datapack overrides...");

        foreach (var childPack in datapackRegistry.Values)
        {
            if (childPack.Settings.ParentDatapackId is not { } parentId)
                continue;

            if (datapackRegistry.TryGet(parentId, out var parentPack))
            {
                DatapackResolver.ResolveOverrides(childPack, parentPack);
                Console.WriteLine($"Successfully linked '{childPack.Id}' as an addon to '{parentPack.Id}'.");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Warning: Parent datapack '{parentId}' for addon '{childPack.Id}' was not found in the registry.");
                Console.ResetColor();
            }
        }
    }
}