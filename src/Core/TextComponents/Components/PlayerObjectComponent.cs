using Core.DataComponents.Models;
using Core.SNBT;
using Core.SNBT.Interfaces;
using Core.TextComponents.Models;
using JetBrains.Annotations;

namespace Core.TextComponents.Components;

/// <summary>
/// Displays the 2D face sprite of a player profile inline.
/// </summary>
[PublicAPI]
public record PlayerObjectComponent(
    string? Name = null,
    int[]? Id = null,
    List<ProfileProperty>? Properties = null,
    string? Texture = null,
    string? Cape = null,
    string? Elytra = null,
    string? Model = null,
    bool? Hat = null,
    TextStyle? Style = null,
    List<TextComponent>? Extra = null
) : TextComponent(Style, Extra)
{
    public override ISnbtNode ToSnbt()
    {
        var builder = Snbt.Compound()
            .Put("type", "object")
            .Put("object", "player");

        // If ONLY the name is provided, output it as a plain string
        if (Name != null && Id == null && Properties == null && Texture == null &&
            Cape == null && Elytra == null && Model == null && Hat == null)
        {
            builder.Put("player", Name);
            return ApplyBaseProperties(builder).Build();
        }

        // Otherwise, construct the full player compound
        builder.PutCompound("player", p =>
        {
            if (Name != null) p.Put("name", Name);
            if (Id != null) p.PutIntArray("id", Id);

            if (Properties != null)
            {
                p.PutList("properties", list =>
                {
                    foreach (var prop in Properties)
                        list.Add(prop.ToSnbt());
                });
            }
            p.PutOptional("texture", Texture);
            p.PutOptional("cape", Cape);
            p.PutOptional("elytra", Elytra);
            p.PutOptional("model", Model);
            if (Hat.HasValue) p.Put("hat", Hat.Value);
        });

        return ApplyBaseProperties(builder).Build();
    }
}