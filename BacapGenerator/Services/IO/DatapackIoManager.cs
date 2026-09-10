using System.IO.Compression;
using System.Text.Json;
using BacapGenerator.Models.Interfaces;
using Core.McFunctions.Models;
using Core.Serialization;

namespace BacapGenerator.Services.IO;

/// <summary>
/// Service responsible for handling file system operations for global datapack files (functions and tags).
/// </summary>
public static class DatapackIoManager
{
    /// <summary>
    /// Writes a generated function to the disk and optionally creates a function tag for it.
    /// </summary>
    /// <param name="function">The AST model of the function.</param>
    /// <param name="functionFile">The target FileInfo where the .mcfunction will be saved.</param>
    /// <param name="tagFile">Optional. The target FileInfo where the .json tag will be saved.</param>
    /// <param name="functionCallPath">The namespace path to put inside the tag (e.g. "namespace:update_score").</param>
    public static void WriteFunctionAndTag(
        McFunction function,
        FileInfo functionFile,
        FileInfo? tagFile = null,
        string? functionCallPath = null)
    {
        functionFile.Directory?.Create();
        File.WriteAllText(functionFile.FullName, function.Build());

        if (tagFile == null || string.IsNullOrWhiteSpace(functionCallPath))
            return;
        tagFile.Directory?.Create();

        var tagContent = new
        {
            replace = false,
            values = new[] { functionCallPath }
        };

        var jsonString = JsonSerializer.Serialize(tagContent, MinecraftDatapackJsonOptions.Default);

        File.WriteAllText(tagFile.FullName, jsonString);
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