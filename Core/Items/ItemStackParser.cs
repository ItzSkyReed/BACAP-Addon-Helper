using Core.DataComponents;
using Core.SNBT;
using JetBrains.Annotations;
using Pidgin;
using Core.Common;
using Core.DataComponents.Interfaces;

namespace Core.Items;

/// <summary>
/// Provides parsing functionality for modern Minecraft Item Stack syntax (1.20.5+),
/// supporting resource location identifiers and item data component modifications.
/// </summary>
public static class ItemStackParser
{
    private static readonly Parser<char, Unit> Whitespace = Parser.SkipWhitespaces;

    /// <summary>
    /// Parses component removal operations indicated by an exclamation mark prefix (e.g., "!minecraft:damage").
    /// </summary>
    private static readonly Parser<char, Action<DataComponentMap>> ComponentRemoval =
        Parser.Char('!')
            .Then(ParserParts.IdentifierParser)
            .Select<Action<DataComponentMap>>(id => map => map.Remove(id));

    /// <summary>
    /// Parses component assignment operations (e.g., "damage=10", "custom_name='Sword'").
    /// Uses <see cref="ComponentRegistry"/> to convert the parsed SNBT node into a typed <see cref="IParsableComponent{TSelf}"/>.
    /// </summary>
    private static readonly Parser<char, Action<DataComponentMap>> ComponentAddition =
        Parser.Map(
            (id, _, node) => new Action<DataComponentMap>(map =>
            {
                // Delegate AST node conversion to the component registry
                var component = ComponentRegistry.Parse(id, node);
                map.Set(component);
            }),
            ParserParts.IdentifierParser,
            Parser.Char('=').Between(Whitespace),
            SnbtParser.AnyNode
        );

    /// <summary>
    /// Matches either a component assignment or removal operation.
    /// </summary>
    private static readonly Parser<char, Action<DataComponentMap>> ComponentOperation =
        Parser.OneOf(ComponentRemoval, ComponentAddition);

    /// <summary>
    /// Parses a comma-separated block of component operations enclosed in square brackets (e.g., "[damage=10, !custom_name]").
    /// </summary>
    private static readonly Parser<char, Action<DataComponentMap>> ComponentsBlock =
        ComponentOperation
            .Separated(Parser.Char(',').Between(Whitespace))
            .Between(Parser.Char('[').Between(Whitespace), Parser.Char(']').Between(Whitespace))
            .Select<Action<DataComponentMap>>(operations => map =>
            {
                // Sequentially apply all parsed operations to the item's component map
                foreach (var op in operations)
                    op(map);
            });

    /// <summary>
    /// The root parser combinator for an item stack: parses the item ID and an optional component block.
    /// </summary>
    [PublicAPI]
    public static readonly Parser<char, ItemStack> Item =
        Parser.Map(
            (id, opsOpt) =>
            {
                var item = new ItemStack(id);
                // Apply component modifications if the bracketed block was provided
                if (opsOpt.HasValue)
                    opsOpt.Value(item.Components);
                return item;
            },
            ParserParts.IdentifierParser,
            ComponentsBlock.Optional()
        ).Between(Whitespace);

    /// <summary>
    /// Parses a raw item stack string into an <see cref="ItemStack"/> instance.
    /// </summary>
    /// <param name="input">The item string to parse (e.g., "minecraft:diamond_sword[damage=15, !enchantments]").</param>
    /// <returns>A fully configured <see cref="ItemStack"/> instance with applied component modifications.</returns>
    /// <exception cref="Pidgin.ParseException">Thrown when the input string contains invalid syntax or unknown component tokens.</exception>
    /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="input"/> string is null.</exception>
    /// <example>
    /// <code>
    /// // Parsing an item with no components
    /// ItemStack item1 = ItemStackParser.Parse("minecraft:apple");
    ///
    /// // Parsing an item with custom components and removals
    /// ItemStack item2 = ItemStackParser.Parse("minecraft:stick[damage=5, !custom_name]");
    /// </code>
    /// </example>
    [PublicAPI]
    public static ItemStack Parse(string input) => Item.ParseOrThrow(input);
}