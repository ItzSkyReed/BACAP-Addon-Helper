using Core.DataComponents.Interfaces;
using Core.SNBT;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

namespace Core.DataComponents.Components;

/// <summary>
/// Configures the swing animation type and timing played when an entity swings the item (<c>minecraft:swing_animation</c>).
/// </summary>
/// <remarks>
/// Legacy component. In Java Edition 26.3+, this behavior is split into
/// <see cref="AttackAnimationComponent"/> (<c>minecraft:attack_animation</c>) and
/// <see cref="InteractAnimationComponent"/> (<c>minecraft:interact_animation</c>).
/// </remarks>
/// <param name="Type">The animation type. Valid values are <c>"whack"</c>, <c>"stab"</c>, or <c>"none"</c>. Defaults to <c>"whack"</c>.</param>
/// <param name="Duration">The animation duration in game ticks (non-negative integer). Defaults to 6 ticks.</param>
[UsedImplicitly]
public record SwingAnimationComponent(
    string Type = SwingAnimationComponent.Whack,
    int Duration = SwingAnimationComponent.DefaultDuration
) : ICompoundComponent<SwingAnimationComponent>
{
    /// <summary>
    /// The default downward/cleaving swing animation.
    /// </summary>
    public const string Whack = "whack";

    /// <summary>
    /// Thrusting swing animation.
    /// </summary>
    public const string Stab = "stab";

    /// <summary>
    /// Disables any swing animation.
    /// </summary>
    public const string None = "none";

    /// <summary>
    /// The default animation duration in game ticks.
    /// </summary>
    public const int DefaultDuration = 6;

    /// <inheritdoc/>
    public static string ComponentId => "minecraft:swing_animation";

    /// <summary>
    /// Parses a <see cref="SwingAnimationComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="compound">The SNBT node to parse, which must be an <see cref="SnbtCompound"/>.</param>
    /// <returns>A populated <see cref="SwingAnimationComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("{type: \"stab\", duration: 10}");
    /// var swingAnimation = SwingAnimationComponent.Parse(node);
    /// </code>
    /// </example>
    public static SwingAnimationComponent Parse(SnbtCompound compound)
    {
        return new SwingAnimationComponent(
            Type: compound.GetString("type", Whack),
            Duration: compound.GetInt("duration", DefaultDuration)
        );
    }

    /// <summary>
    /// Serializes the component into an SNBT compound node, omitting fields that match default values.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the swing animation configuration compound.</returns>
    public ISnbtNode ToSnbt() => Snbt.Compound()
        .PutOptional("type", Type, Whack)
        .PutOptional("duration", Duration, DefaultDuration)
        .Build();
}