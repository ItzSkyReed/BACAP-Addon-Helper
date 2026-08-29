using System.Text.Json;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

namespace Core.SNBT;

/// <summary>
/// Provides high-performance utility methods to map <see cref="System.Text.Json"/> DOM elements directly to SNBT nodes.
/// </summary>
[PublicAPI]
public static class JsonToSnbtMapper
{
    /// <summary>
    /// Converts a <see cref="JsonElement"/> into its corresponding <see cref="ISnbtNode"/> representation.
    /// </summary>
    /// <param name="element">The JSON element to convert.</param>
    /// <returns>An <see cref="ISnbtNode"/> matching the JSON structure.</returns>
    public static ISnbtNode Map(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => ParseJsonObject(element),
            JsonValueKind.Array => ParseJsonArray(element),
            JsonValueKind.String => new SnbtString(element.GetString() ?? ""),
            JsonValueKind.True => new SnbtBool(true),
            JsonValueKind.False => new SnbtBool(false),
            JsonValueKind.Number => ParseJsonNumber(element),
            _ => new SnbtString("") // Fallback for Null/Undefined
        };
    }

    /// <summary>
    /// Parses a JSON object into an <see cref="SnbtCompound"/>.
    /// </summary>
    /// <param name="element">The JSON object element.</param>
    /// <returns>A populated <see cref="SnbtCompound"/>.</returns>
    private static SnbtCompound ParseJsonObject(JsonElement element)
    {
        var dict = new Dictionary<string, ISnbtNode>();
        foreach (var prop in element.EnumerateObject())
        {
            dict[prop.Name] = Map(prop.Value);
        }
        return new SnbtCompound(dict);
    }

    /// <summary>
    /// Parses a JSON array into an <see cref="SnbtList"/>.
    /// </summary>
    /// <param name="element">The JSON array element.</param>
    /// <returns>A populated <see cref="SnbtList"/>.</returns>
    private static SnbtList ParseJsonArray(JsonElement element)
    {
        var length = element.GetArrayLength();

        // Return empty list early to avoid allocation if length is 0
        if (length == 0) return new SnbtList([]);

        var list = new List<ISnbtNode>(length);

        foreach (var item in element.EnumerateArray())
            list.Add(Map(item));
        return new SnbtList(list);
    }

    /// <summary>
    /// Parses a JSON number into the tightest fitting SNBT numeric type.
    /// </summary>
    /// <param name="element">The JSON number element.</param>
    /// <returns>An instance of <see cref="SnbtInt"/>, <see cref="SnbtLong"/>, or <see cref="SnbtDouble"/>.</returns>
    private static ISnbtNode ParseJsonNumber(JsonElement element)
    {
        if (element.TryGetInt32(out var i)) return new SnbtInt(i);
        if (element.TryGetInt64(out var l)) return new SnbtLong(l);
        if (element.TryGetDouble(out var d)) return new SnbtDouble(d);

        return new SnbtString(element.GetRawText());
    }
}