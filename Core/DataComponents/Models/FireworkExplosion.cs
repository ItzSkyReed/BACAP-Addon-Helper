using Core.DataComponents.Models.Interfaces;
using Core.SNBT;

using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;

namespace Core.DataComponents.Models;

/// <summary>
/// Represents a single firework explosion effect definition.
/// Shared across <c>minecraft:firework_explosion</c> and <c>minecraft:fireworks</c>.
/// </summary>
public record FireworkExplosion(
    string Shape = "small_ball",
    List<int>? Colors = null,
    List<int>? FadeColors = null,
    bool HasTrail = false,
    bool HasTwinkle = false
) : ICompoundModel<FireworkExplosion>
{
    public static FireworkExplosion Parse(SnbtCompound compound)
    {
        return new FireworkExplosion(
            Shape: compound.GetString("shape", "small_ball"),
            Colors: ParseColorArray(compound.GetNode("colors")),
            FadeColors: ParseColorArray(compound.GetNode("fade_colors")),
            HasTrail: compound.GetBool("has_trail"),
            HasTwinkle: compound.GetBool("has_twinkle")
        );
    }

    public ISnbtNode ToSnbt()
    {
        var builder = Snbt.Compound()
            .PutOptional("shape", Shape, "small_ball")
            .PutOptional("has_trail", HasTrail, false)
            .PutOptional("has_twinkle", HasTwinkle, false);

        if (Colors is { Count: > 0 })
        {
            var nodes = Colors.Select(c => (ISnbtNode)new SnbtInt(c)).ToList();
            builder.Put("colors", new SnbtIntArray(nodes));
        }

        if (FadeColors is not { Count: > 0 })
            return builder.Build();
        {
            var nodes = FadeColors.Select(c => (ISnbtNode)new SnbtInt(c)).ToList();
            builder.Put("fade_colors", new SnbtIntArray(nodes));
        }

        return builder.Build();
    }

    private static List<int>? ParseColorArray(ISnbtNode? node)
    {
        switch (node)
        {
            case SnbtIntArray intArray:
            {
                var list = new List<int>(intArray.Items.Count);
                foreach (var item in intArray.Items)
                    if (item is SnbtInt i) list.Add(i.Value);
                return list;
            }
            case SnbtList listNode:
            {
                var list = new List<int>(listNode.Items.Count);
                foreach (var item in listNode.Items)
                {
                    switch (item)
                    {
                        case SnbtInt i:
                            list.Add(i.Value);
                            break;
                        case SnbtLong l:
                            list.Add((int)l.Value);
                            break;
                    }
                }
                return list;
            }
            default:
                return null;
        }
    }
}