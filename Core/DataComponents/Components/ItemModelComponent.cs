
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Specifies the custom item model definition used to render this item (<c>minecraft:item_model</c>).
/// References <c>assets/&lt;namespace&gt;/items/&lt;id&gt;.json</c> without the file extension.
/// </summary>
/// <param name="Model">The resource location of the item model definition (e.g., <c>minecraft:diamond_sword</c>).</param>
[UsedImplicitly]
public record ItemModelComponent(
    string Model
) : IStringComponent<ItemModelComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:item_model";

    /// <summary>
    /// Parses an <see cref="ItemModelComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="strNode">The SNBT node to parse, which must be an <see cref="SnbtString"/>.</param>
    /// <returns>A populated <see cref="ItemModelComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("\"minecraft:diamond_sword\"");
    /// var component = ItemModelComponent.Parse(node);
    /// </code>
    /// </example>
    public static ItemModelComponent Parse(SnbtString strNode)
    {
        return  new ItemModelComponent(strNode.Value);
    }

    /// <summary>
    /// Serializes the component into an SNBT string node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> containing the item model identifier.</returns>
    public ISnbtNode ToSnbt() => new SnbtString(Model);

    /// <summary>
    /// Implicitly converts a string identifier into an <see cref="ItemModelComponent"/>.
    /// </summary>
    /// <param name="model">The resource location of the item model.</param>
    public static implicit operator ItemModelComponent(string model) => new(model);
}