using Core.SNBT.Nodes;

using Core.SNBT.Interfaces;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Represents the decorations (pottery sherds or bricks) applied to each of the four faces of a decorated pot (<c>minecraft:pot_decorations</c>).
/// </summary>
/// <remarks>
/// Supports both list representation (ordered as Front, Left, Back, Right) and compound map representation.
/// Unspecified faces default to <c>minecraft:brick</c>.
/// </remarks>
/// <param name="Front">The item identifier for the front face (e.g., <c>minecraft:angler_pottery_sherd</c> or <c>minecraft:brick</c>).</param>
/// <param name="Left">The item identifier for the left face.</param>
/// <param name="Back">The item identifier for the back face.</param>
/// <param name="Right">The item identifier for the right face.</param>
[UsedImplicitly]
public record PotDecorationsComponent(
    string Front = PotDecorationsComponent.DefaultDecoration,
    string Left = PotDecorationsComponent.DefaultDecoration,
    string Back = PotDecorationsComponent.DefaultDecoration,
    string Right = PotDecorationsComponent.DefaultDecoration
) : IParsableComponent<PotDecorationsComponent>
{
    /// <summary>
    /// The default item identifier used when a pot face has no custom sherd applied (<c>minecraft:brick</c>).
    /// </summary>
    [PublicAPI]
    public const string DefaultDecoration = "minecraft:brick";

    /// <inheritdoc/>
    public static string ComponentId => "minecraft:pot_decorations";

    /// <summary>
    /// Initializes a new instance of the <see cref="PotDecorationsComponent"/> record from an ordered collection of face item identifiers.
    /// </summary>
    /// <param name="sherds">Up to 4 face decoration item identifiers in order (Front, Left, Back, Right).</param>
    public PotDecorationsComponent(IReadOnlyList<string> sherds) : this(
        Front: sherds.Count > 0 ? sherds[0] : DefaultDecoration,
        Left: sherds.Count > 1 ? sherds[1] : DefaultDecoration,
        Back: sherds.Count > 2 ? sherds[2] : DefaultDecoration,
        Right: sherds.Count > 3 ? sherds[3] : DefaultDecoration
    )
    {
    }

    /// <summary>
    /// Parses a <see cref="PotDecorationsComponent"/> from an SNBT node representation.
    /// Supports both list format (<c>[front, left, back, right]</c>) and compound map format.
    /// </summary>
    /// <param name="node">The SNBT node to parse (<see cref="SnbtList"/> or <see cref="SnbtCompound"/>).</param>
    /// <returns>A populated <see cref="PotDecorationsComponent"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="node"/> is neither a list nor a compound.</exception>
    /// <example>
    /// <code>
    /// // From list format:
    /// var listNode = SnbtParser.Parse("[\"minecraft:angler_pottery_sherd\", \"minecraft:brick\", \"minecraft:brick\", \"minecraft:brick\"]");
    /// var comp1 = PotDecorationsComponent.Parse(listNode);
    ///
    /// // From compound format:
    /// var compoundNode = SnbtParser.Parse("{front: \"minecraft:angler_pottery_sherd\", back: \"minecraft:archer_pottery_sherd\"}");
    /// var comp2 = PotDecorationsComponent.Parse(compoundNode);
    /// </code>
    /// </example>
    public static PotDecorationsComponent Parse(ISnbtNode node)
    {
        switch (node)
        {
            case SnbtList list:
            {
                var front = list.Items.Count > 0 ? ExtractItemId(list.Items[0]) : DefaultDecoration;
                var left = list.Items.Count > 1 ? ExtractItemId(list.Items[1]) : DefaultDecoration;
                var back = list.Items.Count > 2 ? ExtractItemId(list.Items[2]) : DefaultDecoration;
                var right = list.Items.Count > 3 ? ExtractItemId(list.Items[3]) : DefaultDecoration;
                return new PotDecorationsComponent(front, left, back, right);
            }

            case SnbtCompound compound:
            {
                var front = ExtractOptionalItemId(compound.GetNode("front")) ?? DefaultDecoration;
                var left = ExtractOptionalItemId(compound.GetNode("left")) ?? DefaultDecoration;
                var back = ExtractOptionalItemId(compound.GetNode("back")) ?? DefaultDecoration;
                var right = ExtractOptionalItemId(compound.GetNode("right")) ?? DefaultDecoration;
                return new PotDecorationsComponent(front, left, back, right);
            }

            default:
                throw new ArgumentException("Pot decorations component must be a list or a compound.");
        }
    }

    /// <summary>
    /// Serializes the component into an SNBT list node containing the 4 face decorations.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the 4-element list of decoration IDs.</returns>
    public ISnbtNode ToSnbt() => new SnbtList([
        new SnbtString(Front),
        new SnbtString(Left),
        new SnbtString(Back),
        new SnbtString(Right)
    ]);

    private static string ExtractItemId(ISnbtNode node) => node switch
    {
        SnbtString str => str.Value,
        SnbtCompound comp => comp.GetString("id", DefaultDecoration),
        _ => DefaultDecoration
    };

    private static string? ExtractOptionalItemId(ISnbtNode? node) => node switch
    {
        SnbtString str => str.Value,
        SnbtCompound comp => comp.GetOptionalString("id"),
        _ => null
    };
}