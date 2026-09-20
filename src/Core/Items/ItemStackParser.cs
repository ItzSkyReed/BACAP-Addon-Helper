using Core.Common;
using Core.DataComponents;
using Core.DataComponents.Interfaces;
using Core.SNBT;
using JetBrains.Annotations;
using Pidgin;

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
    /// Parses a comma-separated sequence of component operations.
    /// </summary>
    private static readonly Parser<char, IEnumerable<Action<DataComponentMap>>> ComponentOperations =
        ComponentOperation.Separated(Parser.Char(',').Between(Whitespace));

    /// <summary>
    /// Parses a comma-separated block of component operations enclosed in square brackets (e.g., "[damage=10, !custom_name]").
    /// </summary>
    [PublicAPI]
    public static readonly Parser<char, Action<DataComponentMap>> ComponentsBlock =
        ComponentOperations
            .Between(Parser.Char('[').Between(Whitespace), Parser.Char(']').Between(Whitespace))
            .Select<Action<DataComponentMap>>(operations => map =>
            {
                foreach (var op in operations)
                {
                    op(map);
                }
            });

    /// <summary>
    /// Parses component operations in either bracketed format (<c>[id=val, ...]</c>)
    /// or as a bare comma-separated list (<c>id=val, !removed_id</c>).
    /// </summary>
    [PublicAPI]
    public static readonly Parser<char, Action<DataComponentMap>> StandaloneComponents =
        Parser.OneOf(
            ComponentsBlock,
            ComponentOperations.Select<Action<DataComponentMap>>(operations => map =>
            {
                foreach (var op in operations)
                {
                    op(map);
                }
            })
        );

    /// <summary>
    /// The root parser combinator for an item stack: parses the item ID and an optional component block.
    /// </summary>
    [PublicAPI]
    public static readonly Parser<char, ItemStack> Item =
        Parser.Map(
            (id, opsOpt) =>
            {
                var item = new ItemStack(id);
                if (opsOpt.HasValue)
                {
                    opsOpt.Value(item.Components);
                }
                return item;
            },
            ParserParts.IdentifierParser,
            Parser.Try(ComponentsBlock).Optional()
        );

    /// <summary>
    /// Parses a raw item stack string into an <see cref="ItemStack"/> instance.
    /// </summary>
    /// <param name="input">The item string to parse (e.g., "minecraft:diamond_sword[damage=15, !enchantments]").</param>
    /// <returns>A fully configured <see cref="ItemStack"/> instance with applied component modifications.</returns>
    /// <exception cref="ParseException">Thrown when the input string contains invalid syntax or unknown component tokens.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
    [PublicAPI]
    public static ItemStack Parse(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return Item.Before(Whitespace).Before(Parser<char>.End).ParseOrThrow(input.Trim());
    }

    /// <summary>
    /// Parses a standalone component string (with or without brackets) and applies modifications directly into the target map.
    /// </summary>
    /// <param name="input">The component operations string, e.g. "[unbreakable={}, !custom_name]" or "damage=10".</param>
    /// <param name="target">The component map to update.</param>
    /// <exception cref="ParseException">Thrown when parsing encounters a syntax error.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> or <paramref name="target"/> is null.</exception>
    /// <example>
    /// <code>
    /// ItemStackParser.ApplyComponents("[enchantment_glint_override=true, custom_data={Trophy:1}]", stack.Components);
    /// </code>
    /// </example>
    [PublicAPI]
    public static void ApplyComponents(string input, DataComponentMap target)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(target);

        var trimmed = input.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return;
        }

        var applyAction = StandaloneComponents
            .Between(Whitespace, Whitespace)
            .Before(Parser<char>.End)
            .ParseOrThrow(trimmed);

        applyAction(target);
    }
}