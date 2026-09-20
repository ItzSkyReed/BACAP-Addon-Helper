using Core.Commands.Models;
using Core.SNBT;
using Core.SNBT.Interfaces;
using Core.TextComponents.Models;
using JetBrains.Annotations;

namespace Core.TextComponents.Components;

[PublicAPI]
public record SelectorComponent(
    Selector Selector,
    TextComponent? Separator = null,
    TextStyle? Style = null,
    List<TextComponent>? Extra = null
) : TextComponent(Style, Extra)
{
    public override ISnbtNode ToSnbt()
    {
        var builder = Snbt.Compound().Put("selector", Selector.Value);

        if (Separator != null)
            builder.Put("separator", Separator.ToSnbt());

        return ApplyBaseProperties(builder).Build();
    }
}