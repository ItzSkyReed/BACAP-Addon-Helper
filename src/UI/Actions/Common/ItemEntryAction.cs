using Core.Items;
using Core.Registries;
using UI.Interfaces;

namespace UI.Actions.Common;

/// <summary>
/// Reusable menu action wrapper representing a Minecraft <see cref="ItemStack"/> in selection menus.
/// Automatically handles formatting via <see cref="ItemPromptUtils.FormatItemStack"/>.
/// </summary>
/// <param name="item">The underlying item stack instance.</param>
/// <param name="mcData">The Minecraft registry data used for resolving display names.</param>
/// <example>
/// <code>
/// var entryAction = new ItemEntryAction(itemStack, mcData);
/// </code>
/// </example>
public class ItemEntryAction(ItemStack item, MinecraftData mcData) : ITuiAction
{
    public ItemStack Item { get; } = item;

    public string Title => ItemPromptUtils.FormatItemStack(Item, mcData);

    public Task ExecuteAsync() => Task.CompletedTask;
}