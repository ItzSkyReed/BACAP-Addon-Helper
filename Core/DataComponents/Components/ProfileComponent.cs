using System.Buffers.Binary;
using Core.DataComponents.Models;
using Core.SNBT;
using Core.SNBT.Nodes;

using Core.SNBT.Interfaces;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Provides player profile data and custom skins/capes used for rendering heads, skins, mannequins, and face sprites (<c>minecraft:profile</c>).
/// </summary>
/// <param name="Name">Optional player username (up to 16 characters).</param>
/// <param name="Id">Optional player UUID.</param>
/// <param name="Properties">Optional list of resolved profile properties (such as Base64 texture payloads).</param>
/// <param name="Texture">Optional direct resource pack path to a skin texture overriding the profile skin.</param>
/// <param name="Cape">Optional direct resource pack path to a cape texture.</param>
/// <param name="Elytra">Optional direct resource pack path to an elytra texture.</param>
/// <param name="Model">Optional player model type (<c>wide</c> or <c>slim</c>).</param>
[UsedImplicitly]
public record ProfileComponent(
    string? Name = null,
    Guid? Id = null,
    List<ProfileProperty>? Properties = null,
    string? Texture = null,
    string? Cape = null,
    string? Elytra = null,
    string? Model = null
) : IParsableComponent<ProfileComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:profile";

    /// <summary>
    /// Initializes a new instance of the <see cref="ProfileComponent"/> record referencing a player by username.
    /// </summary>
    /// <param name="name">The player username.</param>
    public ProfileComponent(string name) : this(Name: name, Id: null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProfileComponent"/> record referencing a player by UUID.
    /// </summary>
    /// <param name="id">The player unique identifier.</param>
    public ProfileComponent(Guid id) : this(Name: null, Id: id)
    {
    }

    /// <summary>
    /// Parses a <see cref="ProfileComponent"/> from an SNBT node representation.
    /// Supports both simple username strings and full profile compound definitions.
    /// </summary>
    /// <param name="node">The SNBT node to parse (<see cref="SnbtString"/> or <see cref="SnbtCompound"/>).</param>
    /// <returns>A populated <see cref="ProfileComponent"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="node"/> is neither a string nor a compound.</exception>
    /// <example>
    /// <code>
    /// // From player username:
    /// var comp1 = ProfileComponent.Parse(new SnbtString("MinecraftWiki"));
    ///
    /// // From compound:
    /// var node = SnbtParser.Parse("{name: \"Steve\", id: [I; 0, 0, 0, 1]}");
    /// var comp2 = ProfileComponent.Parse(node);
    /// </code>
    /// </example>
    public static ProfileComponent Parse(ISnbtNode node)
    {
        return node switch
        {
            SnbtString str => new ProfileComponent(str.Value),
            SnbtCompound compound => new ProfileComponent(
                Name: compound.GetOptionalString("name"),
                Id: ParseUuid(compound.GetNode("id")),
                Properties: ParseProperties(compound.GetNode("properties")),
                Texture: compound.GetOptionalString("texture"),
                Cape: compound.GetOptionalString("cape"),
                Elytra: compound.GetOptionalString("elytra"),
                Model: compound.GetOptionalString("model")
            ),
            _ => throw new ArgumentException("Profile component must be a string username or a compound.")
        };
    }

    /// <summary>
    /// Serializes the component into an SNBT node.
    /// Emits a compact <see cref="SnbtString"/> if only <see cref="Name"/> is specified.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the profile configuration.</returns>
    public ISnbtNode ToSnbt()
    {
        if (Id == null && (Properties == null || Properties.Count == 0) &&
            Texture == null && Cape == null && Elytra == null && Model == null)
        {
            if (Name != null)
                return new SnbtString(Name);
        }

        var builder = Snbt.Compound()
            .PutOptional("name", Name)
            .PutOptional("texture", Texture)
            .PutOptional("cape", Cape)
            .PutOptional("elytra", Elytra)
            .PutOptional("model", Model);

        if (Id.HasValue)
            builder.Put("id", UuidToIntArray(Id.Value));

        if (Properties is { Count: > 0 })
        {
            builder.PutList("properties", list =>
            {
                foreach (var prop in Properties)
                    list.Add(prop.ToSnbt());
            });
        }

        return builder.Build();
    }

    /// <summary>
    /// Implicitly converts a username string into a <see cref="ProfileComponent"/>.
    /// </summary>
    /// <param name="name">The player username.</param>
    public static implicit operator ProfileComponent(string name) => new(name);

    private static List<ProfileProperty>? ParseProperties(ISnbtNode? node)
    {
        if (node is not SnbtList list || list.Items.Count == 0)
            return null;

        var properties = new List<ProfileProperty>(list.Items.Count);
        foreach (var item in list.Items)
        {
            if (item is SnbtCompound propComp)
                properties.Add(ProfileProperty.Parse(propComp));
        }

        return properties;
    }

    private static Guid? ParseUuid(ISnbtNode? node)
    {
        switch (node)
        {
            case SnbtIntArray { Items.Count: 4 } intArray:
            {
                var i0 = (intArray.Items[0] as SnbtInt)?.Value ?? 0;
                var i1 = (intArray.Items[1] as SnbtInt)?.Value ?? 0;
                var i2 = (intArray.Items[2] as SnbtInt)?.Value ?? 0;
                var i3 = (intArray.Items[3] as SnbtInt)?.Value ?? 0;

                Span<byte> bytes = stackalloc byte[16];
                BinaryPrimitives.WriteInt32BigEndian(bytes[0..4], i0);
                BinaryPrimitives.WriteInt32BigEndian(bytes[4..8], i1);
                BinaryPrimitives.WriteInt32BigEndian(bytes[8..12], i2);
                BinaryPrimitives.WriteInt32BigEndian(bytes[12..16], i3);

                return new Guid(bytes, bigEndian: true);
            }

            // Handle SnbtList (since standard JSON arrays deserialize as lists rather than int arrays)
            case SnbtList { Items.Count: 4 } list:
            {
                if (list.Items[0] is not SnbtInt i0 ||
                    list.Items[1] is not SnbtInt i1 ||
                    list.Items[2] is not SnbtInt i2 ||
                    list.Items[3] is not SnbtInt i3)
                    return null;

                Span<byte> bytes = stackalloc byte[16];
                BinaryPrimitives.WriteInt32BigEndian(bytes[0..4], i0.Value);
                BinaryPrimitives.WriteInt32BigEndian(bytes[4..8], i1.Value);
                BinaryPrimitives.WriteInt32BigEndian(bytes[8..12], i2.Value);
                BinaryPrimitives.WriteInt32BigEndian(bytes[12..16], i3.Value);

                return new Guid(bytes, bigEndian: true);
            }

            case SnbtString str when Guid.TryParse(str.Value, out var guid):
                return guid;

            default:
                return null;
        }
    }

    private static SnbtIntArray UuidToIntArray(Guid guid)
    {
        Span<byte> bytes = stackalloc byte[16];
        guid.TryWriteBytes(bytes, bigEndian: true, out _);

        var i0 = BinaryPrimitives.ReadInt32BigEndian(bytes[0..4]);
        var i1 = BinaryPrimitives.ReadInt32BigEndian(bytes[4..8]);
        var i2 = BinaryPrimitives.ReadInt32BigEndian(bytes[8..12]);
        var i3 = BinaryPrimitives.ReadInt32BigEndian(bytes[12..16]);

        return new SnbtIntArray([
            new SnbtInt(i0),
            new SnbtInt(i1),
            new SnbtInt(i2),
            new SnbtInt(i3)
        ]);
    }
}