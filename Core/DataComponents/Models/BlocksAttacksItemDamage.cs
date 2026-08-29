
using Core.DataComponents.Models.Interfaces;
using Core.SNBT;

using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;

namespace Core.DataComponents.Models;

/// <summary>
/// Controls the durability damage inflicted on a blocking item when mitigating incoming attacks.
/// </summary>
/// <param name="Threshold">The minimum incoming damage value required before the item takes any durability damage. Defaults to 0.0.</param>
/// <param name="Base">The flat base durability damage dealt to the item on a successful block. Defaults to 0.0.</param>
/// <param name="Factor">The multiplier applied to incoming blocked damage to determine additional durability loss. Defaults to 1.5.</param>
public record BlocksAttacksItemDamage(
    float Threshold = 0.0f,
    float Base = 0.0f,
    float Factor = 1.5f
) : ICompoundModel<BlocksAttacksItemDamage>
{
    /// <summary>
    /// Parses a <see cref="BlocksAttacksItemDamage"/> instance from an SNBT compound node.
    /// </summary>
    /// <param name="compound">The SNBT compound node containing durability degradation settings.</param>
    /// <returns>A populated <see cref="BlocksAttacksItemDamage"/> instance.</returns>
    public static BlocksAttacksItemDamage Parse(SnbtCompound compound)
    {
        return new BlocksAttacksItemDamage(
            Threshold: compound.GetFloat("threshold"),
            Base: compound.GetFloat("base"),
            Factor: compound.GetFloat("factor", 1.5f)
        );
    }

    /// <summary>
    /// Serializes the durability damage configuration into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the item damage compound.</returns>
    public ISnbtNode ToSnbt() => Snbt.Compound()
        .PutOptional("threshold", Threshold, 0.0f)
        .PutOptional("base", Base, 0.0f)
        .PutOptional("factor", Factor, 1.5f)
        .Build();
}