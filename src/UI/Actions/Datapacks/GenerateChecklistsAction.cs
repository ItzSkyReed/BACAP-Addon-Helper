using BacapGenerator.Checklists;
using BacapGenerator.Datapacks;
using BacapGenerator.Datapacks.Models.Settings;
using UI.Interfaces;
using UI.Styling;

namespace UI.Actions.Datapacks;

/// <summary>
/// Action that generates all checklist functions (trigger callbacks, entity verification mcfunctions)
/// for loaded datapacks that have checklists configured.
/// </summary>
/// <param name="registry">The central registry providing access to loaded datapacks.</param>
public class GenerateChecklistsAction(DatapackRegistry registry) : IManageDatapacksAction
{
    /// <inheritdoc/>
    public string Title => "Generate Checklist Functions";

    /// <summary>
    /// Executes checklist generation across all non-reference datapacks with configured checklists.
    /// </summary>
    /// <returns>A completed task representing the asynchronous operation.</returns>
    /// <example>
    /// <code>
    /// var action = new GenerateChecklistsAction(registry);
    /// await action.ExecuteAsync();
    /// </code>
    /// </example>
    public Task ExecuteAsync()
    {
        TuiTheme.RenderHeader(Title);

        var targetPacks = registry.Values
            .Where(d => d.Settings.Type != DatapackType.Reference && d.Settings.Checklists.Count > 0)
            .ToArray();

        if (targetPacks.Length == 0)
        {
            TuiTheme.ShowInfo("No datapacks found with configured checklists.", "darkorange");
            TuiTheme.WaitForKey();
            return Task.CompletedTask;
        }

        foreach (var pack in targetPacks)
        {
            var count = ChecklistsService.GenerateAndSaveAll(pack);
            TuiTheme.ShowSuccess($"Generated [white]{count}[/] checklist(s) for [cyan]{pack.ReleaseName}[/]");
        }

        TuiTheme.Space();
        TuiTheme.WaitForKey();

        return Task.CompletedTask;
    }
}