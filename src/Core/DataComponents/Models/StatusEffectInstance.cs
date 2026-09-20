using Core.DataComponents.Models.Interfaces;
using Core.SNBT;

using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;

namespace Core.DataComponents.Models;

public record StatusEffectInstance(
    string Id,
    byte Amplifier = 0,
    int Duration = 1,
    bool Ambient = false,
    bool ShowParticles = true,
    bool ShowIcon = true
) : ICompoundModel<StatusEffectInstance>
{
    public static StatusEffectInstance Parse(SnbtCompound compound)
    {
        return new StatusEffectInstance(
            Id: compound.GetString("id"),
            Amplifier: (byte)compound.GetInt("amplifier"),
            Duration: compound.GetInt("duration", 1),
            Ambient: compound.GetBool("ambient"),
            ShowParticles: compound.GetBool("show_particles", true),
            ShowIcon: compound.GetBool("show_icon", true)
        );
    }

    public ISnbtNode ToSnbt() => Snbt.Compound()
        .Put("id", Id)
        .PutOptional("amplifier", (sbyte)Amplifier, 0)
        .PutOptional("duration", Duration, 1)
        .PutOptional("ambient", Ambient, false)
        .PutOptional("show_particles", ShowParticles, true)
        .PutOptional("show_icon", ShowIcon, true)
        .Build();
}