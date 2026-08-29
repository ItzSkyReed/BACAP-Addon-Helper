using JetBrains.Annotations;
using Core.Commands.Impl;
using Core.Commands.Models;
using Core.McFunctions.Models;
using Core.McFunctions.Models.Extensions;
using Core.TextComponents.Components;
using Core.TextComponents.Events;
using Core.TextComponents.Models;

namespace BacapGenerator.Models.Advancements.Functions;

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
    /// <param name="bacapAdvancement">The BACAP advancement model associated with this function.</param>
    public MsgFunction(FileInfo file, BacapAdvancement bacapAdvancement)
        : base(file, bacapAdvancement)
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
        var settings = BacapAdvancement.Datapack.Settings.AdvancementMessageSettings;

        if (!settings.Entries.TryGetValue(BacapAdvancement.Tier, out var entry))
        {
            throw new ArgumentException(
                $"Tier '{BacapAdvancement.Tier}' not found in the configuration file to generate advancement messages.");
        }

        // Build the new command
        var newCommand = CreateMessage(
            rootTranslationKey: entry.TranslationKey,
            advancementTitle: BacapAdvancement.TitleText,
            titleColor: entry.TitleColor,
            advancementDesc: "advancement.description.placeholder", // Replace with actual description key/text
            descColor: entry.DescriptionColor,
            advancementTab: BacapAdvancement.Tab
        );

        // Convert the command to whatever line model Function.Lines expects
        var newLine = newCommand.ToLine();

        // Find the existing tellraw command by checking the AST nodes.
        // We look for a tellraw targeted at @a that contains "%1$s" (the player placeholder).
        var existingIndex = Function.Lines.FindIndex(line =>
            line is ExecutableLine { Command: TellrawCommand tellCmd } &&
            tellCmd.Target == Selector.AllPlayers &&
            tellCmd.Message.ToJson().Contains("%1$s")
        );

        // Replace or Insert
        if (existingIndex >= 0)
        {
            // Replace the old message, leaving all other custom commands (titles, # comments) untouched
            Function.Lines[existingIndex] = newLine;
        }
        else
        {
            // If not found, it's safer to insert the announcement at the very top of the file
            Function.Lines.Insert(0, newLine);
        }
    }

    private static TellrawCommand CreateMessage(
        string rootTranslationKey,
        string advancementTitle,
        string titleColor,
        string advancementDesc,
        string descColor,
        BacapAdvancementTab advancementTab)
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
                    With: [new TranslatableComponent(advancementTab.DisplayName)]
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