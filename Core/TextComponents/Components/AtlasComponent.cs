using Core.SNBT;
using Core.SNBT.Interfaces;
using Core.TextComponents.Models;
using JetBrains.Annotations;

namespace Core.TextComponents.Components;

/// <summary>
/// Displays a single sprite from a texture atlas as an inline character.
/// </summary>
[PublicAPI]
public record AtlasComponent(
    string Sprite,
    string? Atlas = null,
    TextStyle? Style = null,
    List<TextComponent>? Extra = null
) : TextComponent(Style, Extra)
{
    public override ISnbtNode ToSnbt()
    {
        var builder = Snbt.Compound()
            .Put("type", "object")
            .Put("object", "atlas")
            .Put("sprite", Sprite);

        // "minecraft:blocks" is the default atlas, so we only append if specified
        if (Atlas != null)
            builder.Put("atlas", Atlas);

        return ApplyBaseProperties(builder).Build();
    }
}