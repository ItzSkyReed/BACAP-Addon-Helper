using BacapGenerator.Configuration;
using BacapGenerator.Datapacks;
using BacapGenerator.DocGen;
using Spectre.Console;
using UI.Styling;

namespace UI.Services;

/// <summary>
/// A shared UI service responsible for running the documentation pipeline and rendering reports in the console.
/// </summary>
public sealed class DocumentationRunnerService(DatapackRegistry datapackRegistry, GlobalConfig globalConfig)
{
    /// <summary>
    /// Executes the documentation pipeline, renders warnings, stubs alerts, and orphaned requirements directly to the console.
    /// </summary>
    /// <param name="promptOnOrphaned">If set to <see langword="true"/>, prompts the user to confirm proceeding when orphaned requirements are found.</param>
    /// <returns><see langword="true"/> if documentation generation succeeded or was accepted; otherwise, <see langword="false"/> if stubs are pending or the user aborted.</returns>
    public bool RunAndRenderReport(bool promptOnOrphaned = false)
    {
        var docConfig = globalConfig.DocumentGenerator;
        var result = DocumentationPipeline.Run(datapackRegistry, docConfig);

        if (result.WasSkipped)
        {
            AnsiConsole.MarkupLine($"[yellow]Skipped:[/] {result.ErrorMessage ?? "Documentation generation was skipped."}");
            return true;
        }

        // Render obsolete mc_path definitions found in YAML
        if (result.HasOrphanedAdvancements)
        {
            RenderOrphanedAdvancementsReport(result.OrphanedAdvancements);
        }

        // Handle newly created stubs requiring user intervention
        if (result.StubsGenerated)
        {
            AnsiConsole.MarkupLine("[bold red]Documentation is incomplete! Process aborted.[/]");
            AnsiConsole.MarkupLine($"[bold yellow]Action Required:[/] Added [bold cyan]{result.TotalAdded}[/] new stub(s), injected [bold cyan]{result.TotalInjected}[/] missing section(s).");
            AnsiConsole.MarkupLine($"  [grey]1.[/] Edit YAML files in [yellow]{docConfig?.RequirementsDirectory}[/]");
            AnsiConsole.MarkupLine("  [grey]2.[/] Fill in missing descriptions (and remove obsolete entries shown above)");
            AnsiConsole.MarkupLine("  [grey]3.[/] Save the files and run this process again.\n");

            return false;
        }

        // If orphaned entries exist and confirmation is requested (e.g. during release)
        if (result.HasOrphanedAdvancements && promptOnOrphaned)
        {
            var shouldContinue = AnsiConsole.Confirm(
                "[yellow]Obsolete requirement paths were detected above. Do you want to proceed anyway?[/]",
                defaultValue: false);

            if (!shouldContinue)
            {
                TuiTheme.ShowError("Operation cancelled by user to clean up obsolete requirements.");
                return false;
            }
        }

        if (!result.IsSuccess)
        {
            TuiTheme.ShowError(result.ErrorMessage ?? "Documentation pipeline failed unexpectedly.");
            return false;
        }

        TuiTheme.ShowSuccess($"Successfully generated documentation for {result.ProcessedAddons} addon(s).");
        return true;
    }

    /// <summary>
    /// Renders a concise diagnostic list displaying requirement paths that no longer exist in the datapacks.
    /// </summary>
    /// <param name="orphanedAdvancements">A dictionary mapping addon IDs to lists of orphaned requirement paths.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="orphanedAdvancements"/> is null.</exception>
    private static void RenderOrphanedAdvancementsReport(
        IReadOnlyDictionary<string, IReadOnlyList<string>> orphanedAdvancements)
    {
        ArgumentNullException.ThrowIfNull(orphanedAdvancements);

        var totalOrphaned = orphanedAdvancements.Values.Sum(list => list.Count);
        AnsiConsole.MarkupLine($"\n[bold orange1]! Found {totalOrphaned} obsolete requirement(s) in YAML (not in datapacks):[/]");

        foreach (var (addonId, paths) in orphanedAdvancements)
        {
            foreach (var path in paths)
            {
                AnsiConsole.MarkupLine($"  [grey]•[/] [[[cyan]{addonId}[/]]] [yellow]{Markup.Escape(path)}[/]");
            }
        }

        AnsiConsole.WriteLine();
    }
}