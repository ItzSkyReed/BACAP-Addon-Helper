using BacapGenerator.Models.Advancements;
using BacapGenerator.Models.Advancements.Functions;

namespace BacapGenerator.Services.IO;

/// <summary>
/// Service responsible for handling all file system operations related to advancements.
/// </summary>
public static class AdvancementIoManager
{
    /// <summary>
    /// Persists advancement JSON and core functions to disk.
    /// Existing reward functions on disk are updated, while unconfigured reward files are left untouched.
    /// </summary>
    /// <param name="advancement">The target BACAP advancement model to persist.</param>
    public static void SaveAdvancement(BacapAdvancement advancement)
    {
        advancement.EnsureMutable();
        advancement.Sync();

        // Check if the main JSON file path changed. If so, delete the old one.
        if (advancement.OriginalFile.FullName != advancement.File.FullName)
        {
            DeleteIfExists(advancement.OriginalFile);
            advancement.OriginalFile = advancement.File;
        }

        // Save JSON definition
        advancement.File.Directory?.Create();
        File.WriteAllText(advancement.File.FullName, advancement.Advancement.ToJson());
        advancement.File.Refresh();

        WriteFunctionSafely(advancement.MsgFunction);

        if (advancement.IsOverride)
        {
            DeleteIfExists(advancement.MacroFunction.OriginalFile);
            DeleteIfExists(advancement.ExpRewardFunction.OriginalFile);
            DeleteIfExists(advancement.ItemRewardFunction.OriginalFile);
            DeleteIfExists(advancement.TrophyRewardFunction.OriginalFile);
        }
        else
        {
            WriteFunctionSafely(advancement.MacroFunction);

            // Synchronize reward functions ONLY if they already exist on the disk
            WriteRewardFunctionIfExists(advancement.ExpRewardFunction);
            WriteRewardFunctionIfExists(advancement.ItemRewardFunction);
            WriteRewardFunctionIfExists(advancement.TrophyRewardFunction);
        }
    }

    /// <summary>
    /// Explicitly creates or updates a specific reward function on disk and synchronizes the parent advancement.
    /// </summary>
    /// <param name="advancement">The parent advancement containing the reward function.</param>
    /// <param name="function">The reward function instance to persist.</param>
    public static void SaveRewardFunction(BacapAdvancement advancement, BaseFunction? function)
    {
        if (function is null)
            return;

        WriteFunctionSafely(function);
        SaveAdvancement(advancement);
    }

    /// <summary>
    /// Deletes the given advancement and all its associated functions from the disk.
    /// </summary>
    /// <param name="advancement">The advancement model to delete.</param>
    public static void DeleteAdvancement(BacapAdvancement advancement)
    {
        advancement.EnsureMutable();

        DeleteIfExists(advancement.OriginalFile);
        DeleteIfExists(advancement.MacroFunction.OriginalFile);
        DeleteIfExists(advancement.MsgFunction.OriginalFile);
        DeleteIfExists(advancement.ExpRewardFunction.OriginalFile);
        DeleteIfExists(advancement.ItemRewardFunction.OriginalFile);
        DeleteIfExists(advancement.TrophyRewardFunction.OriginalFile);
    }

    /// <summary>
    /// Deletes a specific reward function file from disk and updates the parent advancement.
    /// </summary>
    /// <param name="advancement">The parent advancement.</param>
    /// <param name="function">The reward function to remove from disk.</param>
    public static void DeleteRewardFunction(BacapAdvancement advancement, BaseFunction? function)
    {
        if (function is null)
            return;

        DeleteIfExists(function.OriginalFile);
        DeleteIfExists(function.File);

        SaveAdvancement(advancement);
    }

    /// <summary>
    /// Writes or moves a reward function only if it already physically exists on disk.
    /// </summary>
    /// <param name="function">The reward function to inspect and conditionally write.</param>
    private static void WriteRewardFunctionIfExists(BaseFunction? function)
    {
        if (function is null)
            return;

        // If the file exists either at the current or original path, it was intentionally created
        if (File.Exists(function.OriginalFile.FullName) || File.Exists(function.File.FullName))
            WriteFunctionSafely(function);
        else
            // Keep path pointers aligned in memory without creating the file on disk
            function.OriginalFile = function.File;
    }

    /// <summary>
    /// Writes the function to disk, deleting the old file if the path has changed.
    /// </summary>
    /// <param name="function">The function instance to write.</param>
    private static void WriteFunctionSafely(BaseFunction? function)
    {
        if (function is null)
            return;

        // Clean up the old function file if the path was changed
        if (function.OriginalFile.FullName != function.File.FullName)
        {
            DeleteIfExists(function.OriginalFile);
            function.OriginalFile = function.File;
        }

        function.File.Directory?.Create();
        File.WriteAllText(function.File.FullName, function.Function.Build());
        function.File.Refresh();
    }

    /// <summary>
    /// Safely deletes a file if it exists on disk and refreshes its metadata cache.
    /// </summary>
    /// <param name="fileInfo">The file descriptor to delete.</param>
    private static void DeleteIfExists(FileInfo? fileInfo)
    {
        if (fileInfo is null)
        {
            return;
        }

        if (!File.Exists(fileInfo.FullName))
            return;

        fileInfo.Delete();
        fileInfo.Refresh();
    }
}