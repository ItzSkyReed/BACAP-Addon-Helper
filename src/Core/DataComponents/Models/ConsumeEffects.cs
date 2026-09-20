using Core.DataComponents.Models.Interfaces;
using Core.SNBT;

using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;

namespace Core.DataComponents.Models;

public abstract record ConsumeEffect(string Type) : ICompoundModel<ConsumeEffect>
{
    public ISnbtNode ToSnbt()
    {
        var builder = Snbt.Compound().Put("type", Type);
        Populate(builder);
        return builder.Build();
    }

    protected abstract void Populate(SnbtCompoundBuilder builder);

    public static ConsumeEffect Parse(SnbtCompound compound)
    {

        var rawType = compound.GetString("type");
        var type = rawType.StartsWith("minecraft:") ? rawType["minecraft:".Length..] : rawType;

        return type switch
        {
            "apply_effects" => ParseApplyEffects(compound),
            "remove_effects" => ParseRemoveEffects(compound),
            "clear_all_effects" => new ClearAllEffectsConsumeEffect(),
            "teleport_randomly" => new TeleportRandomlyConsumeEffect(compound.GetFloat("diameter", 16.0f)),
            "play_sound" => new PlaySoundConsumeEffect(compound.GetNode("sound") ?? throw new ArgumentException("PlaySound effect missing 'sound' node.")),
            _ => throw new ArgumentException($"Unknown consume effect type: '{rawType}'.")
        };
    }

    private static ApplyEffectsConsumeEffect ParseApplyEffects(SnbtCompound comp)
    {
        var effects = new List<StatusEffectInstance>();
        if (comp.GetNode("effects") is SnbtList list)
        {
            foreach (var item in list.Items)
            {
                if (item is SnbtCompound effectComp)
                    effects.Add(StatusEffectInstance.Parse(effectComp));
            }
        }
        return new ApplyEffectsConsumeEffect(effects, comp.GetFloat("probability", 1.0f));
    }

    private static RemoveEffectsConsumeEffect ParseRemoveEffects(SnbtCompound comp)
    {
        var effects = new List<string>();
        var effNode = comp.GetNode("effects");
        switch (effNode)
        {
            case SnbtString single:
                effects.Add(single.Value);
                break;
            case SnbtList list:
            {
                foreach (var item in list.Items)
                    if (item is SnbtString s) effects.Add(s.Value);
                break;
            }
        }
        return new RemoveEffectsConsumeEffect(effects);
    }
}

public record ApplyEffectsConsumeEffect(
    List<StatusEffectInstance> Effects,
    float Probability = 1.0f
) : ConsumeEffect("apply_effects")
{
    protected override void Populate(SnbtCompoundBuilder builder)
    {
        builder.PutOptional("probability", Probability, 1.0f);
        builder.PutList("effects", list =>
        {
            foreach (var eff in Effects) list.Add(eff.ToSnbt());
        });
    }
}

public record RemoveEffectsConsumeEffect(List<string> Effects) : ConsumeEffect("remove_effects")
{
    protected override void Populate(SnbtCompoundBuilder builder)
    {
        if (Effects.Count == 1)
        {
            builder.Put("effects", Effects[0]);
        }
        else
        {
            builder.PutList("effects", list => list.AddRange(Effects));
        }
    }
}

public record ClearAllEffectsConsumeEffect() : ConsumeEffect("clear_all_effects")
{
    protected override void Populate(SnbtCompoundBuilder builder) { }
}

public record TeleportRandomlyConsumeEffect(float Diameter = 16.0f) : ConsumeEffect("teleport_randomly")
{
    protected override void Populate(SnbtCompoundBuilder builder) =>
        builder.PutOptional("diameter", Diameter, 16.0f);
}

public record PlaySoundConsumeEffect(ISnbtNode Sound) : ConsumeEffect("play_sound")
{
    protected override void Populate(SnbtCompoundBuilder builder) =>
        builder.Put("sound", Sound);
}