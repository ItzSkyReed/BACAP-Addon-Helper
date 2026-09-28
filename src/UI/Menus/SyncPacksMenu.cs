using BacapGenerator.Datapacks;
using BacapGenerator.Datapacks.Models;
using BacapGenerator.Datapacks.Models.Settings;
using Spectre.Console;
using UI.Configuration;
using UI.Interfaces;
using UI.Styling;

namespace UI.Menus;

/// <summary>
/// Sub-menu for synchronizing loaded datapacks and language resource packs to local Minecraft installation folders.
/// </summary>
/// <param name="registry">The registry containing loaded datapack instances.</param>
/// <param name="config">The user configuration providing synchronization target paths.</param>
public sealed class SyncPacksMenu(DatapackRegistry registry, UserConfig config) : IMainMenuAction
{
    public string Title => "Sync Packs to Minecraft";

    /// <summary>
    /// Executes the synchronization workflow by verifying destinations, copying base and compatibility datapacks, and copying resource packs.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task ExecuteAsync()
    {
        TuiTheme.RenderHeader("Sync Packs");

        var nonReferencePacks = registry.Values
            .Where(static dp => dp.Settings.DatapackType != DatapackType.Reference)
            .ToArray();

        var basePacks = nonReferencePacks
            .Where(static dp => dp.Settings.DatapackType != DatapackType.CompatibilityAddon)
            .ToArray();

        var compatibilityAddons = nonReferencePacks
            .Where(static dp => dp.Settings.DatapackType == DatapackType.CompatibilityAddon)
            .ToArray();

        var hasCompatibilityAddons = compatibilityAddons.Length > 0;

        if (!config.HasAnyValidPath)
        {
            TuiTheme.ShowError("No valid destination paths configured.");
            TuiTheme.Space();
            RenderPathStatus("WORLD_DATAPACKS_PATH", config.WorldDatapacksPath, config.IsWorldDatapacksPathValid);
            RenderPathStatus("WORLD_COMPAT_DATAPACKS_PATH", config.WorldCompatDatapacksPath, config.IsWorldCompatDatapacksPathValid, isOptional: !hasCompatibilityAddons);
            RenderPathStatus("RESOURCE_PACKS_PATH", config.ResourcePacksPath, config.IsResourcePacksPathValid);
            TuiTheme.Space();
            TuiTheme.ShowInfo("Please define existing directories in your .env file or system environment variables.");
            TuiTheme.WaitForKey();
            return Task.CompletedTask;
        }

        RenderPathStatus("Base World Target", config.WorldDatapacksPath, config.IsWorldDatapacksPathValid);
        RenderPathStatus("Compatibility World Target", config.WorldCompatDatapacksPath, config.IsWorldCompatDatapacksPathValid, isOptional: !hasCompatibilityAddons);
        RenderPathStatus("Resource Packs Target", config.ResourcePacksPath, config.IsResourcePacksPathValid);

        // Synchronize Base Datapacks (No compatibility addons)
        if (config.IsWorldDatapacksPathValid)
            SyncDatapacks("Base World", basePacks, config.WorldDatapacksPath!);
        else
            TuiTheme.ShowWarning("Skipping base datapacks sync: WORLD_DATAPACKS_PATH is not set or does not exist.");

        // Synchronize Compatibility World Datapacks (All addons including compatibility)
        if (hasCompatibilityAddons)
        {
            if (config.IsWorldCompatDatapacksPathValid)
                // In a compatibility testing world, both base addons and compatibility addons are required
                SyncDatapacks("Compatibility World", nonReferencePacks, config.WorldCompatDatapacksPath!);
            else
                TuiTheme.ShowWarning("Skipping compatibility world sync: compatibility addons are loaded, but WORLD_COMPAT_DATAPACKS_PATH is not set or does not exist.");

        }

        // Synchronize Resource Packs
        if (config.IsResourcePacksPathValid)
            SyncResourcePacks(nonReferencePacks, config.ResourcePacksPath!);
        else
            TuiTheme.ShowWarning("Skipping resource packs sync: RESOURCE_PACKS_PATH is not set or does not exist.");

        TuiTheme.ShowSuccess("Synchronization finished!");
        TuiTheme.WaitForKey();

        return Task.CompletedTask;
    }

