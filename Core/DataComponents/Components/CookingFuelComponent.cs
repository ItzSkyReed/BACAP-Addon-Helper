using Core.DataComponents.Interfaces;
using Core.DataComponents.Models.Providers;
using Core.SNBT;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

namespace Core.DataComponents.Components;

/// <summary>
/// Configures an item's capability to be consumed as smelting fuel in furnaces, blast furnaces, and smokers (<c>minecraft:cooking_fuel</c>).
/// </summary>
/// <param name="BurnTime">The duration in game ticks this fuel burns for. Either a fixed numeric duration or a number provider ID.</param>
/// <param name="SpeedMultiplier">Optional smelting speed multiplier applied while this fuel is burning. Defaults to 1.0.</param>
[UsedImplicitly]
public record CookingFuelComponent(
    FloatProvider BurnTime,
    FloatProvider? SpeedMultiplier = null
) : ICompoundComponent<CookingFuelComponent>
{
    /// <summary>
    /// The default smelting speed multiplier applied during item cooking.
    /// </summary>
    public const float DefaultSpeedMultiplier = 1.0f;

    /// <inheritdoc/>
    public static string ComponentId => "minecraft:cooking_fuel";

    /// <summary>
    /// Initializes a new instance of the <see cref="CookingFuelComponent"/> record with fixed numeric values.
    /// </summary>
    /// <param name="burnTime">The fixed burn time in game ticks.</param>
    /// <param name="speedMultiplier">The optional fixed smelting speed multiplier.</param>
    public CookingFuelComponent(float burnTime, float speedMultiplier = DefaultSpeedMultiplier)
        : this(new FloatProvider(Value: burnTime), new FloatProvider(Value: speedMultiplier))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CookingFuelComponent"/> record using number provider identifiers.
    /// </summary>
    /// <param name="burnTimeProviderId">The namespaced resource identifier of the burn time provider.</param>
    /// <param name="speedMultiplierProviderId">Optional namespaced resource identifier of the speed multiplier provider.</param>
    public CookingFuelComponent(string burnTimeProviderId, string? speedMultiplierProviderId = null)
        : this(
            new FloatProvider(ProviderId: burnTimeProviderId),
            speedMultiplierProviderId != null ? new FloatProvider(ProviderId: speedMultiplierProviderId) : null)
    {
    }

    /// <summary>
    /// Parses a <see cref="CookingFuelComponent"/> from an SNBT compound node representation.
    /// </summary>
    /// <param name="compound">The SNBT compound node containing the cooking fuel configuration.</param>
    /// <returns>A populated <see cref="CookingFuelComponent"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="compound"/> is missing the required <c>burn_time</c> tag.</exception>
    /// <example>
    /// <code>
    /// // From fixed numbers:
    /// var comp1 = CookingFuelComponent.Parse(SnbtParser.Parse("{burn_time: 40.0f, speed_multiplier: 2.0f}"));
    ///
    /// // From provider IDs:
    /// var comp2 = CookingFuelComponent.Parse(SnbtParser.Parse("{burn_time: \"minecraft:cooking/time_coal\", speed_multiplier: \"minecraft:cooking/speed_default\"}"));
    /// </code>
    /// </example>
    public static CookingFuelComponent Parse(SnbtCompound compound)
    {
        if (compound.GetNode("burn_time") is not { } burnTimeNode)
            throw new ArgumentException("Cooking fuel component requires a 'burn_time' tag.", nameof(compound));

        var burnTime = FloatProvider.Parse(burnTimeNode);

        var speedMultiplier = compound.GetNode("speed_multiplier") switch
        {
            { } speedNode => FloatProvider.Parse(speedNode),
            null => null
        };

        return new CookingFuelComponent(burnTime, speedMultiplier);
    }

    /// <summary>
    /// Serializes the component into an SNBT compound node, omitting <c>speed_multiplier</c> when matching the default value.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the cooking fuel compound.</returns>
    public ISnbtNode ToSnbt()
    {
        var builder = Snbt.Compound()
            .Put("burn_time", BurnTime.ToSnbt());

        if (SpeedMultiplier != null && !SpeedMultiplier.IsFixed(DefaultSpeedMultiplier))
            builder.Put("speed_multiplier", SpeedMultiplier.ToSnbt());

        return builder.Build();
    }
}