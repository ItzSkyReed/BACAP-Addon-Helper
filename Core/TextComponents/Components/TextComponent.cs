using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Core.DataComponents.Models.Interfaces;
using Core.Serialization;
using Core.SNBT;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using Core.TextComponents.Models;
using JetBrains.Annotations;

namespace Core.TextComponents.Components;

/// <summary>
/// Base abstract record for Minecraft formatted text components (JSON/SNBT Chat Components).
/// </summary>
/// <param name="Style">Optional style rules (color, bold, italic, font, click/hover events).</param>
/// <param name="Extra">Optional list of sibling text components appended after this component.</param>
public abstract record TextComponent(
    TextStyle? Style = null,
    List<TextComponent>? Extra = null
) : IFlexibleModel<TextComponent>
{

    /// <summary>
    /// Parses any valid SNBT node (string literal, shorthand list, or full compound) into a structured <see cref="TextComponent"/>.
    /// </summary>
    /// <param name="node">The SNBT node to parse.</param>
    /// <returns>A populated <see cref="TextComponent"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the provided node is null.</exception>
    [PublicAPI]
    public static TextComponent Parse(ISnbtNode node) => TextComponentParser.Parse(node);

    /// <summary>
    /// Serializes this text component into its corresponding SNBT node representation.
    /// </summary>
    [PublicAPI]
    public abstract ISnbtNode ToSnbt();

    /// <summary>
    /// Helper factory that initializes an <see cref="SnbtCompoundBuilder"/> pre-populated with common <see cref="Style"/> and <see cref="Extra"/> entries.
    /// </summary>
    /// <returns>A new instance of <see cref="SnbtCompoundBuilder"/>.</returns>
    protected SnbtCompoundBuilder CreateBaseBuilder()
    {
        var builder = Snbt.Compound();

        Style?.ApplyTo(builder);

        if (Extra is { Count: > 0 })
        {
            builder.PutList("extra", list =>
            {
                // Uses Span for zero-allocation enumeration and elides array bounds checking
                foreach (var extra in CollectionsMarshal.AsSpan(Extra))
                {
                    list.Add(extra.ToSnbt());
                }
            });
        }

        return builder;
    }

    public string ToJson()
    {
        return JsonSerializer.Serialize(this, GetType(), MinecraftDatapackJsonOptions.Default);
    }

    /// <summary>
    /// Attempts to read a specific property by key from the underlying SNBT representation.
    /// </summary>
    /// <typeparam name="T">The target value type or node type.</typeparam>
    /// <param name="key">The tag name to search for.</param>
    /// <returns>The resolved value if found; otherwise, <see langword="null"/>.</returns>
    /// <example>
    /// <code>
    /// string? text = component.GetTag&lt;string&gt;("text");
    /// </code>
    /// </example>
    public T? GetTag<T>(string key)
    {
        return ToSnbt() is SnbtCompound compound ? compound.GetOptional<T>(key) : default;
    }

    /// <summary>
    /// Implicitly converts a raw string literal into a <see cref="PlainTextComponent"/>.
    /// </summary>
    /// <param name="text">The plain text content.</param>
    public static implicit operator TextComponent(string text) => new PlainTextComponent(text);
}