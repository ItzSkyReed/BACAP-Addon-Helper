using BacapGenerator.Models.Datapacks;
using BacapGenerator.Models.Datapacks.Settings;
using BacapGenerator.Services.Global;
using Spectre.Console;
using UI.Interfaces;
using UI.Styling;

namespace UI.Actions.Advancements;

/// <summary>
/// Action that generates global datapack functions (update_score, update_points, etc.).
/// </summary>
public class GenerateDatapackFunctionsAction(DatapackRegistry registry) : IManageAdvancementsAction
{
    public string Title => "Generate Global Datapack Functions";

    public Task ExecuteAsync()
    {
        var targetDatapacks = registry.All.Values
            .Where(dp => dp.Settings is { Access: DatapackAccess.ReadWrite })
            .ToList();

        if (targetDatapacks.Count == 0)
        {
            TuiTheme.ShowWarning("No read-write datapacks found.");
            TuiTheme.WaitForKey();
            return Task.CompletedTask;
        }

        TuiTheme.RenderHeader(Title);

        var confirm = AnsiConsole.Confirm($"Generate functions for [green]{targetDatapacks.Count}[/] datapacks?");
        if (!confirm) return Task.CompletedTask;

        TuiTheme.RunProgress("Generating global functions...", targetDatapacks, GlobalFunctionsService.GenerateAndSaveAll);

        TuiTheme.Space();
        TuiTheme.ShowSuccess("Successfully generated global functions!");
        TuiTheme.WaitForKey();

        return Task.CompletedTask;
    }
}