using System.Text.RegularExpressions;
using BacapGenerator.Advancements.Models;
using BacapGenerator.Advancements.Services;
using BacapGenerator.Checklists;
using BacapGenerator.Configuration;
using BacapGenerator.Datapacks;
using BacapGenerator.Datapacks.Models;
using BacapGenerator.Datapacks.Models.Settings;
using BacapGenerator.Datapacks.Services;
using BacapGenerator.Io;
using Spectre.Console;
using UI.Interfaces;
using UI.Services;
using UI.Styling;

namespace UI.Menus;

/// <summary>
/// Sub-menu for managing existing advancements.
/// </summary>
public partial class ReleaseMenu(DatapackRegistry registry, GlobalConfig config, ValidationRunnerService validationService) : IMainMenuAction
{
    [GeneratedRegex(@"^[0-9]+\.[0-9]+(\.[0-9]+)?(-(alpha|beta))?$", RegexOptions.IgnoreCase)]
    private static partial Regex VersionPatternRegex();

    public string Title => "Make Release";

    /// <summary>
    /// Executes the interactive release process by prompting for versioning per parent addon family
    /// and archiving all corresponding datapack folders into distribution archives.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when an orphaned compatibility addon is detected.</exception>
    public async Task ExecuteAsync()
    {
        if (string.IsNullOrWhiteSpace(config.ReleasePath))
        {
            TuiTheme.ShowError("Release path not specified, release will be cancelled.");
            TuiTheme.WaitForKey();
            return;
        }

        var nonReferencePacks = registry.Values
            .Where(dp => dp.Settings.Type != DatapackType.Reference)
            .ToArray();

        // Pre-release Validation
        TuiTheme.RenderHeader("Pre-Release Validation");
        var validationPassed = true;

        foreach (var pack in nonReferencePacks)
        {
            if (!validationService.ValidateAndRenderReport(pack))
            {
                validationPassed = false;
            }
        }

        if (!validationPassed)
        {
            TuiTheme.Space();
            TuiTheme.ShowError("Validation failed. Please fix the highlighted ERRORS before making a release.");
            TuiTheme.WaitForKey();
            return; // Abort release
        }

        TuiTheme.Space();
        TuiTheme.ShowSuccess("All datapacks passed validation! Proceeding with release.");
        TuiTheme.Space();

        // Separate root addons from compatibility addons
        var rootAddons = nonReferencePacks
            .Where(dp => dp.Settings.Type == DatapackType.Addon)
            .ToArray();

        var compatibilityAddons = nonReferencePacks
            .Where(dp => dp.Settings.Type == DatapackType.CompatibilityAddon)
            .ToArray();

        // Iterate through each root addon family
        foreach (var parent in rootAddons)
        {
            var parentId = parent.Id;
            var parentDisplayName = parent.Settings.ReleaseName ?? parent.Settings.MainNamespace;

            AnsiConsole.MarkupLine($"\n[bold yellow]Processing release family:[/] [cyan]{parentDisplayName}[/]");

            // Ask version once per root family
            var version = AskValidVersion();

            // Find all compatibility addons linked to this parent
            var relatedChildren = compatibilityAddons
                .Where(child => string.Equals(child.Settings.ParentDatapackId, parentId, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            var releaseFamily = new List<Datapack>(1 + relatedChildren.Length) { parent };
            releaseFamily.AddRange(relatedChildren);

            // Process archiving for each member of the family
            foreach (var datapack in releaseFamily)
            {
                // Todo I dont like this
                AnsiConsole.MarkupLine($"  [green]•[/] Processing [white]{datapack.ReleaseName}[/] (version [teal]{version}[/])...");

                if (datapack.Settings.Type == DatapackType.Addon)
                {
                    // Todo: Better generation information
                    GlobalAdvancementsService.GenerateAndSaveAll(datapack);
                    GlobalFunctionsService.GenerateAndSaveAll(datapack);

                    ChecklistsService.GenerateAndSaveAll(datapack);

                    foreach (var advancement in datapack.Advancements.OfType<ValidAdvancement>())
                    {
                        AdvancementIoManager.SaveAdvancement(advancement);
                    }
                }

                await DatapackIoManager.ArchiveDatapackAsync(datapack, version, config.ReleasePath);
            }
        }

        TuiTheme.ShowSuccess("Successfully archived all datapacks");
        TuiTheme.WaitForKey();

        var processedIds = rootAddons.Select(a => a.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var orphanedChildren = compatibilityAddons
            .Where(c => string.IsNullOrWhiteSpace(c.Settings.ParentDatapackId) || !processedIds.Contains(c.Settings.ParentDatapackId))
            .ToArray();

        if (orphanedChildren.Length > 0)
        {
            var orphanNames = string.Join(", ", orphanedChildren.Select(c => c.Settings.MainNamespace));
            throw new InvalidOperationException($"Found orphaned CompatibilityAddons without a registered parent Addon: {orphanNames}");
        }
    }


    /// <summary>
    /// Prompts the user to enter a version string matching the pattern 'X.Y[.Z][-alpha|-beta]'.
    /// Continues prompting until validation passes.
    /// </summary>
    /// <returns>A validated semantic-style version string.</returns>
    /// <example>
    /// Valid values: "1.0", "1.0.3", "2.1-beta", "3.0.0-alpha".
    /// </example>
    private static string AskValidVersion()
    {
        return AnsiConsole.Prompt(
            new TextPrompt<string>("Enter [green]datapack version[/]:")
                .PromptStyle("teal")
                .Validate(input =>
                {
                    if (string.IsNullOrWhiteSpace(input))
                        return ValidationResult.Error("[red]Version cannot be empty![/]");

                    var trimmed = input.Trim();

                    if (!VersionPatternRegex().IsMatch(trimmed))
                    {
                        return ValidationResult.Error(
                            "[red]Invalid format![/] Expected: [yellow]X.Y[/], [yellow]X.Y.Z[/], [yellow]X.Y-alpha[/], [yellow]X.Y.Z-beta[/] etc."
                        );
                    }

                    return ValidationResult.Success();
                }));
    }
}