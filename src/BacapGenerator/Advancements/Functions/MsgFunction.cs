using BacapGenerator.Advancements.Models;
using BacapGenerator.Common;
using BacapGenerator.Datapacks.Models;
using Core.Commands.Impl;
using Core.Commands.Models;
using Core.McFunctions.Models;
using Core.McFunctions.Models.Extensions;
using Core.TextComponents.Components;
using Core.TextComponents.Events;
using Core.TextComponents.Models;
using JetBrains.Annotations;

namespace BacapGenerator.Advancements.Functions;

/// <summary>
/// Represents the message function file that executes the /tellraw announcement.
/// Preserves any custom commands (e.g., titles, sounds, particles) added by users.
/// </summary>
public sealed class MsgFunction : BaseFunction
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MsgFunction"/> class.
    /// </summary>
    /// <param name="file">The physical file information.</param>
    /// <param name="parsedFunction">The parsed McFunction AST data.</param>
    /// <param name="bacapAdvancement">The BACAP advancement model associated with this function.</param>
    internal MsgFunction(FileInfo file, McFunction parsedFunction, BacapAdvancement bacapAdvancement)
        : base(file, parsedFunction, bacapAdvancement)
    {
    }

    /// <summary>
    /// Generates or updates the tellraw announcement message.
    /// Identifies the existing announcement by looking for standard BACAP placeholders
    /// to preserve other custom commands in the file.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the required tier is missing in the configuration.</exception>
    [PublicAPI]
    public override void Update()
    {

        var entry = BacapAdvancement.Tier.GetDefaultMessage();

        var newCommand = CreateMessage(
            rootTranslationKey: entry.TranslationKey,
            advancementTitle: BacapAdvancement.TitleText,
            titleColor: entry.TitleColor,
            advancementDesc: BacapAdvancement.CleanDescriptionText,
            descColor: entry.DescriptionColor,
            tab: BacapAdvancement.Tab
        );

        // Convert the command to whatever line model Function.Lines expects
        var newLine = newCommand.ToLine();

        // Find the existing tellraw command by checking the AST nodes directly.
        var existingIndex = Function.Lines.FindIndex(line =>
        {
            if (line is not ExecutableLine { Command: TellrawCommand tellCmd })
                return false;

            if (tellCmd.Target != Selector.AllPlayers)
                return false;

            // Safely check the translatable component properties directly via AST
            return tellCmd.Message is TranslatableComponent trans &&
                   trans.Translate.Contains("%1$s", StringComparison.OrdinalIgnoreCase);

        });

        // Replace or Insert
        if (existingIndex >= 0)
            // Replace the old message, leaving all other custom commands (titles, # comments) untouched
            Function.Lines[existingIndex] = newLine;
        else
            // If not found, it's safer to insert the announcement at the very top of the file
            Function.Lines.Insert(0, newLine);
    }

    private static TellrawCommand CreateMessage(
        string rootTranslationKey,
        string advancementTitle,
        string titleColor,
        string advancementDesc,
        string descColor,
        BacapTab tab)
    {
        var hoverText = new TranslatableComponent(
            Translate: advancementTitle,
            Style: new TextStyle(Color: titleColor),
            Extra:
            [
                new PlainTextComponent("\n"),
                new TranslatableComponent(advancementDesc, Style: new TextStyle(Color: descColor)),
                new PlainTextComponent("\n\n"),
                new TranslatableComponent(
                    Translate: "%1$s tab",
                    Style: new TextStyle(Color: "gray", Italic: true),
                    With: [new TranslatableComponent(tab.DisplayName)]
                )
            ]
        );

        var advancementComponent = new TranslatableComponent(
            Translate: advancementTitle,
            Style: new TextStyle(
                Color: titleColor,
                HoverEvent: new ShowTextHoverEvent(hoverText)
            )
        );

        var rootMessage = new TranslatableComponent(
            Translate: rootTranslationKey,
            With:
            [
                new SelectorComponent(Selector.SelectedPlayer),
                new PlainTextComponent("[", Style: new TextStyle(Color: titleColor)),
                advancementComponent,
                new PlainTextComponent("]", Style: new TextStyle(Color: titleColor))
            ]
        );

        return new TellrawCommand(Target: Selector.AllPlayers, Message: rootMessage);
    }
}