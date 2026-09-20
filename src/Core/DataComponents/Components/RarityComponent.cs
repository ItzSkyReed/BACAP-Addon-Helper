using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

using Core.DataComponents.Interfaces;

namespace Core.DataComponents.Components;

/// <summary>
/// Specifies the rarity tier of an item, determining its default display name color in tooltips (<c>minecraft:rarity</c>).
/// </summary>
/// <param name="Value">The rarity tier (<c>common</c>, <c>uncommon</c>, <c>rare</c>, or <c>epic</c>). Defaults to <c>common</c>.</param>
[UsedImplicitly]
public record RarityComponent(
    string Value = RarityComponent.Common
) : IStringComponent<RarityComponent>
{
    [PublicAPI]
    public const string Common = "common";
    [PublicAPI]
    public const string Uncommon = "uncommon";
    [PublicAPI]
    public const string Rare = "rare";
    [PublicAPI]
    public const string Epic = "epic";

    /// <inheritdoc/>
    public static string ComponentId => "minecraft:rarity";

    /// <summary>
    /// Parses a <see cref="RarityComponent"/> from an SNBT node representation.
    /// </summary>
    /// <param name="strNode">The SNBT node to parse, which must be an <see cref="SnbtString"/>.</param>
    /// <returns>A populated <see cref="RarityComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("\"epic\"");
    /// var component = RarityComponent.Parse(node);
    /// </code>
    /// </example>
    public static RarityComponent Parse(SnbtString strNode)
    {
        return  new RarityComponent(strNode.Value.ToLowerInvariant());
    }

    /// <summary>
    /// Serializes the component into an SNBT string node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> containing the rarity string.</returns>
    public ISnbtNode ToSnbt() => new SnbtString(Value);

    /// <summary>
    /// Implicitly converts a rarity string into a <see cref="RarityComponent"/>.
    /// </summary>
    /// <param name="rarity">The rarity tier identifier.</param>
    public static implicit operator RarityComponent(string rarity) => new(rarity);
}