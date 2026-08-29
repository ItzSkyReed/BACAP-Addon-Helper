using Core.SNBT;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Represents the selected block state properties targeted by a debug stick (<c>minecraft:debug_stick_state</c>).
/// </summary>
/// <param name="Properties">A dictionary mapping block resource identifiers to the currently selected property key to edit.</param>
[UsedImplicitly]
public record DebugStickStateComponent(
    Dictionary<string, string> Properties
) : ICompoundComponent<DebugStickStateComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:debug_stick_state";

    /// <summary>
    /// Parses a <see cref="DebugStickStateComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="compound">The SNBT node to parse, which must be an <see cref="SnbtCompound"/>.</param>
    /// <returns>A populated <see cref="DebugStickStateComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("{\"minecraft:oak_fence\": \"west\", \"minecraft:candle\": \"lit\"}");
    /// var component = DebugStickStateComponent.Parse(node);
    /// </code>
    /// </example>
    public static DebugStickStateComponent Parse(SnbtCompound compound)
    {

        var properties = new Dictionary<string, string>(compound.Tags.Count);

        foreach (var (blockId, valueNode) in compound.Tags)
        {
            if (valueNode is SnbtString str)
                properties[blockId] = str.Value;
        }

        return new DebugStickStateComponent(properties);
    }

    /// <summary>
    /// Serializes the component into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the compound of selected block state properties.</returns>
    public ISnbtNode ToSnbt()
    {
        var builder = Snbt.Compound();

        foreach (var (blockId, propertyKey) in Properties)
            builder.Put(blockId, propertyKey);

        return builder.Build();
    }
}