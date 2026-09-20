using BacapGenerator.Checklists;
using BacapGenerator.Datapacks.Models.Settings.Checklists;
using Core.Commands.Impl;
using Core.Commands.Models;
using Core.McFunctions.Models;
using Core.McFunctions.Models.Interfaces;
using Core.SNBT;
using Core.TextComponents.Components;
using Core.TextComponents.Models;
using JetBrains.Annotations;

namespace BacapGenerator.Generation;

/// <summary>
/// Service responsible for synthesizing .mcfunction AST models from checklist definitions.
/// </summary>
public static class ChecklistGenerator
{
    /// <summary>
    /// Builds the trigger callback function (.mcfunction) containing mob data initializations,
    /// dynamic runtime entity checks, tellraw announcements, and scoreboard resets.
    /// </summary>
    /// <param name="definition">The checklist configuration settings.</param>
    /// <returns>A fully structured <see cref="McFunction"/> representing the callback logic.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="definition"/> is <see langword="null"/>.</exception>
    /// <example>
    /// <code>
    /// McFunction callback = ChecklistGenerator.GenerateTriggerCallback(checklistDefinition);
    /// </code>
    /// </example>
    [PublicAPI]
    public static McFunction GenerateTriggerCallback(ChecklistDefinitionSettings definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        var lines = new List<IMcFunctionLine>();

        // Generate storage reset and detection checks for every configured entity
        foreach (var category in definition.Categories)
        {
            var entities = ChecklistEntityPresetRegistry.ResolveEntities(category.Entities);

            foreach (var entity in entities)
            {
                var storageKey = $"{entity}Check";

                // Comment line: #mob
                lines.Add(new RawCommandLine($"#{entity}"));

                // Reset storage value to incomplete state
                var incompleteValue = Snbt.Compound()
                    .Put("translate", $"entity.minecraft.{entity}")
                    .Put("color", definition.Style.IncompleteColor)
                    .Build();

                lines.Add(new RawCommandLine(
                    $"data modify storage {definition.StorageName} {storageKey} set value {incompleteValue.ToSnbtString()}"));

                // Execute detection check to update value to complete state if entity is nearby
                var completeValue = Snbt.Compound()
                    .Put("translate", $"entity.minecraft.{entity}")
                    .Put("color", definition.Style.CompleteColor)
                    .Build();

                var selector = SelectorFormatter.BuildEntitySelector(
                    entity,
                    definition.Distance,
                    definition.ExtraSelector,
                    category.ExtraSelector);

                lines.Add(new RawCommandLine(
                    $"execute at @s if entity {selector} run data modify storage {definition.StorageName} {storageKey} set value {completeValue.ToSnbtString()}"));
            }
        }

        // Chat Header: top divider, title, bottom divider
        var dividerStyle = new TextStyle(Color: definition.Style.DividerColor, Strikethrough: true);

        var divider = new PlainTextComponent(Text: definition.Style.DividerText, Style: dividerStyle);

        lines.Add(new ExecutableLine(new TellrawCommand(Selector.SelectedPlayer, divider)));

        var headerStyle = new TextStyle(Color: "gray");

        var headerComponent = new TranslatableComponent(Translate: definition.HeaderTranslateKey, Style: headerStyle);

        lines.Add(new ExecutableLine(new TellrawCommand(Selector.SelectedPlayer, headerComponent)));

        lines.Add(new ExecutableLine(new TellrawCommand(Selector.SelectedPlayer, divider)));

        // Category tellraw lines
        foreach (var category in definition.Categories)
        {
            var entities = ChecklistEntityPresetRegistry.ResolveEntities(category.Entities);
            var tellrawMessage = BuildCategoryTellrawComponent(definition, category, entities);
            var tellrawCommand = new TellrawCommand(Selector.SelectedPlayer, tellrawMessage);

            if (!string.IsNullOrWhiteSpace(category.DimensionRequirement))
                lines.Add(new ExecutableLine(new ExecuteCommand(
                    $"at @s if dimension {category.DimensionRequirement}",
                    tellrawCommand)));
            else
                lines.Add(new ExecutableLine(tellrawCommand));
        }

        // Reset trigger scoreboard value
        lines.Add(new ExecutableLine(new ScoreboardPlayersMathCommand(
            ScoreboardMathOperation.Set,
            Selector.SelectedPlayer,
            definition.TriggerScoreboard,
            0)));

        return new McFunction(lines);
    }

    /// <summary>
    /// Builds the advancement check function (.mcfunction) that evaluates entity presence in a single command.
    /// </summary>
    /// <param name="definition">The parent checklist definition.</param>
    /// <param name="category">The specific category containing entities and target advancement.</param>
    /// <returns>A configured <see cref="McFunction"/> containing the validation and grant command.</returns>
    /// <exception cref="ArgumentNullException">Thrown when arguments are <see langword="null"/>.</exception>
    /// <example>
    /// <code>
    /// McFunction check = ChecklistGenerator.GenerateCategoryCheck(definition, category);
    /// </code>
    /// </example>
    [PublicAPI]
    public static McFunction GenerateCategoryCheck(ChecklistDefinitionSettings definition, ChecklistCategorySettings category)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(category);

        var entities = ChecklistEntityPresetRegistry.ResolveEntities(category.Entities);

        var conditions = string.Join(' ', entities.Select(entity =>
        {
            var selector = SelectorFormatter.BuildEntitySelector(
                entity,
                definition.Distance,
                definition.ExtraSelector,
                category.ExtraSelector);

            return $"if entity {selector}";
        }));

        var grantCmd = new AdvancementCommand(
            AdvancementAction.Grant,
            Selector.SelectedPlayer,
            AdvancementMode.Only,
            category.Advancement);

        var executeCmd = new ExecuteCommand($"at @s {conditions}", grantCmd);

        return new McFunction([new ExecutableLine(executeCmd)]);
    }

    private static PlainTextComponent BuildCategoryTellrawComponent(
        ChecklistDefinitionSettings definition,
        ChecklistCategorySettings category,
        IReadOnlyList<string> entities)
    {
        var elements = new List<TextComponent>();

        var separatorStyle = new TextStyle(Color: definition.Style.SeparatorColor);
        var andSeparator = new TranslatableComponent(Translate: " and ", Style: separatorStyle);
        var commaSeparator = new PlainTextComponent(Text: ", ", Style: separatorStyle);

        // Category Prefix (e.g. "Overworld: ")
        if (!string.IsNullOrWhiteSpace(category.PrefixText))
        {
            var prefixStyle = new TextStyle(Color: category.PrefixColor ?? definition.Style.SeparatorColor);
            elements.Add(new TranslatableComponent(Translate: category.PrefixText, Style: prefixStyle));
        }

        // List of entities backed by command storage NBT values
        for (var i = 0; i < entities.Count; i++)
        {
            var storageKey = $"{entities[i]}Check";

            var storageNode = Snbt.Compound()
                .Put("storage", definition.StorageName)
                .Put("nbt", storageKey)
                .Put("interpret", true)
                .Build();

            var storageComponent = TextComponent.Parse(storageNode);

            if (i == entities.Count - 1 && entities.Count > 1)
            {
                elements.Add(andSeparator);
                elements.Add(storageComponent);
            }
            else
            {
                elements.Add(storageComponent);

                if (i < entities.Count - 2)
                    elements.Add(commaSeparator);
            }
        }

        return new PlainTextComponent(string.Empty, Extra: elements);
    }
}