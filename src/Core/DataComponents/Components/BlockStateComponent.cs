using Core.DataComponents.Interfaces;
using Core.SNBT;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

namespace Core.DataComponents.Components;

/// <summary>
/// Overrides default block state properties applied when placing a block item in the world (<c>minecraft:block_state</c>).
/// </summary>
/// <param name="Properties">The dictionary of block state property keys and their corresponding string values (e.g. <c>facing=north</c>, <c>half=top</c>, <c>lit=true</c>).</param>
[UsedImplicitly]
public record BlockStateComponent(
    Dictionary<string, string> Properties
) : ICompoundComponent<BlockStateComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:block_state";

    /// <summary>
    /// Initializes a new instance of the <see cref="BlockStateComponent"/> record with an empty property dictionary.
    /// </summary>
    public BlockStateComponent() : this(new Dictionary<string, string>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlockStateComponent"/> record from key-value tuple pairs.
    /// </summary>
    /// <param name="properties">The block state property key-value pairs.</param>
    public BlockStateComponent(params (string Key, string Value)[] properties)
        : this(new Dictionary<string, string>(properties.Length))
    {
        foreach (var (key, value) in properties)
        {
            Properties[key] = value;
        }
    }

    /// <summary>
    /// Parses a <see cref="BlockStateComponent"/> directly from an SNBT compound node.
    /// Converts primitive NBT values (booleans, integers, strings) into their canonical string representations.
    /// </summary>
    /// <param name="compound">The SNBT compound containing block state property key-value pairs.</param>
    /// <returns>A populated <see cref="BlockStateComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("{facing: 'north', half: 'top', open: 1b}");
    /// var component = BlockStateComponent.Parse((SnbtCompound)node);
    /// </code>
    /// </example>
    public static BlockStateComponent Parse(SnbtCompound compound)
    {
        var properties = new Dictionary<string, string>(compound.Tags.Count);

        foreach (var (key, valueNode) in compound.Tags)
        {
            var valueStr = valueNode switch
            {
                SnbtString s => s.Value,
                SnbtByte b => b.Value switch
                {
                    1 => "true",
                    0 => "false",
                    _ => b.Value.ToString()
                },
                SnbtInt i => i.Value.ToString(),
                _ => valueNode.ToSnbtString()
            };

            properties[key] = valueStr;
        }

        return new BlockStateComponent(properties);
    }

    /// <summary>
    /// Serializes the block state properties into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the block state mapping compound.</returns>
    public ISnbtNode ToSnbt()
    {
        var builder = Snbt.Compound();

        foreach (var (key, value) in Properties)
        {
            builder.Put(key, value);
        }

        return builder.Build();
    }
}