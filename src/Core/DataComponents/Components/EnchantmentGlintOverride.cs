using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;
using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Overrides the visual enchantment glint effect on an item (<c>minecraft:enchantment_glint_override</c>).
/// When <see langword="true"/>, the item displays a glint even without enchantments.
/// When <see langword="false"/>, the glint is hidden even if enchantments are present.
/// </summary>
/// <param name="Value">The boolean flag controlling glint visibility override.</param>
[UsedImplicitly]
public record EnchantmentGlintOverrideComponent(
    bool Value
) : IFlexibleComponent<EnchantmentGlintOverrideComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:enchantment_glint_override";

    /// <summary>
    /// Parses an <see cref="EnchantmentGlintOverrideComponent"/> from an SNBT node representation.
    /// Supports both <see cref="SnbtByte"/> and <see cref="SnbtBool"/> nodes.
    /// </summary>
    /// <param name="node">The SNBT node to parse.</param>
    /// <returns>A populated <see cref="EnchantmentGlintOverrideComponent"/> instance.</returns>
    public static EnchantmentGlintOverrideComponent Parse(ISnbtNode node)
    {
        return node switch
        {
            SnbtByte byteNode => new EnchantmentGlintOverrideComponent(byteNode.Value != 0),
            SnbtBool boolNode => new EnchantmentGlintOverrideComponent(boolNode.Value),
            _ => throw new ArgumentException(
                $"Component '{ComponentId}' requires node of type 'SnbtByte' or 'SnbtBool', but got '{node.GetType().Name}'.", nameof(node))
        };
    }

    /// <summary>
    /// Serializes the component into an SNBT byte node (0b or 1b).
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> containing the boolean state.</returns>
    public ISnbtNode ToSnbt() => new SnbtBool(Value);
}