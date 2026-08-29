using Core.DataComponents.Interfaces;
using Core.SNBT;

using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using Core.Utils;
using JetBrains.Annotations;

namespace Core.DataComponents.Components;

/// <summary>
/// Stores custom model data parameters evaluated by client item models for procedural overrides, tints, and predicate dispatching (<c>minecraft:custom_model_data</c>).
/// </summary>
/// <param name="Floats">Optional list of floating-point values evaluated by <c>range_dispatch</c> model predicates.</param>
/// <param name="Flags">Optional list of boolean flags evaluated by <c>condition</c> model predicates.</param>
/// <param name="Strings">Optional list of string identifiers evaluated by <c>select</c> model predicates.</param>
/// <param name="Colors">Optional list of RGB colors (packed 24-bit integers <c>0xRRGGBB</c>) used for dynamic item tint layers.</param>
[UsedImplicitly]
public record CustomModelDataComponent(
    List<float>? Floats = null,
    List<bool>? Flags = null,
    List<string>? Strings = null,
    List<int>? Colors = null
) : ICompoundComponent<CustomModelDataComponent>
{
    /// <inheritdoc/>
    public static string ComponentId => "minecraft:custom_model_data";

    /// <summary>
    /// Parses a <see cref="CustomModelDataComponent"/> directly from an SNBT compound node.
    /// Supports polymorphic color formats (RGB float arrays or packed integers) and boolean flag representations.
    /// </summary>
    /// <param name="compound">The SNBT compound node containing model data lists.</param>
    /// <returns>A populated <see cref="CustomModelDataComponent"/> instance.</returns>
    /// <example>
    /// <code>
    /// var node = SnbtParser.Parse("{floats: [1.0f, 2.5f], flags: [B; 1b, 0b], colors: [16711680, [0.0, 1.0, 0.0]]}");
    /// var component = CustomModelDataComponent.Parse((SnbtCompound)node);
    /// </code>
    /// </example>
    public static CustomModelDataComponent Parse(SnbtCompound compound)
    {
        return new CustomModelDataComponent(
            Floats: ParseFloats(compound.GetNode("floats")),
            Flags: ParseFlags(compound.GetNode("flags")),
            Strings: ParseStrings(compound.GetNode("strings")),
            Colors: ParseColors(compound.GetNode("colors"))
        );
    }

    /// <summary>
    /// Serializes the custom model data into an SNBT compound node.
    /// </summary>
    /// <returns>An <see cref="ISnbtNode"/> representing the model data compound.</returns>
    public ISnbtNode ToSnbt()
    {
        var builder = Snbt.Compound();

        if (Floats is { Count: > 0 })
        {
            builder.PutList("floats", l =>
            {
                foreach (var f in Floats)
                    l.Add(new SnbtFloat(f));
            });
        }

        if (Flags is { Count: > 0 })
        {
            var byteNodes = Flags.Select(ISnbtNode (f) => new SnbtByte((sbyte)(f ? 1 : 0))).ToList();
            builder.Put("flags", new SnbtByteArray(byteNodes));
        }

        if (Strings is { Count: > 0 })
        {
            builder.PutList("strings", l =>
            {
                foreach (var s in Strings)
                    l.Add(new SnbtString(s));
            });
        }

        if (Colors is not { Count: > 0 })
            return builder.Build();
        var intNodes = Colors.Select(ISnbtNode (c) => new SnbtInt(c)).ToList();
        builder.Put("colors", new SnbtIntArray(intNodes));

        return builder.Build();
    }

    private static List<float>? ParseFloats(ISnbtNode? node)
    {
        if (node is not SnbtList list)
            return null;

        var result = new List<float>(list.Items.Count);
        foreach (var item in list.Items)
        {
            switch (item)
            {
                case SnbtFloat f:
                    result.Add(f.Value);
                    break;
                case SnbtDouble d:
                    result.Add((float)d.Value);
                    break;
                case SnbtInt i:
                    result.Add(i.Value);
                    break;
            }
        }
        return result;
    }

    private static List<bool>? ParseFlags(ISnbtNode? node)
    {
        if (node is SnbtByteArray byteArray)
        {
            var result = new List<bool>(byteArray.Items.Count);
            foreach (var item in byteArray.Items)
            {
                if (item is SnbtByte b)
                    result.Add(b.Value != 0);
            }
            return result;
        }

        if (node is SnbtList list)
        {
            var result = new List<bool>(list.Items.Count);
            foreach (var item in list.Items)
            {
                if (item is SnbtByte b)
                    result.Add(b.Value != 0);
            }
            return result;
        }

        return null;
    }

    private static List<string>? ParseStrings(ISnbtNode? node)
    {
        if (node is not SnbtList list)
            return null;

        var result = new List<string>(list.Items.Count);
        foreach (var item in list.Items)
        {
            if (item is SnbtString s)
                result.Add(s.Value);
        }
        return result;
    }

    private static List<int>? ParseColors(ISnbtNode? node)
    {
        if (node is SnbtIntArray intArray)
        {
            var result = new List<int>(intArray.Items.Count);
            foreach (var item in intArray.Items)
            {
                if (item is SnbtInt i)
                    result.Add(i.Value);
            }
            return result;
        }

        if (node is SnbtList list)
        {
            var result = new List<int>(list.Items.Count);
            foreach (var item in list.Items)
            {
                switch (item)
                {
                    case SnbtInt i:
                        result.Add(i.Value);
                        break;
                    case SnbtLong l:
                        result.Add((int)l.Value);
                        break;
                    case SnbtList { Items.Count: >= 3 } rgbList:
                    {
                        var r = GetFloatValue(rgbList.Items[0]);
                        var g = GetFloatValue(rgbList.Items[1]);
                        var b = GetFloatValue(rgbList.Items[2]);
                        result.Add(ColorUtils.PackRgb(r, g, b));
                        break;
                    }
                }
            }
            return result;
        }

        return null;
    }

    private static float GetFloatValue(ISnbtNode node) => node switch
    {
        SnbtFloat f => f.Value,
        SnbtDouble d => (float)d.Value,
        SnbtInt i => i.Value,
        _ => 0.0f
    };
}