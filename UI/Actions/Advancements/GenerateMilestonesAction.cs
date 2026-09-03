using BacapGenerator.Models.Datapacks;
using BacapGenerator.Models.Datapacks.Settings;
using BacapGenerator.Services.Global;
using Spectre.Console;
using UI.Interfaces;
using UI.Styling;

namespace UI.Actions.Advancements;

/// <summary>
/// Action that triggers the generation of milestones and advancement legends across eligible datapacks.
/// </summary>
public class GenerateMilestonesAction(DatapackRegistry registry) : IManageAdvancementsAction
{
    public string Title => "Generate Milestones & Advancement Legend";

    public Task ExecuteAsync()
    {
        var targetDatapacks = registry.All.Values
            .Where(dp => dp.Settings is { Access: DatapackAccess.ReadWrite, MilestoneMcPaths.Count: > 0 } &&
                         !string.IsNullOrWhiteSpace(dp.Settings.AdvancementLegendMcPath))
            .ToList();

        if (targetDatapacks.Count == 0)
        {
            TuiTheme.ShowWarning("No datapacks found with ReadWrite access, MilestoneMcPaths, and AdvancementLegendMcPath configured.");
            TuiTheme.WaitForKey();
            return Task.CompletedTask;
        }

        TuiTheme.RenderHeader(Title);

        var confirm = AnsiConsole.Confirm(
            $"Found [green]{targetDatapacks.Count}[/] eligible datapack(s). Ready to generate and overwrite milestone/legend files?");

        if (!confirm)
            return Task.CompletedTask;

        TuiTheme.Space();

        // The progress bar now iterates over datapacks, delegating work to the service
        TuiTheme.RunProgress(
            "Generating milestones and legends...",
            targetDatapacks,
            GlobalAdvancementsService.GenerateAndSaveAll
        );

        TuiTheme.Space();
        TuiTheme.ShowSuccess("Successfully generated and saved milestone and legend files!");
        TuiTheme.WaitForKey();

        return Task.CompletedTask;
    }
}