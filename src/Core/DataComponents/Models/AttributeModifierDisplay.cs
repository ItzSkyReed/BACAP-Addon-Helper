using Core.DataComponents.Models.Interfaces;
using Core.SNBT;

using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using Core.TextComponents.Components;
using JetBrains.Annotations;

namespace Core.DataComponents.Models;

/// <summary>
/// Controls tooltip formatting and custom text overrides for an attribute modifier.
/// </summary>
/// <param name="Type">The display mode: <c>default</c>, <c>hidden</c>, or <c>override</c>. Defaults to <c>default</c>.</param>
/// <param name="Value">The text component override displayed when <paramref name="Type"/> is set to <c>override</c>.</param>
public record AttributeModifierDisplay(
    string Type = AttributeModifierDisplay.DefaultType,
    TextComponent? Value = null
) : ICompoundModel<AttributeModifierDisplay>
{
    [PublicAPI] public const string DefaultType = "default";
    [PublicAPI] public const string HiddenType = "hidden";
    [PublicAPI] public const string OverrideType = "override";

    /// <summary>
    /// Parses an <see cref="AttributeModifierDisplay"/> from an SNBT compound node.
    /// </summary>
    /// <param name="compound">The compound node containing display settings.</param>
    /// <returns>A populated <see cref="AttributeModifierDisplay"/> instance.</returns>
    public static AttributeModifierDisplay Parse(SnbtCompound compound)
    {
        var type = compound.GetString("type", DefaultType);
        TextComponent? value = null;

        if (compound.GetNode("value") is { } valNode)
        {
            value = TextComponent.Parse(valNode);
        }

        return new AttributeModifierDisplay(type, value);
    }

    /// <summary>
    /// Serializes display settings into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> containing display metadata.</returns>
    public ISnbtNode ToSnbt()
    {
        var builder = Snbt.Compound()
            .PutOptional("type", Type, DefaultType);

        if (Value != null)
        {
            builder.Put("value", Value.ToSnbt());
        }

        return builder.Build();
    }
}