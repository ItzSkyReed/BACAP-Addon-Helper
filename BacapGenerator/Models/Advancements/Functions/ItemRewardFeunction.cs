using BacapGenerator.Utils;
using JetBrains.Annotations;
using Core.Commands.Impl;
using Core.Commands.Models;
using Core.Items;
using Core.McFunctions.Models;
using Core.McFunctions.Models.Interfaces;
using Core.TextComponents.Components;
using Core.TextComponents.Models;

namespace BacapGenerator.Models.Advancements.Functions;

/// <summary>
/// Represents the item reward function file.
/// Manages multiple item rewards (give) and their personal local announcements (tellraw @s),
/// while preserving any custom commands in the file.
/// </summary>
public sealed class ItemRewardFunction : BaseFunction
{
    /// <summary>
    /// Gets or sets the list of items to reward the player.
    /// Setting this property automatically updates the underlying function file lines.
    /// <exception cref="KeyNotFoundException">When item id is not found in registry to get its translation key</exception>
    /// </summary>
    [PublicAPI]
    public IReadOnlyList<ItemStack> RewardItems { get; private set; } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemRewardFunction"/> class.
    /// </summary>
    /// <param name="file">The physical file information.</param>
    /// <param name="bacapAdvancement">The BACAP advancement model associated with this function.</param>
    public ItemRewardFunction(FileInfo file, BacapAdvancement bacapAdvancement)
        : base(file, bacapAdvancement)
    {
        ParseExistingItems();
    }

    /// <summary>
    /// Generates or updates the item reward commands (/give @s) and local announcements (/tellraw @s).
    /// Preserves any unrelated custom commands (like comments or particles) in the file.
    /// <exception cref="KeyNotFoundException">When item id is not found in registry to get its translation key</exception>
    /// </summary>
    [PublicAPI]
    public override void Update()
    {
        var linesToRemove = new List<IMcFunctionLine>();
        var insertIndex = -1;

        // Identify the existing item reward block
        for (var i = 0; i < Function.Lines.Count; i++)
        {
            var line = Function.Lines[i];
            var isRewardBlockLine = false;

            switch (line)
            {
                case ExecutableLine { Command: GiveCommand giveCmd } when
                    giveCmd.Target == Selector.SelectedPlayer:
                    isRewardBlockLine = true;
                    break;

                case ExecutableLine { Command: TellrawCommand tellCmd } when
                    tellCmd.Target == Selector.SelectedPlayer:
                {
                    var json = tellCmd.Message.ToJson();

                    if (json.Contains(" +") && json.Contains("green"))
                        isRewardBlockLine = true;

                    break;
                }
            }

            if (!isRewardBlockLine)
                continue;

            linesToRemove.Add(line);

            if (insertIndex == -1)
                insertIndex = i;
        }

        // Clean up old reward lines
        foreach (var line in linesToRemove)
            Function.Lines.Remove(line);

        // Generate new lines for the updated items
        var newLines = new List<IMcFunctionLine>();
        foreach (var item in RewardItems)
        {
            var giveCmd = new GiveCommand(Selector.SelectedPlayer, item);
            var tellrawCmd = CreateItemMessage(item);

            newLines.Add(new ExecutableLine(giveCmd));
            newLines.Add(new ExecutableLine(tellrawCmd));
        }

        // Insert the new lines back into the AST
        if (insertIndex == -1)
            // If there were no items before, append to the end of the file
            Function.Lines.AddRange(newLines);
        else
            // Insert exactly where the old block used to be
            Function.Lines.InsertRange(insertIndex, newLines);
    }

    /// <summary>
    /// Scans the currently loaded function lines to extract existing item rewards.
    /// Automatically sums up amounts for consecutive identical items (e.g., 3x 64 rockets -> 192).
    /// </summary>
    private void ParseExistingItems()
    {
        var parsedItems = new List<ItemStack>();

        foreach (var line in Function.Lines)
        {
            if (line is not ExecutableLine { Command: GiveCommand giveCmd } || giveCmd.Target != Selector.SelectedPlayer)
                continue;

            // Fallback to item.Count if Count was omitted in the give command
            var amount = giveCmd.Count ?? giveCmd.Item.Count;
            var lastItem = parsedItems.LastOrDefault();

            // If it's the exact same item ID and NBT/Components, sum the count
            if (lastItem != null &&
                lastItem.Id == giveCmd.Item.Id &&
                lastItem.Components.ToSnbt().ToString() == giveCmd.Item.Components.ToSnbt().ToString())
                parsedItems[^1] = lastItem with { Count = lastItem.Count + amount };
            else
                // Add as a new item
                parsedItems.Add(giveCmd.Item with { Count = amount });
        }

        RewardItems = parsedItems;
    }

    /// <summary>
    /// Creates the tellraw command for a specific item reward.
    /// Example: tellraw @s {"color":"green","text":" +16 ","extra":[{"translate":"item.minecraft.egg"}]}
    /// <exception cref="KeyNotFoundException">When item id is not found in registry to get its translation key</exception>
    /// </summary>
    private TellrawCommand CreateItemMessage(ItemStack item)
    {
        var itemIdWithoutNamespace = MinecraftUtils.StripNamespace(item.Id);
        if (!BacapAdvancement.Datapack.MinecraftData.Items.TryGetValue(itemIdWithoutNamespace, out var itemRegistryEntry))
            throw new KeyNotFoundException($"Item '{itemIdWithoutNamespace}' not found in Item registry.");

        var rootMessage = new PlainTextComponent(
            Text: $" +{item.Count} ",
            Style: new TextStyle(Color: "green"),
            Extra:
            [
                new TranslatableComponent(itemRegistryEntry.TranslationKey)
            ]
        );

        return new TellrawCommand(Target: Selector.SelectedPlayer, Message: rootMessage);
    }
}