using Core.SNBT;
using Core.SNBT.Interfaces;
using Core.TextComponents.Models;
using JetBrains.Annotations;

namespace Core.TextComponents.Components;

public enum NbtDataSource { Entity, Block, Storage }

[PublicAPI]
public record NbtComponent(
    string NbtPath,
    NbtDataSource SourceType,
    string SourceTarget,
    bool Interpret = false,
    TextComponent? Separator = null,
    TextStyle? Style = null,
    List<TextComponent>? Extra = null
) : TextComponent(Style, Extra)
{
    public override ISnbtNode ToSnbt()
    {
        var builder = Snbt.Compound().Put("nbt", NbtPath);

        var sourceKey = SourceType switch
        {
            NbtDataSource.Entity => "entity",
            NbtDataSource.Block => "block",
            NbtDataSource.Storage => "storage",
            _ => "entity"
        };
        builder.Put(sourceKey, SourceTarget);

        if (Interpret) builder.Put("interpret", true);
        if (Separator != null) builder.Put("separator", Separator.ToSnbt());

        return ApplyBaseProperties(builder).Build();
    }
}