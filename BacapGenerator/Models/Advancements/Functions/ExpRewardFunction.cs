using JetBrains.Annotations;
using Core.Commands.Impl;
using Core.Commands.Models;
using Core.McFunctions.Models;
using Core.TextComponents.Components;
using Core.TextComponents.Models;

namespace BacapGenerator.Models.Advancements.Functions;

/// <summary>
/// Represents the experience reward function file.
/// Manages both the /experience grant command and its corresponding /tellraw announcement,
/// while preserving any custom commands in the file.
/// </summary>
public sealed class ExpRewardFunction : BaseFunction
{
    private int _experienceAmount;

    /// <summary>
    /// Gets or sets the amount of experience to reward.
    /// Setting this property automatically updates the underlying function file lines.
    /// </summary>
    [PublicAPI]
    public int ExperienceAmount
    {
        get => _experienceAmount;
        set
        {
            if (_experienceAmount == value)
                return;

            _experienceAmount = value;
            Update();
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpRewardFunction"/> class
    /// and attempts to parse the existing experience amount from the file.
    /// </summary>
    /// <param name="file">The physical file information.</param>
    /// <param name="bacapAdvancement">The BACAP advancement model associated with this function.</param>
    public ExpRewardFunction(FileInfo file, BacapAdvancement bacapAdvancement)
        : base(file, bacapAdvancement)
    {
        ParseExistingExperience();
    }

    /// <summary>
    /// Generates or updates the experience reward and tellraw commands.
    /// Identifies the existing commands by looking for standard heuristics
    /// to preserve other custom commands in the file.
    /// </summary>
    [PublicAPI]
    public override void Update()
    {
        // Build new commands
        var xpCommand = new ExperienceCommand(ExperienceAction.Add, Selector.SelectedPlayer, _experienceAmount);
        var tellrawCommand = CreateExperienceMessage(_experienceAmount);

        var xpLine = new ExecutableLine(xpCommand);
        var tellrawLine = new ExecutableLine(tellrawCommand);

        // Find existing command indices directly through the AST!
        var xpIndex = Function.Lines.FindIndex(line =>
            line is ExecutableLine { Command: ExperienceCommand { Action: ExperienceAction.Add } expCmd } &&
            expCmd.Target == Selector.SelectedPlayer
        );

        var tellrawIndex = Function.Lines.FindIndex(line =>
            line is ExecutableLine { Command: TellrawCommand tellCmd } &&
            tellCmd.Target == Selector.SelectedPlayer &&
            tellCmd.Message.ToJson().Contains("Experience", StringComparison.OrdinalIgnoreCase)
        );

        // Replace or Insert XP Command
        if (xpIndex >= 0)
            Function.Lines[xpIndex] = xpLine;
        else
        {
            Function.Lines.Insert(0, xpLine);
            xpIndex = 0;
        }

        // Replace or Insert Tellraw Command
        if (tellrawIndex >= 0)
            Function.Lines[tellrawIndex] = tellrawLine;
        else
            Function.Lines.Insert(xpIndex + 1, tellrawLine);
    }

    /// <summary>
    /// Creates the specific tellraw command for the experience reward announcement.
    /// Example: tellraw @s {"color":"blue","text":" +[exp],"extra":[{"translate":"Experience"}]}
    /// </summary>
    private static TellrawCommand CreateExperienceMessage(int amount)
    {
        var rootMessage = new PlainTextComponent(
            Text: $" +{amount} ",
            Style: new TextStyle(Color: "blue"),
            Extra:
            [
                new TranslatableComponent("Experience")
            ]
        );

        return new TellrawCommand(Target: Selector.SelectedPlayer, Message: rootMessage);
    }

    /// <summary>
    /// Scans the currently loaded function lines to extract the existing experience amount
    /// </summary>
    private void ParseExistingExperience()
    {
        foreach (var line in Function.Lines)
        {
            if (line is not ExecutableLine { Command: ExperienceCommand { Action: ExperienceAction.Add } expCmd } ||
                expCmd.Target != Selector.SelectedPlayer ||
                !expCmd.Amount.HasValue) continue;
            _experienceAmount = expCmd.Amount.Value;
            return;
        }

        _experienceAmount = 0;
    }
}