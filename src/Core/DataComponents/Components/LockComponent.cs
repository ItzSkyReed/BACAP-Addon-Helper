using Core.SNBT;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Represents the lock configuration on a container item or block entity (<c>minecraft:lock</c>).
/// Holds an item predicate that players must match in their main hand to unlock and open the container.
/// </summary>
/// <param name="Predicate">The SNBT compound defining the item predicate requirements (such as matching items, count, or components).</param>
[UsedImplicitly]
public record LockComponent(
    SnbtCompound Predicate
) : ICompoundComponent<LockComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:lock";

    /// <summary>
    /// Creates a <see cref="LockComponent"/> requiring an item with a specific custom name (matching legacy lock behavior).
    /// </summary>
    /// <param name="customName">The required custom name of the key item.</param>
    /// <returns>A new <see cref="LockComponent"/> configured with a custom name predicate.</returns>
    public static LockComponent WithCustomName(string customName)
    {
        var compound = Snbt.Compound()
            .Put("components", Snbt.Compound()
                .Put("minecraft:custom_name", customName)
                .Build())
            .Build();

        return new LockComponent(compound);
    }

    /// <summary>
    /// Parses a <see cref="LockComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="compound">The SNBT node to parse, which must be an <see cref="SnbtCompound"/>.</param>
    /// <returns>A populated <see cref="LockComponent"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="compound"/> is not an <see cref="SnbtCompound"/>.</exception>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("{components: {\"minecraft:custom_name\": \"Furnace Key\"}}");
    /// var component = LockComponent.Parse(node);
    /// </code>
    /// </example>
    public static LockComponent Parse(SnbtCompound compound)
    {
        return new LockComponent(compound);
    }

    /// <summary>
    /// Serializes the component into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> containing the item predicate compound.</returns>
    public ISnbtNode ToSnbt() => Predicate;

    /// <summary>
    /// Implicitly converts an <see cref="SnbtCompound"/> item predicate into a <see cref="LockComponent"/>.
    /// </summary>
    /// <param name="predicate">The item predicate compound.</param>
    public static implicit operator LockComponent(SnbtCompound predicate) => new(predicate);
}