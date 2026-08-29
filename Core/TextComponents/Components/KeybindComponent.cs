using Core.SNBT.Interfaces;
using Core.TextComponents.Models;
using JetBrains.Annotations;

namespace Core.TextComponents.Components;

[PublicAPI]
public record KeybindComponent(
    string Keybind,
    TextStyle? Style = null,
    List<TextComponent>? Extra = null
) : TextComponent(Style, Extra)
{
    public override ISnbtNode ToSnbt() =>
        CreateBaseBuilder()
            .Put("keybind", Keybind)
            .Build();
}