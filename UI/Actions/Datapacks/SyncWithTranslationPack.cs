using BacapGenerator.Datapacks;
using BacapGenerator.LanguagePacks.Services;
using Spectre.Console;
using UI.Interfaces;
using UI.Styling;

namespace UI.Actions.Datapacks;

/// <summary>
/// Action that synchronizes base translation templates and updates language pack files with missing translation entries.
/// </summary>
public sealed class SyncWithTranslationPack(DatapackRegistry datapackRegistry) : IManageDatapacksAction
{

    /// <inheritdoc/>
    public string Title => "Sync With Translation Pack";

    /// <summary>
    /// Executes the translation pack synchronization and renders a detailed breakdown of modified files.
    /// </summary>
    /// <returns>A completed <see cref="Task"/> representing the UI action execution.</returns>
    /// <example>
    /// <code>
    /// var action = new SyncWithTranslationPack(datapackRegistry);
    /// await action.ExecuteAsync();
    /// </code>
    /// </example>
    public Task ExecuteAsync()
    {
        TuiTheme.RenderHeader(Title);

        var results = TranslationPackSyncService.SyncAll(datapackRegistry);

        if (results.Count == 0)
        {
            TuiTheme.ShowWarning("No primary addons with configured language pack settings found.");
            TuiTheme.WaitForKey();
            return Task.CompletedTask;
        }

        foreach (var result in results)
        {
            var primaryName = result.Group.Primary.ReleaseName;
            var compatNames = result.Group.CompatibilityAddons.Count > 0
                ? $" (Merged addons: {string.Join(", ", result.Group.CompatibilityAddons.Select(a => a.ReleaseName))})"
                : string.Empty;

            AnsiConsole.MarkupLine($"[bold cyan]{primaryName}[/][grey]{compatNames}[/]");

            if (!result.IsSuccess)
            {
                TuiTheme.ShowError(result.ErrorMessage ?? "Unknown synchronization error");
                TuiTheme.Space();
                continue;
            }

            AnsiConsole.MarkupLine(
                $"  [green]✓[/] Base template updated: [grey]{Markup.Escape(result.BaseTranslationFile!.Name)}[/] " +
                $"([bold]{result.TotalRequiredKeys}[/] keys total)");

            if (result.FileSummaries.Count == 0)
            {
                TuiTheme.ShowWarning("  No language files found in resource pack assets/minecraft/lang folder.");
                TuiTheme.Space();
                continue;
            }

            var table = TuiTheme.CreateTable("Locale", "Group", "Translated", "Status");

            foreach (var summary in result.FileSummaries.OrderBy(s => s.File.Code))
            {
                var statusMarkup = (summary.WasPatched, summary.MissingKeysCount, summary.UnusedKeysCount) switch
                {
                    (false, > 0, _) or (false, _, > 0) => "[bold red]Patch failed[/]",
                    (_, 0, 0) => "[bold green]Up to date[/]",
                    (_, > 0, > 0) => $"[bold yellow]+{summary.MissingKeysCount}[/] [bold red]-{summary.UnusedKeysCount}[/]",
                    (_, > 0, 0) => $"[bold yellow]+{summary.MissingKeysCount} added[/]",
                    (_, 0, > 0) => $"[bold red]-{summary.UnusedKeysCount} removed[/]"
                };

                table.AddRow(
                    $"[white]{summary.File.Code}[/]",
                    $"[Gray84]{summary.File.MajorLanguageGroup}[/]",
                    $"[green]{summary.ExistingCount}[/]",
                    statusMarkup);
            }

            TuiTheme.RenderElement(table);
            TuiTheme.Space();
        }

        TuiTheme.ShowSuccess("Translation packs synchronization completed.");
        TuiTheme.WaitForKey();

        return Task.CompletedTask;
    }
}