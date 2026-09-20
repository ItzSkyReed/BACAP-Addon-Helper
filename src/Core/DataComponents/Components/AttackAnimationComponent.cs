using Core.DataComponents.Interfaces;
using Core.SNBT;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

namespace Core.DataComponents.Components;

/// <summary>
/// Configures the swing animation type and timing played when an entity attacks with the item (<c>minecraft:attack_animation</c>).
/// </summary>
/// <param name="Type">The swing animation type. Valid values are <c>"whack"</c>, <c>"stab"</c>, or <c>"none"</c>. Defaults to <c>"whack"</c>.</param>
/// <param name="Duration">The animation duration in game ticks (non-negative integer). Defaults to 6 ticks (0.3 seconds).</param>
[UsedImplicitly]
public record AttackAnimationComponent(
    string Type = AttackAnimationComponent.Whack,
    int Duration = AttackAnimationComponent.DefaultDuration
) : ICompoundComponent<AttackAnimationComponent>
{
    /// <summary>
    /// The default downward/cleaving swing animation.
    /// </summary>
    [PublicAPI]
    public const string Whack = "whack";

    /// <summary>
    /// Thrusting attack animation (e.g., used by spears).
    /// </summary>
    [PublicAPI]
    public const string Stab = "stab";

    /// <summary>
    /// Disables any attack swing animation.
    /// </summary>
    [PublicAPI]
    public const string None = "none";

    /// <summary>
    /// The default animation duration in game ticks (0.3 seconds).
    /// </summary>
    [PublicAPI]
    public const int DefaultDuration = 6;

    /// <inheritdoc/>
    [PublicAPI]
    public static string ComponentId => "minecraft:attack_animation";

    /// <summary>
    /// Parses an <see cref="AttackAnimationComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="compound">The SNBT node to parse, which must be an <see cref="SnbtCompound"/>.</param>
    /// <returns>A populated <see cref="AttackAnimationComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("{type: \"stab\", duration: 10}");
    /// var attackAnimation = AttackAnimationComponent.Parse(node);
    /// </code>
    /// </example>
    [PublicAPI]
    public static AttackAnimationComponent Parse(SnbtCompound compound)
    {
        return new AttackAnimationComponent(
            Type: compound.GetString("type", Whack),
            Duration: compound.GetInt("duration", DefaultDuration)
        );
    }

    /// <summary>
    /// Serializes the component into an SNBT compound node, omitting fields that match default values.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the attack animation configuration compound.</returns>
    [PublicAPI]
    public ISnbtNode ToSnbt() => Snbt.Compound()
        .PutOptional("type", Type, Whack)
        .PutOptional("duration", Duration, DefaultDuration)
        .Build();
}