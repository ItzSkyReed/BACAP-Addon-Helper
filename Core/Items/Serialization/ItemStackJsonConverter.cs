using System.Text.Json;
using System.Text.Json.Serialization;
using Core.SNBT;
using Core.SNBT.Nodes;
using JetBrains.Annotations;

namespace Core.Items.Serialization;

/// <summary>
/// Converts an <see cref="ItemStack"/> to and from JSON by utilizing direct AST mapping.
/// </summary>
public class ItemStackJsonConverter : JsonConverter<ItemStack>
{
    /// <summary>
    /// Reads and converts the JSON to an <see cref="ItemStack"/>.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>A deserialized <see cref="ItemStack"/> instance.</returns>
    /// <exception cref="JsonException">Thrown when the JSON structure cannot be parsed into a valid SNBT compound.</exception>
    [PublicAPI]
    public override ItemStack Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        try
        {
            using var document = JsonDocument.ParseValue(ref reader);

            if (document.RootElement.ValueKind != JsonValueKind.Object)
                throw new JsonException($"Expected a JSON object (compound) for ItemStack, but got {document.RootElement.ValueKind}.");

            var snbtNode = JsonToSnbtMapper.Map(document.RootElement);

            if (snbtNode is SnbtCompound compound)
            {
                return ItemStack.Parse(compound);
            }

            throw new JsonException($"Expected mapped node to be SnbtCompound, but got {snbtNode.GetType().Name}.");
        }
        catch (Exception ex) when (ex is not JsonException)
        {
            throw new JsonException("Failed to parse ItemStack from the provided JSON.", ex);
        }
    }

    /// <summary>
    /// Writes a specified <see cref="ItemStack"/> value as JSON.
    /// </summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="value">The <see cref="ItemStack"/> to write.</param>
    /// <param name="options">The serializer options.</param>
    [PublicAPI]
    public override void Write(Utf8JsonWriter writer, ItemStack value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString("id", value.Id);

        if (value.Count != 1)
            writer.WriteNumber("count", value.Count);

        if (!value.Components.IsEmpty)
        {
            writer.WritePropertyName("components");
            WriteSnbtNode(writer, value.Components.ToSnbt());
        }

        writer.WriteEndObject();
    }

    /// <summary>
    /// Recursively translates an SNBT node into strict JSON tokens.
    /// </summary>
    /// <param name="writer">The JSON writer instance.</param>
    /// <param name="node">The SNBT node to translate.</param>
    private static void WriteSnbtNode(Utf8JsonWriter writer, ISnbtNode node)
    {
        switch (node)
        {
            case SnbtCompound compound:
                writer.WriteStartObject();
                foreach (var (key, val) in compound.Tags)
                {
                    writer.WritePropertyName(key);
                    WriteSnbtNode(writer, val);
                }
                writer.WriteEndObject();
                break;

            case SnbtList list:
                writer.WriteStartArray();
                foreach (var item in CollectionsMarshal.AsSpan(list.Items))
                    WriteSnbtNode(writer, item);
                writer.WriteEndArray();
                break;

            case SnbtByteArray ba:
                writer.WriteStartArray();
                foreach (var item in CollectionsMarshal.AsSpan(ba.Items))
                    WriteSnbtNode(writer, item);
                writer.WriteEndArray();
                break;

            case SnbtIntArray ia:
                writer.WriteStartArray();
                foreach (var item in CollectionsMarshal.AsSpan(ia.Items))
                    WriteSnbtNode(writer, item);
                writer.WriteEndArray();
                break;

            case SnbtLongArray la:
                writer.WriteStartArray();
                foreach (var item in CollectionsMarshal.AsSpan(la.Items))
                    WriteSnbtNode(writer, item);
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