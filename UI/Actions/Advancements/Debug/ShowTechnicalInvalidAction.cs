using BacapGenerator.Advancements.Models;
using BacapGenerator.Datapacks;
using BacapGenerator.Datapacks.Models;
using Spectre.Console;
using UI.Interfaces;
using UI.Styling;

namespace UI.Actions.Advancements.Debug;

/// <summary>
/// Debug action that displays counts, filenames, and failure reasons for non-BACAP advancements.
/// </summary>
public class ShowTechnicalInvalidAction(DatapackRegistry registry) : IDebugAdvancementsAction
{
    public string Title => "View Technical/Invalid advancements";

    public Task ExecuteAsync()
    {
        TuiTheme.RenderHeader("Technical & Invalid Advancements Debug");

        var summaryTable = TuiTheme.CreateTable("Datapack", "Category", "Count");
        var foundAny = false;

        foreach (var (id, datapack) in registry)
        {
            var nonBacap = datapack.Advancements
                .Where(adv => adv is not BacapAdvancement)
                .ToList();

            if (nonBacap.Count == 0)
                continue;

            foundAny = true;

            var grouped = nonBacap
                .GroupBy(adv => adv.GetType().Name)
                .OrderBy(g => g.Key);

            foreach (var group in grouped)
            {
                summaryTable.AddRow(
                    $"[cyan]{id}[/]",
                    $"[yellow]{group.Key}[/]",
                    $"[bold red]{group.Count()}[/]"
                );
            }
        }

        if (!foundAny)
        {
            TuiTheme.ShowInfo("No technical or invalid advancements found in any datapack.", "green");
            TuiTheme.WaitForKey();
            return Task.CompletedTask;
        }

        TuiTheme.RenderElement(summaryTable);
        TuiTheme.Space();

        foreach (var (id, datapack) in registry)
        {
            var nonBacap = datapack.Advancements
                .Where(adv => adv is not BacapAdvancement)
                .ToList();

            if (nonBacap.Count == 0)
                continue;

            var rootTree = TuiTheme.CreateTree($"[bold cyan]{id}[/]");

            var grouped = nonBacap
                .GroupBy(adv => adv.GetType().Name)
                .OrderBy(g => g.Key);

            foreach (var group in grouped)
            {
                var typeNode = rootTree.AddNode($"[yellow]{group.Key}[/] [grey]({group.Count()})[/]");

                foreach (var adv in group.OrderBy(a => a.File.Name))
                {
                    if (adv is InvalidAdvancement invalidAdv)
                    {
                        var escapedReason = Markup.Escape(invalidAdv.ErrorReason.ToString());
                        typeNode.AddNode($"[grey]{adv.File.Name}[/] - [red]{escapedReason}[/]");
                    }
                    else
                    {
                        typeNode.AddNode($"[grey]{adv.File.Name}[/]");
                    }
                }
            }

            TuiTheme.RenderElement(rootTree);
            TuiTheme.Space();
        }

        TuiTheme.WaitForKey();
        return Task.CompletedTask;
    }
}