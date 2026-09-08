using BacapGenerator.Models.Advancements;
using BacapGenerator.Models.Advancements.Functions;
using BacapGenerator.Models.Datapacks.Settings;

namespace BacapGenerator.Services.IO;

/// <summary>
/// Service responsible for handling all file system operations related to advancements and their reward functions.
/// Centralizes persistence policies based on datapack modes and compatibility overrides.
/// </summary>
public static class AdvancementIoManager
{
    /// <summary>
    /// Persists advancement JSON and its associated functions to disk according to datapack configuration rules.
    /// Unconfigured reward files are left untouched, while disabled overrides are purged from disk.
    /// </summary>
    /// <param name="advancement">The target BACAP advancement model to persist.</param>
    /// <exception cref="InvalidOperationException">Thrown when attempting to save an advancement from a Reference (read-only) datapack.</exception>
    /// <example>
    /// <code>
    /// AdvancementIoManager.SaveAdvancement(advancement);
    /// // Or with explicit settings:
    /// AdvancementIoManager.SaveAdvancement(advancement, datapack.Settings);
    /// </code>
    /// </example>
    public static void SaveAdvancement(BacapAdvancement advancement)
    {
        advancement.EnsureMutable();

        var effectiveSettings = advancement.Datapack.Settings;
        if (effectiveSettings.Type == DatapackType.Reference)
        {
            throw new InvalidOperationException(
                $"Cannot persist advancement '{advancement.McPath}' because datapack is configured as Reference (read-only).");
        }

        advancement.Sync();

        // Persist Advancement JSON (clean up old file if path changed)
        if (advancement.OriginalFile.FullName != advancement.File.FullName)
        {
            DeleteIfExists(advancement.OriginalFile);
            advancement.OriginalFile = advancement.File;
        }

        advancement.File.Directory?.Create();
        File.WriteAllText(advancement.File.FullName, advancement.Advancement.ToJson());
        advancement.File.Refresh();

        // Resolve synchronization rules for functions
        var isCompatibility = effectiveSettings.Type == DatapackType.CompatibilityAddon || advancement.IsOverride;
        var compatSettings = effectiveSettings.CompatibilityAddonSettings;

        var allowMsg = !isCompatibility || (compatSettings?.OverrideMsg ?? true);
        var allowMacro = !isCompatibility;
        var allowExp = !isCompatibility || (compatSettings?.OverrideExpRewards ?? false);
        var allowItems = !isCompatibility || (compatSettings?.OverrideItemRewards ?? false);
        var allowTrophies = !isCompatibility || (compatSettings?.OverrideTrophyRewards ?? false);

        // Synchronize core functions
        SynchronizeFunction(advancement.MsgFunction, isEnabled: allowMsg, requireExistingOnDisk: false);
        SynchronizeFunction(advancement.MacroFunction, isEnabled: allowMacro, requireExistingOnDisk: false);

        // Synchronize reward functions (only written if configured/existing on disk)
        SynchronizeFunction(advancement.ExpRewardFunction, isEnabled: allowExp, requireExistingOnDisk: true);
        SynchronizeFunction(advancement.ItemRewardFunction, isEnabled: allowItems, requireExistingOnDisk: true);
        SynchronizeFunction(advancement.TrophyRewardFunction, isEnabled: allowTrophies, requireExistingOnDisk: true);
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
    /// Deletes the given advancement and all its associated functions from disk.
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
    /// Conditionally writes or deletes a function file based on whether the feature is enabled for the datapack.
    /// </summary>
    /// <param name="function">The function to synchronize.</param>
    /// <param name="isEnabled">Whether this function category is active in datapack settings.</param>
    /// <param name="requireExistingOnDisk">
    /// If <see langword="true"/>, the function will only be written if it already physically exists on disk.
    /// </param>
    private static void SynchronizeFunction(BaseFunction? function, bool isEnabled, bool requireExistingOnDisk)
    {
        if (function is null)
            return;

        if (!isEnabled)
        {
            DeleteIfExists(function.OriginalFile);
            DeleteIfExists(function.File);
            return;
        }

        if (requireExistingOnDisk)
        {
            WriteRewardFunctionIfExists(function);
        }
        else
        {
            WriteFunctionSafely(function);
        }
    }

    /// <summary>
    /// Writes or moves a reward function only if it already physically exists on disk.
    /// </summary>
    /// <param name="function">The reward function to inspect and conditionally write.</param>
    private static void WriteRewardFunctionIfExists(BaseFunction? function)
    {
        if (function is null)
            return;

        // If the file exists either at current or original path, it was intentionally created
        if (File.Exists(function.OriginalFile.FullName) || File.Exists(function.File.FullName))
            WriteFunctionSafely(function);
        else
            // Keep path pointers aligned in memory without creating an empty dummy file
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
        if (fileInfo is null || !File.Exists(fileInfo.FullName))
            return;

        fileInfo.Delete();
        fileInfo.Refresh();
    }
}