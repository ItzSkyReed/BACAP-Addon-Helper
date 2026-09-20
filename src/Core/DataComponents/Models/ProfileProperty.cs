using Core.DataComponents.Models.Interfaces;
using Core.SNBT;
using Core.SNBT.Nodes;

using Core.SNBT.Interfaces;

namespace Core.DataComponents.Models;

/// <summary>
/// Represents a single user profile property, such as Base64-encoded skin/cape texture metadata and signature.
/// </summary>
/// <param name="Name">The property identifier name (commonly <c>textures</c>).</param>
/// <param name="Value">The Base64-encoded JSON texture data string.</param>
/// <param name="Signature">Optional Mojang cryptographic signature verifying property authenticity.</param>
public record ProfileProperty(
    string Name,
    string Value,
    string? Signature = null
) : ICompoundModel<ProfileProperty>
{
    /// <summary>
    /// Parses a <see cref="ProfileProperty"/> from an SNBT compound node.
    /// </summary>
    /// <param name="compound">The SNBT compound node containing property fields.</param>
    /// <returns>A populated <see cref="ProfileProperty"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when required fields are missing.</exception>
    public static ProfileProperty Parse(SnbtCompound compound)
    {
        return new ProfileProperty(
            Name: compound.GetString("name"),
            Value: compound.GetString("value"),
            Signature: compound.GetOptionalString("signature")
        );
    }

    /// <summary>
    /// Serializes the profile property into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the property compound.</returns>
    public ISnbtNode ToSnbt() => Snbt.Compound()
        .Put("name", Name)
        .Put("value", Value)
        .PutOptional("signature", Signature)
        .Build();
}