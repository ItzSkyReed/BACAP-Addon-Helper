using BacapGenerator.Models.Datapacks;
using BacapGenerator.Services.Global;
using Spectre.Console;
using UI.Interfaces;
using UI.Styling;

namespace UI.Actions.Advancements;

/// <summary>
/// Action that generates global datapack functions (update_score, update_points, etc.) for the BACAP Enhanced addon.
/// </summary>
/// <param name="registry">The central registry providing access to loaded datapacks.</param>
public class GenerateDatapackFunctionsAction(DatapackRegistry registry) : IManageAdvancementsAction
{
    /// <inheritdoc/>
    public string Title => "Generate Global Datapack Functions";

    /// <summary>
    /// Prompts the user for confirmation and executes the global function generation for the BACAP Enhanced datapack.
    /// </summary>
    /// <returns>A completed task representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the BACAP Enhanced datapack has not been loaded into the registry.</exception>
    /// <example>
    /// <code>
    /// var action = new GenerateDatapackFunctionsAction(registry);
    /// await action.ExecuteAsync();
    /// </code>
    /// </example>
    public Task ExecuteAsync()
    {
        TuiTheme.RenderHeader(Title);

        var bacaped = registry.Bacaped;

        GlobalFunctionsService.GenerateAndSaveAll(bacaped);

        TuiTheme.Space();
        TuiTheme.ShowSuccess($"Successfully generated global functions for {bacaped.Id}!");
        TuiTheme.WaitForKey();

        return Task.CompletedTask;
    }
}