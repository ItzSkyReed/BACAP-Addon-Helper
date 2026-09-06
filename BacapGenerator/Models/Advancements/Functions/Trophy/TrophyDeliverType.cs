namespace BacapGenerator.Models.Advancements.Functions.Trophy;

/// <summary>
/// Specifies how a trophy reward is delivered to the player.
/// </summary>
public enum TrophyDeliveryType
{
    /// <summary>
    /// Delivered directly to the player's inventory via the <c>/give</c> command.
    /// </summary>
    Inventory,

    /// <summary>
    /// Spawned in the world at the death location via the <c>/summon minecraft:item</c> command.
    /// </summary>
    DeathLocation
}