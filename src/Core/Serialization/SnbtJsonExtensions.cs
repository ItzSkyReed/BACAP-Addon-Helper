using System.Runtime.InteropServices;
using System.Text.Json;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

namespace Core.Serialization;

/// <summary>
/// Provides extension methods for bridging SNBT nodes and System.Text.Json writing capabilities.
/// </summary>
public static class SnbtJsonExtensions
{
    /// <summary>
    /// Recursively translates an SNBT node into strict JSON tokens and writes them to the provided writer.
    /// </summary>
    /// <param name="node">The source SNBT node to translate.</param>
    /// <param name="writer">The JSON writer instance to output the tokens to.</param>
    /// <exception cref="ArgumentNullException">Thrown when the writer is null.</exception>
    /// <example>
    /// <code>
    /// mySnbtNode.WriteTo(jsonWriter);
    /// </code>
    /// </example>
    [PublicAPI]
    public static void WriteTo(this ISnbtNode node, Utf8JsonWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        switch (node)
        {
            case SnbtCompound compound:
                writer.WriteStartObject();
                foreach (var (key, val) in compound.Tags)
                {
                    writer.WritePropertyName(key);
                    val.WriteTo(writer); // Recursive call using extension method
                }
                writer.WriteEndObject();
                break;

            case SnbtList list:
                writer.WriteStartArray();
                foreach (var item in CollectionsMarshal.AsSpan(list.Items))
                    item.WriteTo(writer);
                writer.WriteEndArray();
                break;

            case SnbtByteArray ba:
                writer.WriteStartArray();
                foreach (var item in CollectionsMarshal.AsSpan(ba.Items))
                    item.WriteTo(writer);
                writer.WriteEndArray();
                break;

            case SnbtIntArray ia:
                writer.WriteStartArray();
                foreach (var item in CollectionsMarshal.AsSpan(ia.Items))
                    item.WriteTo(writer);
                writer.WriteEndArray();
                break;

            case SnbtLongArray la:
                writer.WriteStartArray();
                foreach (var item in CollectionsMarshal.AsSpan(la.Items))
                    item.WriteTo(writer);
                writer.WriteEndArray();
                break;

            case SnbtString str: writer.WriteStringValue(str.Value); break;
            case SnbtBool bl: writer.WriteBooleanValue(bl.Value); break;
            case SnbtByte b: writer.WriteNumberValue(b.Value); break;
            case SnbtShort s: writer.WriteNumberValue(s.Value); break;
            case SnbtInt i: writer.WriteNumberValue(i.Value); break;
            case SnbtLong l: writer.WriteNumberValue(l.Value); break;
            case SnbtFloat f: writer.WriteNumberValue(f.Value); break;
            case SnbtDouble d: writer.WriteNumberValue(d.Value); break;

            default:
                writer.WriteNullValue();
                break;
        }
    }
}