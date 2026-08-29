using Core.DataComponents.Models.Interfaces;
using Core.SNBT;
using Core.SNBT.Nodes;


using Core.SNBT.Interfaces;

namespace Core.DataComponents.Models;

/// <summary>
/// Represents an individual custom mob effect instance applied by a potion or consumable item.
/// </summary>
/// <param name="Id">The resource identifier of the status effect (e.g. <c>minecraft:speed</c>).</param>
/// <param name="Amplifier">The effect tier amplifier, where level I has value 0. Defaults to 0.</param>
/// <param name="Duration">The effect duration in ticks. A value of -1 is treated as infinity. Defaults to 1 tick.</param>
/// <param name="Ambient">Whether the effect is provided by an ambient source (beacon/conduit) with less intrusive screen visuals. Defaults to <see langword="false"/>.</param>
/// <param name="ShowParticles">Whether the effect produces visible particles around the entity. Defaults to <see langword="true"/>.</param>
/// <param name="ShowIcon">Whether an icon for this effect appears in the HUD and inventory. Defaults to <see langword="true"/>.</param>
public record CustomEffect(
    string Id,
    sbyte Amplifier = 0,
    int Duration = 1,
    bool Ambient = false,
    bool ShowParticles = true,
    bool ShowIcon = true
) : ICompoundModel<CustomEffect>
{
    /// <summary>
    /// Parses a <see cref="CustomEffect"/> from an SNBT compound node.
    /// </summary>
    /// <param name="compound">The SNBT compound node containing effect properties.</param>
    /// <returns>A populated <see cref="CustomEffect"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when the required <c>id</c> field is missing.</exception>
    public static CustomEffect Parse(SnbtCompound compound)
    {
        return new CustomEffect(
            Id: compound.GetString("id"),
            Amplifier: (sbyte)compound.GetInt("amplifier"),
            Duration: compound.GetInt("duration", 1),
            Ambient: compound.GetBool("ambient"),
            ShowParticles: compound.GetBool("show_particles", true),
            ShowIcon: compound.GetBool("show_icon", true)
        );
    }

    /// <summary>
    /// Serializes the custom effect into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the effect compound.</returns>
    public ISnbtNode ToSnbt() => Snbt.Compound()
        .Put("id", Id)
        .PutOptional("amplifier", Amplifier, 0)
        .PutOptional("duration", Duration, 1)
        .PutOptional("ambient", Ambient, false)
        .PutOptional("show_particles", ShowParticles, true)
        .PutOptional("show_icon", ShowIcon, true)
        .Build();
}