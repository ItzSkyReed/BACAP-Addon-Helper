 using Core.SNBT;
using Core.SNBT.Nodes;

using Core.SNBT.Interfaces;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Represents nutritional and saturation properties restored when an item is consumed (<c>minecraft:food</c>).
/// </summary>
/// <remarks>
/// When combined with <c>minecraft:consumable</c>, this component allows mobs like foxes, wolves, and cats
/// to recognize and consume the item for nourishment or healing.
/// </remarks>
/// <param name="Nutrition">The number of hunger/food points restored. Must be non-negative.</param>
/// <param name="Saturation">The saturation modifier value restored when eaten.</param>
/// <param name="CanAlwaysEat">Whether this item can be consumed even when the player's hunger bar is completely full. Defaults to <see langword="false"/>.</param>
[UsedImplicitly]
public record FoodComponent(
    int Nutrition,
    float Saturation,
    bool CanAlwaysEat = false
) : ICompoundComponent<FoodComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:food";

    /// <summary>
    /// Parses a <see cref="FoodComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="compound">The SNBT node to parse, which must be an <see cref="SnbtCompound"/>.</param>
    /// <returns>A populated <see cref="FoodComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("{nutrition: 3, saturation: 1.0f, can_always_eat: 1b}");
    /// var component = FoodComponent.Parse(node);
    /// </code>
    /// </example>
    public static FoodComponent Parse(SnbtCompound compound)
    {

        return new FoodComponent(
            Nutrition: compound.GetInt("nutrition"),
            Saturation: compound.GetFloat("saturation"),
            CanAlwaysEat: compound.GetBool("can_always_eat")
        );
    }

    /// <summary>
    /// Serializes the component into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the food properties compound.</returns>
    public ISnbtNode ToSnbt() => Snbt.Compound()
        .Put("nutrition", Nutrition)
        .Put("saturation", Saturation)
        .PutOptional("can_always_eat", CanAlwaysEat, false)
        .Build();
}