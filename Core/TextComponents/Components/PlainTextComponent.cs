using Core.SNBT;
using Core.SNBT.Interfaces;
using Core.TextComponents.Models;
using JetBrains.Annotations;

namespace Core.TextComponents.Components;

[PublicAPI]
public record PlainTextComponent(
    string Text,
    TextStyle? Style = null,
    List<TextComponent>? Extra = null
) : TextComponent(Style, Extra)
{
    public override ISnbtNode ToSnbt()
    {
        var builder = Snbt.Compound().Put("text", Text);
        return ApplyBaseProperties(builder).Build();
    }
}