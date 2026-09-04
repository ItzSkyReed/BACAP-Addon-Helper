using BacapGenerator.Models.Advancements;
using BacapGenerator.Models.Datapacks.Settings;
using Spectre.Console;
using UI.Styling;

namespace UI.Actions.Common;

/// <summary>
/// Provides reusable interactive search functionality for BACAP advancements via the TUI.
/// </summary>
public static class AdvancementSearcher
{
    /// <summary>
    /// Executes an interactive search loop for advancements.
    /// Continues prompting until the user selects an advancement or chooses to exit.
    /// </summary>
    /// <param name="advancements">The collection of advancements to search through.</param>
    /// <returns>The selected <see cref="BacapAdvancement"/>, or <see langword="null"/> if the user exits.</returns>
    public static BacapAdvancement? PromptSearch(IReadOnlyCollection<BacapAdvancement> advancements)
    {
        while (true)
        {
            TuiTheme.RenderHeader("Advancement Search");

            var query = AnsiConsole.Prompt(
                new TextPrompt<string>("[yellow]Enter advancement Title or McPath (or leave empty to go back):[/]")
                    .AllowEmpty());

            if (string.IsNullOrWhiteSpace(query))
                return null;

            // Search and rank results based on priorities
            var results = advancements
                .Select(adv =>
                {
                    var titleMatch = adv.TitleText.Contains(query, StringComparison.OrdinalIgnoreCase);
                    var pathMatch = adv.File.Name.Contains(query, StringComparison.OrdinalIgnoreCase);

                    var score = (titleMatch, pathMatch) switch
                    {
                        (true, true) => 1,  // Priority 1: Matches both Title and McPath
                        (true, false) => 2, // Priority 2: Matches only Title
                        (false, true) => 3, // Priority 3: Matches only McPath
                        _ => 4              // No match
                    };

                    return new { Advancement = adv, Score = score };
                })
                .Where(x => x.Score < 4)
                .OrderBy(x => x.Score)
                .ThenBy(x => x.Advancement.TitleText)
                .Select(x => x.Advancement)
                .ToList();

            if (results.Count == 0)
            {
                TuiTheme.ShowWarning($"No advancements found matching '{query}'.");
                TuiTheme.WaitForKey();
                continue; // Prompt again
            }

            // Prepare choices combining search results and the Back button
            var choices = results.Select(object (r) => r).ToList();
            choices.Add(new BackAction());

            TuiTheme.RenderHeader($"Search Results for '{query}'");

            var selected = TuiTheme.PromptSelection(
                $"Found [green]{results.Count}[/] match(es). Select one:",
                choices,
                item =>
                {
                    switch (item)
                    {
                        case BackAction nav:
                            return nav.Title;

                        case BacapAdvancement adv:
                        {
                            var escapedTitle = Markup.Escape(adv.TitleText);

                            var datapack = Markup.Escape(adv.Datapack.Id.ToDisplayName());


                            var path = Markup.Escape(adv.McPath);

                            // Renders as: Thorny Prices  bacaped:adventure/thorny_prices
                            return $"[white]{escapedTitle}[/]  [cyan]{datapack}[/] | [grey]{path}[/]";
                        }

                        default:
                            return item.ToString()!;
                    }
                });

            // If the user clicks "Back" in the results list, return to the text prompt
            if (selected is BackAction)
                continue;

            return selected as BacapAdvancement;
        }
    }
}