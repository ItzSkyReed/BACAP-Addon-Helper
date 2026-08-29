
using Core.DataComponents.Models.Interfaces;
using Core.SNBT;
using Core.SNBT.Nodes;

using Core.SNBT.Interfaces;

namespace Core.DataComponents.Models;

/// <summary>
/// Defines speed and duration requirements for kinetic weapon attack phases (damage, dismount, and knockback).
/// </summary>
/// <param name="MaxDurationTicks">The duration in ticks after the initial delay during which this condition remains active.</param>
/// <param name="MinSpeed">The minimum attacker speed (in blocks/second along look direction) required to trigger the effect. Defaults to 0.0.</param>
/// <param name="MinRelativeSpeed">The minimum relative speed between attacker and target (in blocks/second) required. Defaults to 0.0.</param>
public record KineticWeaponConditions(
    int MaxDurationTicks,
    float MinSpeed = 0.0f,
    float MinRelativeSpeed = 0.0f
) : ICompoundModel<KineticWeaponConditions>
{
    /// <summary>
    /// Parses a <see cref="KineticWeaponConditions"/> model from an SNBT compound node.
    /// </summary>
    /// <param name="compound">The SNBT compound to parse.</param>
    /// <returns>A populated <see cref="KineticWeaponConditions"/> instance.</returns>
    public static KineticWeaponConditions Parse(SnbtCompound compound)
    {
        return new KineticWeaponConditions(
            MaxDurationTicks: compound.GetInt("max_duration_ticks"),
            MinSpeed: compound.GetFloat("min_speed"),
            MinRelativeSpeed: compound.GetFloat("min_relative_speed")
        );
    }

    /// <summary>
    /// Serializes the conditions into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the condition parameters.</returns>
    public ISnbtNode ToSnbt() => Snbt.Compound()
        .Put("max_duration_ticks", MaxDurationTicks)
        .PutOptional("min_speed", MinSpeed, 0.0f)
        .PutOptional("min_relative_speed", MinRelativeSpeed, 0.0f)
        .Build();
}