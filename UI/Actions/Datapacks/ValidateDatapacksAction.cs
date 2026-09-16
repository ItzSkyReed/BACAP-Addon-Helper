using BacapGenerator.Models.Datapacks;
using UI.Interfaces;
using UI.Services;
using UI.Styling;

namespace UI.Actions.Datapacks;

/// <summary>
/// Action that runs validation across all loaded datapacks and reports results without building.
/// </summary>
public sealed class ValidateDatapacksAction(DatapackRegistry datapackRegistry, ValidationRunnerService validationService) : IManageDatapacksAction
{
    public string Title => "Run Datapack Validation";

    public Task ExecuteAsync()
    {
        TuiTheme.RenderHeader(Title);

        foreach (var datapack in datapackRegistry.Values)
        {
            validationService.ValidateAndRenderReport(datapack);
        }

        TuiTheme.WaitForKey();
        return Task.CompletedTask;
    }
}