using Spectre.Console;
using UI.Interfaces;
using UI.Services;
using UI.Styling;

namespace UI.Actions.Datapacks;

/// <summary>
/// Action that synchronizes documentation requirements and generates final JSON payloads for export.
/// </summary>
public class GenerateDocumentationAction(DocumentationRunnerService documentationRunner) : IManageDatapacksAction
{

    public string Title => "Sync Requirements & Generate Documentation";

    /// <summary>
    /// Executes the documentation synchronization and generation workflow.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task ExecuteAsync()
    {
        TuiTheme.RenderHeader(Title);
        AnsiConsole.MarkupLine("[grey]Running documentation pipeline...[/]");

        documentationRunner.RunAndRenderReport(promptOnOrphaned: false);

        TuiTheme.WaitForKey();
        return Task.CompletedTask;
    }
}