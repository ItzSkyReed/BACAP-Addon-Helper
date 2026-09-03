using System.Text.Json;
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
        // 1. Запись самой функции
        functionFile.Directory?.Create();
        File.WriteAllText(functionFile.FullName, function.Build());

        // Запись тега (аналог cls._update_function_tag из питона)
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
}