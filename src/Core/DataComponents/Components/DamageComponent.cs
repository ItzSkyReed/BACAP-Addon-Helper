using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

[UsedImplicitly]
public record DamageComponent(int Damage) : IIntComponent<DamageComponent>
{
    public static string ComponentId => "minecraft:damage";

    public static DamageComponent Parse(SnbtInt intNode)
    {
        return new DamageComponent(intNode.Value);
    }

    public ISnbtNode ToSnbt() => new SnbtInt(Damage);
}