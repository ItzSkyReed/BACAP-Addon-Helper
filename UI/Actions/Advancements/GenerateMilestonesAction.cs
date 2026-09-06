using BacapGenerator.Models.Datapacks;
using BacapGenerator.Services.Global;
using UI.Interfaces;
using UI.Styling;

namespace UI.Actions.Advancements;

/// <summary>
/// Action that triggers the generation and persistence of milestone and advancement legend files for the BACAP Enhanced addon.
/// </summary>
/// <param name="registry">The central registry providing access to loaded datapacks.</param>
public class GenerateMilestonesAction(DatapackRegistry registry) : IManageAdvancementsAction
{
    /// <inheritdoc/>
    public string Title => "Generate Milestones & Advancement Legend";

    /// <summary>
    /// Prompts the user for confirmation and generates milestone and legend advancement files for the BACAP Enhanced datapack.
    /// </summary>
    /// <returns>A completed task representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the BACAP Enhanced datapack has not been loaded into the registry.</exception>
    /// <example>
    /// <code>
    /// var action = new GenerateMilestonesAction(registry);
    /// await action.ExecuteAsync();
    /// </code>
    /// </example>
    public Task ExecuteAsync()
    {
        TuiTheme.RenderHeader(Title);

        var bacaped = registry.Bacaped;

        GlobalAdvancementsService.GenerateAndSaveAll(bacaped);

        TuiTheme.Space();
        TuiTheme.ShowSuccess($"Successfully generated and saved milestone and legend files for {bacaped.Id}!");
        TuiTheme.WaitForKey();

        return Task.CompletedTask;
    }
}