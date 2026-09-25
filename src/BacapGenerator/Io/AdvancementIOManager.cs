using BacapGenerator.Advancements.Functions;
using BacapGenerator.Advancements.Models;
using BacapGenerator.Datapacks.Models.Settings;
using JetBrains.Annotations;

namespace BacapGenerator.Io;

/// <summary>
/// Service responsible for handling all file system operations related to advancements and their reward functions.
/// Centralizes persistence policies based on datapack modes and compatibility overrides.
/// </summary>
public static class AdvancementIoManager
{
    /// <summary>
    /// Persists advancement JSON to disk. If the target is a <see cref="BacapAdvancement"/>,
    /// synchronizes attached function files according to datapack configuration rules.
    /// </summary>
    /// <param name="advancement">The target valid advancement model to persist.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="advancement"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when attempting to save an advancement belonging to a Reference (read-only) datapack.
    /// </exception>
    /// <example>
    /// <code>
    /// // Persists advancement JSON (and attached functions if BACAP)
    /// AdvancementIoManager.SaveAdvancement(validAdvancement);
    /// </code>
    /// </example>
    [PublicAPI]
    public static void SaveAdvancement(ValidAdvancement advancement)
    {
        ArgumentNullException.ThrowIfNull(advancement);

        advancement.EnsureMutable();

        if (advancement.Datapack.Settings.DatapackType == DatapackType.Reference)
            throw new InvalidOperationException(
                $"Cannot persist advancement '{advancement.McPath}' because datapack is configured as Reference (read-only).");

        // If it's a BacapAdvancement, ensure in-memory function ASTs are synchronized before disk write
        if (advancement is BacapAdvancement bacap)
            bacap.Sync();

        // Persist Advancement JSON (clean up old file if McPath/filename changed)
        SaveAdvancementJson(advancement);

        // Persist and synchronize attached functions if this is a playable BACAP advancement
        if (advancement is BacapAdvancement bacapAdv)
            SynchronizeBacapFunctions(bacapAdv);
    }

    /// <summary>
    /// Deletes the given advancement JSON and all its associated function files from disk.
    /// </summary>
    /// <param name="advancement">The advancement model to delete.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="advancement"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when attempting to delete an advancement belonging to a Reference (read-only) datapack.
    /// </exception>
    /// <example>
    /// <code>
    /// AdvancementIoManager.DeleteAdvancement(advancement);
    /// </code>
    /// </example>
    [PublicAPI]
    public static void DeleteAdvancement(ValidAdvancement advancement)
    {
        ArgumentNullException.ThrowIfNull(advancement);

        advancement.EnsureMutable();

        // Delete main JSON file (both current and original if path was mutated in memory)
        DeleteIfExists(advancement.OriginalFile);
        DeleteIfExists(advancement.File);

        // Delete all associated reward functions if this is a BACAP advancement
        if (advancement is not BacapAdvancement bacap)
            return;

        DeleteIfExists(bacap.MacroFunction.OriginalFile);
        DeleteIfExists(bacap.MacroFunction.File);

        DeleteIfExists(bacap.MsgFunction.OriginalFile);
        DeleteIfExists(bacap.MsgFunction.File);

        DeleteIfExists(bacap.ExpRewardFunction.OriginalFile);
        DeleteIfExists(bacap.ExpRewardFunction.File);

        DeleteIfExists(bacap.ItemRewardFunction.OriginalFile);
        DeleteIfExists(bacap.ItemRewardFunction.File);

        DeleteIfExists(bacap.TrophyRewardFunction.OriginalFile);
        DeleteIfExists(bacap.TrophyRewardFunction.File);
    }

    /// <summary>
    /// Explicitly creates or updates a specific reward function on disk and synchronizes the parent advancement.
    /// </summary>
    /// <param name="advancement">The parent BACAP advancement containing the reward function.</param>
    /// <param name="function">The reward function instance to persist.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="advancement"/> is <see langword="null"/>.</exception>
    [PublicAPI]
    public static void SaveRewardFunction(BacapAdvancement advancement, BaseFunction? function)
    {
        ArgumentNullException.ThrowIfNull(advancement);

        if (function is null)
            return;

        WriteFunctionSafely(function);
        SaveAdvancement(advancement);
    }

    /// <summary>
    /// Deletes a specific reward function file from disk and updates the parent advancement.
    /// </summary>
    /// <param name="advancement">The parent BACAP advancement.</param>
    /// <param name="function">The reward function to remove from disk.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="advancement"/> is <see langword="null"/>.</exception>
    [PublicAPI]
    public static void DeleteRewardFunction(BacapAdvancement advancement, BaseFunction? function)
    {
        ArgumentNullException.ThrowIfNull(advancement);

        if (function is null)
            return;

        DeleteIfExists(function.OriginalFile);
        DeleteIfExists(function.File);

        SaveAdvancement(advancement);
    }

    /// <summary>
    /// Handles physical file creation, movement, and JSON writing for an advancement.
    /// </summary>
    /// <param name="advancement">The advancement whose JSON is being persisted.</param>
    private static void SaveAdvancementJson(ValidAdvancement advancement)
    {
        if (!string.Equals(advancement.OriginalFile.FullName, advancement.File.FullName, StringComparison.OrdinalIgnoreCase))
        {
            DeleteIfExists(advancement.OriginalFile);
            advancement.OriginalFile = advancement.File;
        }

        advancement.File.Directory?.Create();
        File.WriteAllText(advancement.File.FullName, advancement.Advancement.ToJson());
        advancement.File.Refresh();
    }

    /// <summary>
    /// Synchronizes all five BACAP function files according to datapack capabilities and override rules.
    /// </summary>
    /// <param name="advancement">The BACAP advancement owning the functions.</param>
    private static void SynchronizeBacapFunctions(BacapAdvancement advancement)
    {
        var settings = advancement.Datapack.Settings;
        var isCompatibility = settings.DatapackType == DatapackType.CompatibilityAddon || advancement.IsOverride;

        var allowMsg = !isCompatibility || settings.CompatibilityAddonSettings.OverrideMsg;
        var allowMacro = !isCompatibility;
        var allowExp = !isCompatibility || settings.CompatibilityAddonSettings.OverrideExpRewards;
        var allowItems = !isCompatibility || settings.CompatibilityAddonSettings.OverrideItemRewards;
        var allowTrophies = !isCompatibility || settings.CompatibilityAddonSettings.OverrideTrophyRewards;

        // Core execution and announcement functions
        SynchronizeFunction(advancement.MsgFunction, isEnabled: allowMsg, requireExistingOnDisk: false);
        SynchronizeFunction(advancement.MacroFunction, isEnabled: allowMacro, requireExistingOnDisk: false);

        // Optional reward functions (only persisted if explicitly configured/existing on disk)
        SynchronizeFunction(advancement.ExpRewardFunction, isEnabled: allowExp, requireExistingOnDisk: true);
        SynchronizeFunction(advancement.ItemRewardFunction, isEnabled: allowItems, requireExistingOnDisk: true);
        SynchronizeFunction(advancement.TrophyRewardFunction, isEnabled: allowTrophies, requireExistingOnDisk: true);
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
            WriteRewardFunctionIfExists(function);
        else
            WriteFunctionSafely(function);
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

        if (!string.Equals(function.OriginalFile.FullName, function.File.FullName, StringComparison.OrdinalIgnoreCase))
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