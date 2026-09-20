using Core.SNBT;
using Core.SNBT.Nodes;

using Core.SNBT.Interfaces;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Applies a usage cooldown duration to the item and any items sharing its cooldown group (<c>minecraft:use_cooldown</c>).
/// </summary>
/// <param name="Seconds">The use cooldown duration in seconds.</param>
/// <param name="CooldownGroup">Optional resource location identifier to group cooldowns across multiple item types.</param>
[UsedImplicitly]
public record UseCooldownComponent(
    float Seconds,
    string? CooldownGroup = null
) : ICompoundComponent<UseCooldownComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:use_cooldown";

    /// <summary>
    /// Parses a <see cref="UseCooldownComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="compound">The SNBT node to parse, which must be an <see cref="SnbtCompound"/>.</param>
    /// <returns>A populated <see cref="UseCooldownComponent"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="compound"/> is not an <see cref="SnbtCompound"/>.</exception>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("{seconds: 10.0f, cooldown_group: \"custom:pearls\"}");
    /// var component = UseCooldownComponent.Parse(node);
    /// </code>
    /// </example>
    public static UseCooldownComponent Parse(SnbtCompound compound)
    {

        return new UseCooldownComponent(
            Seconds: compound.GetFloat("seconds"),
            CooldownGroup: compound.GetOptionalString("cooldown_group")
        );
    }

    /// <summary>
    /// Serializes the component into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the use cooldown compound.</returns>
    public ISnbtNode ToSnbt() => Snbt.Compound()
        .Put("seconds", Seconds)
        .PutOptional("cooldown_group", CooldownGroup)
        .Build();

    /// <summary>
    /// Implicitly converts a float duration in seconds into a <see cref="UseCooldownComponent"/>.
    /// </summary>
    /// <param name="seconds">The cooldown duration in seconds.</param>
    public static implicit operator UseCooldownComponent(float seconds) => new(seconds);
}