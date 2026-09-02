using Core.SNBT;
using Core.SNBT.Interfaces;
using Core.TextComponents.Models;
using JetBrains.Annotations;

namespace Core.TextComponents.Components;

[PublicAPI]
public record TranslatableComponent(
    string Translate,
    string? Fallback = null,
    List<TextComponent>? With = null,
    TextStyle? Style = null,
    List<TextComponent>? Extra = null
) : TextComponent(Style, Extra)
{
    /// <summary>
    /// Serializes this component into an SNBT node representation.
    /// </summary>
    /// <returns>The constructed SNBT node.</returns>
    public override ISnbtNode ToSnbt()
    {
        var builder = Snbt.Compound().Put("translate", Translate);

        if (Fallback != null)
            builder.Put("fallback", Fallback);

        if (With is { Count: > 0 })
        {
            builder.PutList("with", list =>
            {
                foreach (var arg in With)
                    list.Add(arg.ToSnbt());
            });
        }

        return ApplyBaseProperties(builder).Build();
    }
}