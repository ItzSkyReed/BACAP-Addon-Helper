using Core.DataComponents.Interfaces;
using Core.DataComponents.Models.Providers;
using Core.SNBT;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

namespace Core.DataComponents.Components;

/// <summary>
/// Configures an item's capability to be consumed as fuel inside brewing stands (<c>minecraft:brewing_fuel</c>).
/// </summary>
/// <param name="Uses">The number of infusions fueled by consuming this item. Either a fixed number or a number provider ID.</param>
/// <param name="SpeedMultiplier">Optional brewing speed multiplier applied while this fuel is active. Defaults to 1.0.</param>
[UsedImplicitly]
public record BrewingFuelComponent(
    FloatProvider Uses,
    FloatProvider? SpeedMultiplier = null
) : ICompoundComponent<BrewingFuelComponent>
{
    /// <summary>
    /// The default speed multiplier applied during brewing infusions.
    /// </summary>
    public const float DefaultSpeedMultiplier = 1.0f;

    /// <inheritdoc/>
    public static string ComponentId => "minecraft:brewing_fuel";

    /// <summary>
    /// Parses a <see cref="BrewingFuelComponent"/> from an SNBT compound node representation.
    /// </summary>
    /// <param name="compound">The SNBT compound node containing the brewing fuel configuration.</param>
    /// <returns>A populated <see cref="BrewingFuelComponent"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="compound"/> is missing the required <c>uses</c> tag.</exception>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("{uses: \"minecraft:brewing/uses_default\", speed_multiplier: 1.5f}");
    /// var brewingFuel = BrewingFuelComponent.Parse(node);
    /// </code>
    /// </example>
    public static BrewingFuelComponent Parse(SnbtCompound compound)
    {
        if (compound.GetNode("uses") is not { } usesNode)
            throw new ArgumentException("Brewing fuel component requires a 'uses' tag.", nameof(compound));

        var uses = FloatProvider.Parse(usesNode);

        var speedMultiplier = compound.GetNode("speed_multiplier") switch
        {
            { } speedNode => FloatProvider.Parse(speedNode),
            null => null
        };

        return new BrewingFuelComponent(uses, speedMultiplier);
    }

    /// <summary>
    /// Serializes the component into an SNBT compound node, omitting <c>speed_multiplier</c> when matching the default value.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the brewing fuel compound.</returns>
    public ISnbtNode ToSnbt()
    {
        var builder = Snbt.Compound()
            .Put("uses", Uses.ToSnbt());

        if (SpeedMultiplier != null && !SpeedMultiplier.IsFixed(DefaultSpeedMultiplier))
            builder.Put("speed_multiplier", SpeedMultiplier.ToSnbt());

        return builder.Build();
    }
}