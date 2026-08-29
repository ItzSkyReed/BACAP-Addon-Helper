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
) : IByteComponent<EnchantmentGlintOverrideComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:enchantment_glint_override";

    /// <summary>
    /// Parses an <see cref="EnchantmentGlintOverrideComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="byteNode">The SNBT node to parse, which must be an <see cref="SnbtByte"/> representing a boolean.</param>
    /// <returns>A populated <see cref="EnchantmentGlintOverrideComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("false");
    /// var component = EnchantmentGlintOverrideComponent.Parse(node);
    /// </code>
    /// </example>
    public static EnchantmentGlintOverrideComponent Parse(SnbtByte byteNode)
    {
        return new EnchantmentGlintOverrideComponent(byteNode.Value != 0);
    }

    /// <summary>
    /// Serializes the component into an SNBT byte node (0b or 1b).
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> containing the boolean state.</returns>
    public ISnbtNode ToSnbt() => new SnbtByte((sbyte)(Value ? 1 : 0));
}