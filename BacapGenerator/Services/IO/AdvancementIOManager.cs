using BacapGenerator.Models.Advancements;
using BacapGenerator.Models.Advancements.Functions;

namespace BacapGenerator.Services.IO;

/// <summary>
/// Service responsible for handling all file system operations related to advancements.
/// </summary>
public static class AdvancementIoManager
{
    public static void SaveAdvancement(BacapAdvancement advancement)
    {
        advancement.EnsureMutable();

        advancement.Sync();

        // Check if the main JSON file path changed. If so, delete the old one.
        if (advancement.OriginalFile.FullName != advancement.File.FullName)
        {
            DeleteIfExists(advancement.OriginalFile);
            advancement.OriginalFile = advancement.File; // Update pointer after successful move
        }

        // Save JSON
        advancement.File.Directory?.Create();
        File.WriteAllText(advancement.File.FullName, advancement.Advancement.ToJson());

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
            WriteFunctionSafely(advancement.ExpRewardFunction);
            WriteFunctionSafely(advancement.ItemRewardFunction);
            WriteFunctionSafely(advancement.TrophyRewardFunction);
        }
    }

    /// <summary>
    /// Deletes the given advancement and all its functions from the disk.
    /// </summary>
    /// <param name="advancement">The advancement model to delete.</param>
    public static void DeleteAdvancement(BacapAdvancement advancement)
    {
        advancement.EnsureMutable();

        // Use OriginalFile to guarantee we delete what's actually on the disk,
        // just in case the paths were changed in memory before calling delete.
        DeleteIfExists(advancement.OriginalFile);

        DeleteIfExists(advancement.MacroFunction.OriginalFile);
        DeleteIfExists(advancement.MsgFunction.OriginalFile);
        DeleteIfExists(advancement.ExpRewardFunction.OriginalFile);
        DeleteIfExists(advancement.ItemRewardFunction.OriginalFile);
        DeleteIfExists(advancement.TrophyRewardFunction.OriginalFile);
    }

    /// <summary>
    /// Writes the function to disk, deleting the old file if the path has changed.
    /// </summary>
    private static void WriteFunctionSafely(BaseFunction? function)
    {
        if (function == null) return;

        // Clean up the old function file if the path was changed
        if (function.OriginalFile.FullName != function.File.FullName)
        {
            DeleteIfExists(function.OriginalFile);
            function.OriginalFile = function.File; // Update pointer
        }

        function.File.Directory?.Create();
        File.WriteAllText(function.File.FullName, function.Function.Build());
    }

    /// <summary>
    /// Safely deletes a file if it exists.
    /// </summary>
    private static void DeleteIfExists(FileInfo? fileInfo)
    {
        if (fileInfo is { Exists: true })
        {
            fileInfo.Delete();
        }
    }
}