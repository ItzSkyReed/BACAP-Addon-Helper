using BacapGenerator.Configuration;
using BacapGenerator.Datapacks;
using BacapGenerator.DocGen;
using Spectre.Console;
using UI.Interfaces;
using UI.Styling;

namespace UI.Actions.Datapacks;

/// <summary>
/// Action that synchronizes documentation requirements and generates final JSON payloads for web export.
/// </summary>
public class GenerateDocumentationAction(DatapackRegistry registry, GlobalConfig globalConfig) : IManageDatapacksAction
{
    public string Title => "Sync Requirements & Generate Documentation";

    public Task ExecuteAsync()
    {
        TuiTheme.RenderHeader(Title);
        AnsiConsole.MarkupLine("[grey]Running documentation pipeline...[/]");

        var result = DocumentationPipeline.Run(registry, globalConfig.DocumentGenerator);

        if (result.WasSkipped)
        {
            TuiTheme.ShowAlert("Skipped", result.ErrorMessage ?? "Skipped", Color.Yellow);
            TuiTheme.WaitForKey();
            return Task.CompletedTask;
        }

        if (result.StubsGenerated)
        {
            TuiTheme.Space();
            TuiTheme.ShowAlert("Action Required",
                $"Added [bold cyan]{result.TotalAdded}[/] new stubs and injected [bold cyan]{result.TotalInjected}[/] missing sections into YAML files.\n\n" +
                $"[white]1. Open the YAML files in[/] [yellow]{globalConfig.DocumentGenerator!.RequirementsDirectory}[/]\n" +
                $"[white]2. Fill in the missing descriptions.[/]\n" +
                $"[white]3. Save the files and run this action again to generate the JSON.[/]",
                Color.Yellow);

            TuiTheme.WaitForKey();
            return Task.CompletedTask;
        }

        TuiTheme.Space();
        TuiTheme.ShowSuccess($"Successfully generated web documentation for {result.ProcessedAddons} addon(s).");
        TuiTheme.WaitForKey();

        return Task.CompletedTask;
    }
}