using BacapGenerator.Utils;
using JetBrains.Annotations;
using Core.Commands.Impl;
using Core.Commands.Models;
using Core.DataComponents.Components;
using Core.Items;
using Core.McFunctions.Models;
using Core.McFunctions.Models.Interfaces;
using Core.TextComponents.Components;
using Core.TextComponents.Models;

namespace BacapGenerator.Models.Advancements.Functions.Trophy;

/// <summary>
/// Represents the trophy reward function file.
/// Manages trophy item rewards and their golden local announcements (tellraw @s).
/// </summary>
public sealed class TrophyRewardFunction : BaseFunction
{
    private readonly List<TrophyReward> _trophies = [];

    [PublicAPI] public IReadOnlyList<TrophyReward> Trophies => _trophies;

    public TrophyRewardFunction(FileInfo file, BacapAdvancement bacapAdvancement)
        : base(file, bacapAdvancement)
    {
        ParseExistingTrophies();
    }

    /// <summary>
    /// Adds a new trophy reward to the function if it doesn't already exist.
    /// Duplicates (matching ID and components) are silently ignored.
    /// </summary>
    /// <param name="trophy">The trophy reward to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when the provided trophy is null.</exception>
    [PublicAPI]
    public void AddTrophy(TrophyReward trophy)
    {
        ArgumentNullException.ThrowIfNull(trophy);

        // Check if an identical trophy already exists
        var isDuplicate = _trophies.Any(t =>
            t.Item.Id == trophy.Item.Id &&
            t.Item.Components.ToSnbtString() == trophy.Item.Components.ToSnbtString());

        if (isDuplicate)
            return;

        _trophies.Add(trophy);
        Update();
    }

    /// <summary>
    /// Adds a collection of trophy rewards to the function, ignoring any duplicates.
    /// This is more efficient than calling <see cref="AddTrophy"/> multiple times.
    /// </summary>
    /// <param name="trophies">The collection of trophies to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when the provided collection is null.</exception>
    [PublicAPI]
    public void AddTrophies(IEnumerable<TrophyReward> trophies)
    {
        ArgumentNullException.ThrowIfNull(trophies);

        var hasChanges = false;

        foreach (var trophy in trophies)
        {
            var isDuplicate = _trophies.Any(t =>
                t.Item.Id == trophy.Item.Id &&
                t.Item.Components.ToSnbtString() == trophy.Item.Components.ToSnbtString());

            if (isDuplicate)
                continue;
            _trophies.Add(trophy);
            hasChanges = true;
        }

        if (hasChanges)
            Update();
    }

    [PublicAPI]
    public override void Update()
    {
        var linesToRemove = new List<IMcFunctionLine>();
        var insertIndex = -1;

        for (var i = 0; i < Function.Lines.Count; i++)
        {
            var line = Function.Lines[i];
            var isTrophyBlockLine = false;

            switch (line)
            {
                case ExecutableLine { Command: GiveCommand giveCmd } when giveCmd.Target == Selector.SelectedPlayer:
                    if (IsTrophyItem(giveCmd.Item))
                        isTrophyBlockLine = true;
                    break;

                case ExecutableLine { Command: TellrawCommand tellCmd } when tellCmd.Target == Selector.SelectedPlayer:
                {
                    if (tellCmd.Message is PlainTextComponent { Style.Color: "gold" } ptc &&
                        ptc.Text.Contains(" +"))
                    {
                        isTrophyBlockLine = true;
                    }

                    break;
                }
            }

            if (!isTrophyBlockLine)
                continue;

            linesToRemove.Add(line);

            if (insertIndex == -1)
                insertIndex = i;
        }

        foreach (var line in linesToRemove)
            Function.Lines.Remove(line);

        var newLines = new List<IMcFunctionLine>();
        foreach (var trophy in _trophies)
        {
            var giveCmd = new GiveCommand(Selector.SelectedPlayer, trophy.Item);
            var tellrawCmd = CreateTrophyMessage(trophy);

            newLines.Add(new ExecutableLine(giveCmd));
            newLines.Add(new ExecutableLine(tellrawCmd));
        }

        if (insertIndex == -1)
            Function.Lines.AddRange(newLines);
        else
            Function.Lines.InsertRange(insertIndex, newLines);
    }

    private void ParseExistingTrophies()
    {
        var parsedTrophies = new List<TrophyReward>();

        foreach (var line in Function.Lines)
        {
            if (line is not ExecutableLine { Command: GiveCommand giveCmd } ||
                giveCmd.Target != Selector.SelectedPlayer ||
                !IsTrophyItem(giveCmd.Item))
            {
                continue;
            }

            var amount = giveCmd.Count ?? giveCmd.Item.Count;
            var itemStack = giveCmd.Item with { Count = amount };

            parsedTrophies.Add(new TrophyReward(itemStack));
        }

        _trophies.Clear();
        _trophies.AddRange(parsedTrophies);
    }

    /// <summary>
    /// Checks if the item components contain the Trophy custom_data marker.
    /// </summary>
    private static bool IsTrophyItem(ItemStack item)
    {
        return item.Components.TryGet<CustomDataComponent>(out var customData)
               && customData.Tag.Tags.ContainsKey("Trophy");
    }

    /// <summary>
    /// Creates the tellraw command for a trophy.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the trophy item does not contain a valid title.</exception>
    private static TellrawCommand CreateTrophyMessage(TrophyReward trophy)
    {
        var title = trophy.TitleTranslationKey
                    ?? throw new InvalidOperationException($"Cannot create tellraw message: " +
                                                           $"Trophy item '{trophy.Item.Id}' is missing a custom name or translation key.");


        var rootMessage = new PlainTextComponent(
            Text: $" +{trophy.Item.Count} ",
            Style: new TextStyle(Color: "gold"),
            Extra:
            [
                new TranslatableComponent(title)
            ]
        );

        return new TellrawCommand(Target: Selector.SelectedPlayer, Message: rootMessage);
    }
}