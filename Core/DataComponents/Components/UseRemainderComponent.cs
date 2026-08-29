
using Core.SNBT;
using Core.SNBT.Nodes;

using Core.SNBT.Interfaces;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Specifies the remainder item stack that replaces this item after its count decreases upon use (<c>minecraft:use_remainder</c>).
/// </summary>
/// <param name="Id">The resource location identifier of the remainder item.</param>
/// <param name="Count">The stack size count (1 to 99). Defaults to 1.</param>
/// <param name="CustomComponents">Optional map of additional non-default data components attached to the remainder item.</param>
[UsedImplicitly]
public record UseRemainderComponent(
    string Id,
    int Count = 1,
    SnbtCompound? CustomComponents = null
) : ICompoundComponent<UseRemainderComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:use_remainder";

    /// <summary>
    /// Parses a <see cref="UseRemainderComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="compound">The SNBT node to parse, which must be an <see cref="SnbtCompound"/>.</param>
    /// <returns>A populated <see cref="UseRemainderComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("{id: \"minecraft:bone\", count: 2, components: {custom_name: \"Chicken Bone\"}}");
    /// var component = UseRemainderComponent.Parse(node);
    /// </code>
    /// </example>
    public static UseRemainderComponent Parse(SnbtCompound compound)
    {

        return new UseRemainderComponent(
            Id: compound.GetString("id"),
            Count: compound.GetInt("count", 1),
            CustomComponents: compound.GetNode("components") as SnbtCompound
        );
    }

    /// <summary>
    /// Serializes the component into an SNBT compound node representing the remainder item stack.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> containing the item stack definition.</returns>
    public ISnbtNode ToSnbt()
    {
        var builder = Snbt.Compound()
            .Put("id", Id)
            .PutOptional("count", Count, 1);

        if (CustomComponents != null)
            builder.Put("components", CustomComponents);

        return builder.Build();
    }

    /// <summary>
    /// Implicitly converts an item identifier string into a <see cref="UseRemainderComponent"/> with count 1.
    /// </summary>
    /// <param name="id">The remainder item resource identifier.</param>
    public static implicit operator UseRemainderComponent(string id) => new(id);
}