    /// <summary>
    /// Synchronizes valid source datapack directories into a destination world datapacks directory.
    /// </summary>
    /// <param name="category">The label for the progress display.</param>
    /// <param name="datapacks">The loaded datapacks to process.</param>
    /// <param name="targetWorldDatapacksDir">The absolute destination directory for datapacks.</param>
    private static void SyncDatapacks(string category, IReadOnlyCollection<Datapack> datapacks, string targetWorldDatapacksDir)
    {
        var validPacks = datapacks
            .Where(static dp => !string.IsNullOrWhiteSpace(dp.Settings.Path) && Directory.Exists(dp.Settings.Path))
            .ToArray();

        if (validPacks.Length == 0)
        {
            TuiTheme.ShowWarning($"No datapacks with existing source directories found for {category}.");
            return;
        }

        TuiTheme.RunProgress($"Syncing {category} Datapacks", validPacks, pack =>
        {
            var folderName = !string.IsNullOrWhiteSpace(pack.ReleaseName)
                ? pack.ReleaseName
                : Path.GetFileName(pack.Settings.Path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

            var destinationDirectory = Path.Combine(targetWorldDatapacksDir, folderName);
            CopyDirectory(pack.Settings.Path, destinationDirectory);
        });

        TuiTheme.ShowSuccess($"Synchronized {validPacks.Length} datapack(s) to {category}.");
    }

    /// <summary>
    /// Synchronizes distinct valid language pack directories into the target resourcepacks folder.
    /// </summary>
    /// <param name="datapacks">The loaded datapacks containing optional language pack settings.</param>
    /// <param name="targetResourcePacksDir">The absolute destination directory for resource packs.</param>
    private static void SyncResourcePacks(IReadOnlyCollection<Datapack> datapacks, string targetResourcePacksDir)
    {
        var resourcePackSources = datapacks
            .Select(static dp => dp.Settings.LanguagePackSettings?.Path)
            .Where(static path => !string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
            .Select(static path => path!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (resourcePackSources.Length == 0)
        {
            TuiTheme.ShowWarning("No language packs with existing source directories found to synchronize.");
            return;
        }

        TuiTheme.RunProgress("Syncing Resource Packs", resourcePackSources, sourcePath =>
        {
            var folderName = Path.GetFileName(sourcePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            var destinationDirectory = Path.Combine(targetResourcePacksDir, folderName);
            CopyDirectory(sourcePath, destinationDirectory);
        });

        TuiTheme.ShowSuccess($"Synchronized {resourcePackSources.Length} resource pack(s).");
    }

    /// <summary>
    /// Replaces the destination directory entirely by removing any existing directory and performing a clean copy from the source directory.
    /// </summary>
    /// <param name="sourceDirectory">The directory path to copy from.</param>
    /// <param name="destinationDirectory">The destination directory path to completely recreate and populate.</param>
    /// <exception cref="DirectoryNotFoundException">Thrown when the source directory does not exist.</exception>
    /// <exception cref="IOException">Thrown when an I/O error occurs during directory deletion or file transfer.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when caller does not have the required permission to access or delete files.</exception>
    private static void CopyDirectory(string sourceDirectory, string destinationDirectory)
    {
        if (!Directory.Exists(sourceDirectory))
            throw new DirectoryNotFoundException($"Source directory not found: {sourceDirectory}");

        // Clean target directory completely
        if (Directory.Exists(destinationDirectory))
        {
            foreach (var file in Directory.EnumerateFiles(destinationDirectory, "*", SearchOption.AllDirectories))
                File.SetAttributes(file, FileAttributes.Normal);

            Directory.Delete(destinationDirectory, recursive: true);
        }

        Directory.CreateDirectory(destinationDirectory);

        foreach (var file in Directory.EnumerateFiles(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(sourceDirectory, file);
            var destinationFilePath = Path.Combine(destinationDirectory, relativePath);
            var destinationSubDir = Path.GetDirectoryName(destinationFilePath);

            if (destinationSubDir != null)
                Directory.CreateDirectory(destinationSubDir);

            File.Copy(file, destinationFilePath, overwrite: true);
        }
    }

    /// <summary>
    /// Displays a standardized status line for an environment path configuration.
    /// </summary>
    /// <param name="label">The human-readable name of the setting.</param>
    /// <param name="path">The resolved directory path.</param>
    /// <param name="isValid">A value indicating whether the directory exists and is usable.</param>
    /// <param name="isOptional">Indicates whether this path is optional under current conditions.</param>
    private static void RenderPathStatus(string label, string? path, bool isValid, bool isOptional = false)
    {
        if (isValid)
        {
            AnsiConsole.MarkupLine($"  [Grey84]{label}:[/] [green]{path}[/]");
            return;
        }

        if (isOptional && string.IsNullOrWhiteSpace(path))
        {
            AnsiConsole.MarkupLine($"  [Grey84]{label}:[/] [grey]not configured (optional)[/]");
            return;
        }

        var displayValue = string.IsNullOrWhiteSpace(path) ? "not configured" : $"directory not found ({path})";
        AnsiConsole.MarkupLine($"  [Grey84]{label}:[/] [red]{displayValue}[/]");
    }
}