using Core.DataComponents.Interfaces;
using Core.SNBT;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

namespace Core.DataComponents.Components;

/// <summary>
/// Configures the swing animation type and timing played when an entity interacts with a block or entity using the item (<c>minecraft:interact_animation</c>).
/// </summary>
/// <param name="Type">The interaction animation type. Valid values are <c>"whack"</c>, <c>"stab"</c>, or <c>"none"</c>. Defaults to <c>"whack"</c>.</param>
/// <param name="Duration">The animation duration in game ticks (non-negative integer). Defaults to 6 ticks (0.3 seconds).</param>
[UsedImplicitly]
public record InteractAnimationComponent(
    string Type = InteractAnimationComponent.Whack,
    int Duration = InteractAnimationComponent.DefaultDuration
) : ICompoundComponent<InteractAnimationComponent>
{
    /// <summary>
    /// The default downward/cleaving swing animation.
    /// </summary>
    public const string Whack = "whack";

    /// <summary>
    /// Thrusting interaction animation.
    /// </summary>
    public const string Stab = "stab";

    /// <summary>
    /// Disables any interaction swing animation.
    /// </summary>
    public const string None = "none";

    /// <summary>
    /// The default interaction animation duration in game ticks (0.3 seconds).
    /// </summary>
    public const int DefaultDuration = 6;

    /// <inheritdoc/>
    public static string ComponentId => "minecraft:interact_animation";

    /// <summary>
    /// Parses an <see cref="InteractAnimationComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="compound">The SNBT node to parse, which must be an <see cref="SnbtCompound"/>.</param>
    /// <returns>A populated <see cref="InteractAnimationComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("{type: \"stab\", duration: 8}");
    /// var interactAnimation = InteractAnimationComponent.Parse(node);
    /// </code>
    /// </example>
    public static InteractAnimationComponent Parse(SnbtCompound compound)
    {
        return new InteractAnimationComponent(
            Type: compound.GetString("type", Whack),
            Duration: compound.GetInt("duration", DefaultDuration)
        );
    }

    /// <summary>
    /// Serializes the component into an SNBT compound node, omitting fields that match default values.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the interaction animation configuration compound.</returns>
    public ISnbtNode ToSnbt() => Snbt.Compound()
        .PutOptional("type", Type, Whack)
        .PutOptional("duration", Duration, DefaultDuration)
        .Build();
}