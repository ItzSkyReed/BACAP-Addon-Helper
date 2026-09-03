using Core.DataComponents.Interfaces;
using Core.SNBT;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

namespace Core.DataComponents.Components;

/// <summary>
/// Configures an item to be picked up and consumed by villagers (<c>minecraft:villager_food</c>).
/// </summary>
/// <param name="Nutrition">A positive integer representing how many nutrition points the item supplies to villagers.</param>
[UsedImplicitly]
public record VillagerFoodComponent(
    int Nutrition
) : ICompoundComponent<VillagerFoodComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:villager_food";

    /// <summary>
    /// Parses a <see cref="VillagerFoodComponent"/> from an SNBT compound node representation.
    /// </summary>
    /// <param name="compound">The SNBT compound node containing villager food properties.</param>
    /// <returns>A populated <see cref="VillagerFoodComponent"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="compound"/> is missing the required <c>nutrition</c> integer tag.</exception>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("{nutrition: 12}");
    /// var villagerFood = VillagerFoodComponent.Parse(node);
    /// </code>
    /// </example>
    public static VillagerFoodComponent Parse(SnbtCompound compound)
    {
        var nutrition = compound.GetOptionalInt("nutrition")
            ?? throw new ArgumentException("Villager food component requires a 'nutrition' integer tag.", nameof(compound));

        return new VillagerFoodComponent(nutrition);
    }

    /// <summary>
    /// Serializes the component into an SNBT compound node containing the <c>nutrition</c> field.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the villager food compound.</returns>
    public ISnbtNode ToSnbt() => Snbt.Compound()
        .Put("nutrition", Nutrition)
        .Build();

    /// <summary>
    /// Implicitly converts an integer nutrition value into a <see cref="VillagerFoodComponent"/>.
    /// </summary>
    /// <param name="nutrition">The nutrition points.</param>
    public static implicit operator VillagerFoodComponent(int nutrition) => new(nutrition);

    /// <summary>
    /// Implicitly converts a <see cref="VillagerFoodComponent"/> to its underlying nutrition value.
    /// </summary>
    /// <param name="component">The component instance.</param>
    public static implicit operator int(VillagerFoodComponent component) => component.Nutrition;
}