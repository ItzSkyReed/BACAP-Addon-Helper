using BacapGenerator.Advancements.Models;
using Core.TextComponents.Components;
using Spectre.Console;
using UI.Interfaces;
using UI.Styling;

namespace UI.Actions.Common;

/// <summary>
/// Provides reusable interactive search and selection functionality for advancements via the TUI.
/// Supports both fully-featured BACAP advancements and technical/vanilla root advancements.
/// </summary>
public static class AdvancementSearcher
{
    /// <summary>
    /// Executes an interactive search loop for advancements of the specified type.
    /// Continues prompting until the user selects an advancement or chooses to exit.
    /// </summary>
    /// <typeparam name="TAdvancement">The specific advancement type deriving from <see cref="ValidAdvancement"/>.</typeparam>
    /// <param name="advancements">The collection of valid advancements to search through.</param>
    /// <param name="headerTitle">The header text displayed above the search prompt.</param>
    /// <returns>The selected advancement instance, or <see langword="null"/> if the user cancels or exits.</returns>
    /// <example>
    /// <code>
    /// // Searching strictly editable BACAP advancements:
    /// BacapAdvancement? selected = await AdvancementSearcher.PromptSearch(editableList);
    ///
    /// // Searching any valid advancement for parent selection:
    /// ValidAdvancement? parent = await AdvancementSearcher.PromptSearch(allValidList, "Select Parent Advancement");
    /// </code>
    /// </example>
    public static async Task<TAdvancement?> PromptSearch<TAdvancement>(
        IReadOnlyCollection<TAdvancement> advancements,
        string headerTitle = "Advancement Search")
        where TAdvancement : ValidAdvancement
    {
        while (true)
        {
            TuiTheme.RenderHeader(headerTitle);

            var query = AnsiConsole.Prompt(
                new TextPrompt<string>("[yellow]Enter advancement Title or Minecraft Path (or leave empty to go back):[/]")
                    .AllowEmpty());

            if (string.IsNullOrWhiteSpace(query))
                return null;

            // Search and rank results based on title and McPath matches
            var results = advancements
                .Select(adv =>
                {
                    var title = TryGetTitle(adv);
                    var hasTitle = !string.IsNullOrWhiteSpace(title);

                    var titleMatch = hasTitle && title.Contains(query, StringComparison.OrdinalIgnoreCase);
                    var pathMatch = adv.McPath.Contains(query, StringComparison.OrdinalIgnoreCase)
                                 || adv.File.Name.Contains(query, StringComparison.OrdinalIgnoreCase);

                    var score = (titleMatch, pathMatch) switch
                    {
                        (true, true) => 1,  // Priority 1: Matches both Title and Path
                        (true, false) => 2, // Priority 2: Matches Title only
                        (false, true) => 3, // Priority 3: Matches Path only
                        _ => 4              // No match
                    };

                    return new { Advancement = adv, Title = title, Score = score };
                })
                .Where(x => x.Score < 4)
                .OrderBy(x => x.Score)
                .ThenBy(x => x.Title)
                .ThenBy(x => x.Advancement.McPath)
                .Select(x => x.Advancement)
                .ToList();

            if (results.Count == 0)
            {
                TuiTheme.ShowWarning($"No advancements found matching '{query}'.");
                TuiTheme.WaitForKey();
                continue;
            }

            var backAction = new BackAction();
            var choices = results
                .Select(ITuiAction (adv) => new SelectAdvancementAction<TAdvancement>(adv))
                .ToList();

            choices.Add(backAction);

            TuiTheme.RenderHeader($"Search Results for '{query}'");

            var selected = await TuiTheme.PromptSelectionOrDefaultAsync(
                $"Found [green]{results.Count}[/] match(es). Select one or press [bold]Q[/] to return:",
                choices,
                action => action.Title);

            if (selected is null || selected == backAction)
                continue;

            if (selected is SelectAdvancementAction<TAdvancement> selectAdvAction)
                return selectAdvAction.Advancement;
        }
    }

    /// <summary>
    /// Safely extracts the display title string from any <see cref="ValidAdvancement"/>,
    /// returning <see cref="string.Empty"/> if the advancement lacks display metadata.
    /// </summary>
    /// <param name="advancement">The advancement to extract the title from.</param>
    /// <returns>The resolved title string, or an empty string for technical advancements.</returns>
    private static string TryGetTitle(ValidAdvancement advancement)
    {
        if (advancement is BacapAdvancement bacap)
            return bacap.TitleText;

        var titleComponent = advancement.Advancement.Display?.Title;
        if (titleComponent is null)
            return string.Empty;

        return (titleComponent as TranslatableComponent)?.Translate
               ?? (titleComponent as PlainTextComponent)?.Text
               ?? titleComponent.ToString()!;
    }

    /// <summary>
    /// Actionable wrapper for selecting an advancement in the search results menu.
    /// </summary>
    /// <typeparam name="TItem">The concrete advancement type.</typeparam>
    /// <param name="advancement">The underlying advancement instance.</param>
    private sealed class SelectAdvancementAction<TItem>(TItem advancement) : ITuiAction
        where TItem : ValidAdvancement
    {
        public TItem Advancement { get; } = advancement;

        public string Title
        {
            get
            {
                var titleText = TryGetTitle(Advancement);
                var titleDisplay = !string.IsNullOrWhiteSpace(titleText)
                    ? $"[white]{Markup.Escape(titleText)}[/]"
                    : "[grey][[Technical]][/]";

                return $"{titleDisplay} [cyan]{Markup.Escape(Advancement.Datapack.Id)}[/] | [grey]{Markup.Escape(Advancement.McPath)}[/]";
            }
        }

        public Task ExecuteAsync() => Task.CompletedTask;
    }
}