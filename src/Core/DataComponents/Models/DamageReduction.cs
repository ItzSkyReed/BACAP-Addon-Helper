using Core.DataComponents.Models.Interfaces;
using Core.SNBT;

using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;

namespace Core.DataComponents.Models;

/// <summary>
/// Defines a damage reduction rule when blocking with an item (such as a shield).
/// </summary>
/// <param name="Base">The flat base amount of damage absorbed by the block. Defaults to 0.0.</param>
/// <param name="Factor">The multiplier applied to reduce remaining incoming damage (0.0 = completely blocked, 1.0 = no reduction). Defaults to 1.0.</param>
/// <param name="HorizontalBlockingAngle">The horizontal arc angle in degrees centered on the player's look direction within which incoming attacks are blocked. Defaults to 90.0.</param>
/// <param name="Types">Optional list of damage type identifiers or tags (e.g. <c>#minecraft:is_projectile</c>) to which this reduction rule applies.</param>
public record DamageReduction(
    float Base = 0.0f,
    float Factor = 1.0f,
    float HorizontalBlockingAngle = 90.0f,
    List<string>? Types = null
) : ICompoundModel<DamageReduction>
{
    /// <summary>
    /// Parses a <see cref="DamageReduction"/> instance from an SNBT compound node.
    /// </summary>
    /// <param name="compound">The SNBT compound node containing damage reduction properties.</param>
    /// <returns>A populated <see cref="DamageReduction"/> instance.</returns>
    public static DamageReduction Parse(SnbtCompound compound)
    {
        List<string>? types = null;
        var typeNode = compound.GetNode("type") ?? compound.GetNode("types");

        switch (typeNode)
        {
            case SnbtString singleType:
                types = [singleType.Value];
                break;
            case SnbtList typeList:
            {
                types = new List<string>(typeList.Items.Count);
                foreach (var t in typeList.Items)
                {
                    if (t is SnbtString s)
                        types.Add(s.Value);
                }
                break;
            }
        }

        return new DamageReduction(
            Base: compound.GetFloat("base"),
            Factor: compound.GetFloat("factor", 1.0f),
            HorizontalBlockingAngle: compound.GetFloat("horizontal_blocking_angle", 90.0f),
            Types: types
        );
    }

    /// <summary>
    /// Serializes the damage reduction rule into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the damage reduction compound.</returns>
    public ISnbtNode ToSnbt()
    {
        var builder = Snbt.Compound()
            .PutOptional("base", Base, 0.0f)
            .PutOptional("factor", Factor, 1.0f)
            .PutOptional("horizontal_blocking_angle", HorizontalBlockingAngle, 90.0f);

        if (Types is not { Count: > 0 })
            return builder.Build();

        if (Types.Count == 1)
            builder.Put("type", Types[0]);
        else
            builder.PutList("types", tl => tl.AddRange(Types));

        return builder.Build();
    }
}