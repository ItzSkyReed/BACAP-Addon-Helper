using Core.SNBT;
using Core.SNBT.Interfaces;
using Core.TextComponents.Models;
using JetBrains.Annotations;

namespace Core.TextComponents.Components;

[PublicAPI]
public record ScoreComponent(
    string Name,
    string Objective,
    TextStyle? Style = null,
    List<TextComponent>? Extra = null
) : TextComponent(Style, Extra)
{
    public override ISnbtNode ToSnbt()
    {
        var builder = Snbt.Compound()
            .PutCompound("score", s => s
                .Put("name", Name)
                .Put("objective", Objective)
            );

        return ApplyBaseProperties(builder).Build();
    }
}