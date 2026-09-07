using BacapGenerator.Models.Advancements;
using Spectre.Console;
using UI.Interfaces;
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
    /// <example>
    /// <code>
    /// BacapAdvancement? selected = AdvancementSearcher.PromptSearch(allAdvancements);
    /// if (selected != null)
    /// {
    ///     Console.WriteLine(selected.TitleText);
    /// }
    /// </code>
    /// </example>
    public static async Task<BacapAdvancement?> PromptSearch(IReadOnlyCollection<BacapAdvancement> advancements)
    {
        while (true)
        {
            TuiTheme.RenderHeader("Advancement Search");

            var query = AnsiConsole.Prompt(
                new TextPrompt<string>("[yellow]Enter advancement Title or minecraft Path (or leave empty to go back):[/]")
                    .AllowEmpty());

            if (string.IsNullOrWhiteSpace(query))
            {
                return null;
            }

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

            // Map results to ITuiAction choices and append the back button
            var backAction = new BackAction();
            var choices = results
                .Select(ITuiAction (adv) => new SelectAdvancementAction(adv))
                .ToList();

            choices.Add(backAction);

            TuiTheme.RenderHeader($"Search Results for '{query}'");

            var selected = await TuiTheme.PromptSelectionOrDefaultAsync(
                $"Found [green]{results.Count}[/] match(es). Select one or press [bold]Q[/] to return:",
                choices,
                action => action.Title);

            // User chose to return to the search prompt
            if (selected is null || selected == backAction)
            {
                continue;
            }

            if (selected is SelectAdvancementAction selectAdvAction)
            {
                return selectAdvAction.Advancement;
            }
        }
    }

    /// <summary>
    /// Represents an actionable wrapper for selecting an advancement in the search results menu.
    /// </summary>
    /// <param name="advancement">The underlying advancement instance.</param>
    private sealed class SelectAdvancementAction(BacapAdvancement advancement) : ITuiAction
    {
        public BacapAdvancement Advancement { get; } = advancement;

        public string Title =>
            $"[white]{Markup.Escape(Advancement.TitleText)}[/]  [cyan]{Markup.Escape(Advancement.Datapack.Id)}[/] | [grey]{Markup.Escape(Advancement.McPath)}[/]";

        public Task ExecuteAsync() => Task.CompletedTask;
    }
}