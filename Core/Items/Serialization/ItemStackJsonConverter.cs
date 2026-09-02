using System.Text.Json;
using System.Text.Json.Serialization;
using Core.Serialization;
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
        using var document = JsonDocument.ParseValue(ref reader);
        var rawJson = document.RootElement.GetRawText();

        if (document.RootElement.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException($"Expected a JSON object (compound) for ItemStack, but got {document.RootElement.ValueKind}. Raw JSON: {rawJson}");
        }

        try
        {
            var snbtNode = JsonToSnbtMapper.Map(document.RootElement);

            if (snbtNode is SnbtCompound compound)
            {
                return ItemStack.Parse(compound);
            }

            throw new JsonException($"Expected mapped node to be SnbtCompound, but got {snbtNode.GetType().Name}. Raw JSON: {rawJson}");
        }
        catch (JsonException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new JsonException($"Failed to parse ItemStack. Reason: {ex.Message}. Raw JSON: {rawJson}", ex);
        }
    }

    /// <summary>
    /// Writes a specified <see cref="ItemStack"/> value as JSON.
    /// </summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="value">The <see cref="ItemStack"/> to write.</param>
    /// <param name="options">The serializer options.</param>
    public override void Write(Utf8JsonWriter writer, ItemStack value, JsonSerializerOptions options)
    {
        value.ToSnbt().WriteTo(writer);
    }
}