using BacapGenerator.Datapacks;
using BacapGenerator.LanguagePack.Services;
using Spectre.Console;
using UI.Interfaces;
using UI.Styling;

namespace UI.Actions.Datapacks;

/// <summary>
/// Action that synchronizes and regenerates <c>base_translation.json</c> files across all configured datapacks.
/// </summary>
public sealed class UpdateBaseTranslations(DatapackRegistry datapackRegistry) : IManageDatapacksAction
{
    private readonly BaseTranslationUpdateService _updateService = new();

    /// <inheritdoc/>
    public string Title => "Update Base Translations";

    /// <summary>
    /// Executes the generation process and displays an overview table of modified translation files.
    /// </summary>
    /// <returns>A completed <see cref="Task"/> representing the UI action lifecycle.</returns>
    /// <example>
    /// <code>
    /// var action = new UpdateBaseTranslations(registry);
    /// await action.ExecuteAsync();
    /// </code>
    /// </example>
    public Task ExecuteAsync()
    {
        TuiTheme.RenderHeader(Title);

        var results = _updateService.UpdateAll(datapackRegistry);

        if (results.Count == 0)
        {
            TuiTheme.ShowWarning("No primary addons with configured language pack settings found.");
            TuiTheme.WaitForKey();
            return Task.CompletedTask;
        }

        var table = TuiTheme.CreateTable("Datapack", "Addons Merged", "Status", "Output Path");

        var successCount = 0;

        foreach (var result in results)
        {
            var compatNames = result.CompatibilityAddons.Count > 0
                ? string.Join(", ", result.CompatibilityAddons.Select(a => a.ReleaseName))
                : "[grey]None[/]";

            if (result.IsSuccess)
            {
                successCount++;
                table.AddRow(
                    $"[white]{result.Datapack.ReleaseName}[/]",
                    compatNames,
                    "[bold green]Updated[/]",
                    $"[grey]{Markup.Escape(result.OutputFile!.FullName)}[/]");
            }
            else
            {
                table.AddRow(
                    $"[white]{result.Datapack.ReleaseName}[/]",
                    compatNames,
                    "[bold red]Failed[/]",
                    $"[red]{Markup.Escape(result.ErrorMessage ?? "Unknown error")}[/]");
            }
        }

        TuiTheme.RenderElement(table);
        TuiTheme.Space();

        if (successCount == results.Count)
            TuiTheme.ShowSuccess($"Successfully generated base translations for all {successCount} datapack(s).");
        else
            TuiTheme.ShowWarning($"Updated {successCount} of {results.Count} base translation files. Check the table above for errors.");

        TuiTheme.WaitForKey();
        return Task.CompletedTask;
    }
}