using System;
using System.Diagnostics;
using BacapGenerator.Factories;
using BacapGenerator.Models.Datapacks.Settings;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace BacapGenerator.Services;

/// <summary>
/// The main application service that drives the datapack generation process.
/// </summary>
public class GeneratorAppService(
    IOptions<List<DatapackSettings>> options,
    IDatapackFactory datapackFactory)
{
    private readonly List<DatapackSettings> _datapackConfigs = options.Value;

    /// <summary>
    /// Executes the main logic for loading and processing datapacks.
    /// </summary>
    public void Run()
    {
        Console.WriteLine($"Found {_datapackConfigs.Count} datapacks to process.");

        foreach (var config in _datapackConfigs)
        {
            try
            {
                Console.WriteLine($"\nProcessing datapack at: {config.DatapackPath} (Mode: {config.Access})");

                // Start the high-resolution timer
                var sw = Stopwatch.StartNew();

                // The factory validates settings, injects MinecraftData, and parses all files
                var datapack = datapackFactory.Create(config);

                // Stop the timer
                sw.Stop();

                Console.WriteLine($"Successfully loaded {datapack.Advancements.Count} advancements!");
                Console.WriteLine($"Elapsed time: {sw.ElapsedMilliseconds} ms ({sw.Elapsed.TotalSeconds:F2} seconds)");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error processing datapack config:\n{ex}");
                Console.ResetColor();
            }
        }
    }
}