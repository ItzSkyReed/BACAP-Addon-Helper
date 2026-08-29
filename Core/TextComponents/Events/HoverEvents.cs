
using Core.SNBT;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using Core.TextComponents.Components;
using JetBrains.Annotations;

namespace Core.TextComponents.Events;

public abstract record HoverEvent(string Action)
{
    [PublicAPI]
    public ISnbtNode ToSnbt()
    {
        var builder = Snbt.Compound().Put("action", Action);
        Populate(builder);
        return builder.Build();
    }

    protected abstract void Populate(SnbtCompoundBuilder builder);
}

[PublicAPI]
public record ShowTextHoverEvent(TextComponent Value) : HoverEvent("show_text")
{
    protected override void Populate(SnbtCompoundBuilder builder) => builder.Put("value", Value.ToSnbt());
}

[PublicAPI]
public record ShowItemHoverEvent(
    string Id,
    int? Count = null,
    SnbtCompound? Components = null
) : HoverEvent("show_item")
{
    protected override void Populate(SnbtCompoundBuilder builder)
    {
        builder.Put("id", Id);
        if (Count.HasValue) builder.Put("count", Count.Value);
        if (Components != null) builder.Put("components", Components);
    }
}

[PublicAPI]
public record ShowEntityHoverEvent(
    string Id,
    ISnbtNode Uuid,
    TextComponent? Name = null
) : HoverEvent("show_entity")
{
    protected override void Populate(SnbtCompoundBuilder builder)
    {
        builder.Put("id", Id);
        builder.Put("uuid", Uuid);
        if (Name != null) builder.Put("name", Name.ToSnbt());
    }
}