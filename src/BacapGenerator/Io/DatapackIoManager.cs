using System.IO.Compression;
using System.Text.Json;
using BacapGenerator.Datapacks.Models;
using Core.McFunctions.Models;
using Core.Serialization;

namespace BacapGenerator.Io;

/// <summary>
/// Service responsible for handling file system operations for global datapack files (functions and tags).
/// </summary>
public static class DatapackIoManager
{
    /// <summary>
    /// Writes a Minecraft function instance to disk.
    /// </summary>
    /// <param name="function">The function model to serialize.</param>
    /// <param name="file">The target file path info.</param>
    public static void WriteFunction(McFunction function, FileInfo file)
    {
        ArgumentNullException.ThrowIfNull(function);
        ArgumentNullException.ThrowIfNull(file);

        file.Directory?.Create();
        File.WriteAllLines(file.FullName, function.Lines.Select(line => line.Build()));
    }

    /// <summary>
    /// Writes a Minecraft function tag JSON file with multiple function entries.
    /// </summary>
    /// <param name="file">The target JSON file path info.</param>
    /// <param name="values">The collection of function resource identifiers.</param>
    public static void WriteTag(FileInfo file, IReadOnlyList<string> values)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentNullException.ThrowIfNull(values);

        file.Directory?.Create();

        var tagModel = new
        {
            replace = false,
            values
        };

        var json = JsonSerializer.Serialize(tagModel, MinecraftDatapackJsonOptions.DatapackIoManager);
        File.WriteAllText(file.FullName, json);
    }


    /// <summary>
    /// Creates a zip archive from the specified datapack directory.
    /// </summary>
    /// <param name="datapack">Datapack to release.</param>
    /// <param name="version">The version string to append to the filename.</param>
    /// <param name="outputDirectory">The directory to output the file</param>
    /// <returns>A task representing the asynchronous compression process.</returns>
    public static async Task ArchiveDatapackAsync(IReadOnlyDatapack datapack, string version, string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);

        var zipPath = Path.Combine(outputDirectory, $"{datapack.Settings.ReleaseName} {version}.zip");

        if (File.Exists(zipPath))
            File.Delete(zipPath);

        await Task.Run(() => ZipFile.CreateFromDirectory(datapack.Settings.Path, zipPath, CompressionLevel.Optimal, includeBaseDirectory: false));
    }
}