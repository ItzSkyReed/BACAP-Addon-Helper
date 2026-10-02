using System.Buffers.Binary;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BacapGenerator.Converters;

/// <summary>
/// A JSON converter that reads Minecraft UUIDs from either a 4-element int array or a string,
/// and writes them as standard hyphenated UUID strings.
/// </summary>
public sealed class MinecraftGuidJsonConverter : JsonConverter<Guid>
{
    /// <summary>
    /// Reads and converts the JSON element to a <see cref="Guid"/>.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="typeToConvert">The target conversion type.</param>
    /// <param name="options">Serializer options.</param>
    /// <returns>The deserialized <see cref="Guid"/>.</returns>
    /// <exception cref="JsonException">Thrown when the token is neither a valid UUID string nor a 4-element integer array.</exception>
    public override Guid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                return reader.TryGetGuid(out var guid)
                    ? guid
                    : throw new JsonException($"Unable to parse \"{reader.GetString()}\" as a valid Guid.");

            case JsonTokenType.StartArray:
            {
                Span<int> ints = stackalloc int[4];
                var count = 0;

                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                {
                    if (count >= 4 || !reader.TryGetInt32(out ints[count]))
                        throw new JsonException("Minecraft UUID array must contain exactly 4 32-bit integers.");

                    count++;
                }

                if (count != 4)
                    throw new JsonException($"Expected 4 integers for UUID, but found {count}.");

                Span<byte> bytes = stackalloc byte[16];
                BinaryPrimitives.WriteInt32BigEndian(bytes[0..4], ints[0]);
                BinaryPrimitives.WriteInt32BigEndian(bytes[4..8], ints[1]);
                BinaryPrimitives.WriteInt32BigEndian(bytes[8..12], ints[2]);
                BinaryPrimitives.WriteInt32BigEndian(bytes[12..16], ints[3]);

                return new Guid(bytes, bigEndian: true);
            }
            default:
                throw new JsonException($"Unexpected token type {reader.TokenType} when parsing UUID.");
        }
    }

    /// <summary>
    /// Writes the <see cref="Guid"/> as a standard hyphenated UUID string.
    /// </summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="value">The <see cref="Guid"/> value to write.</param>
    /// <param name="options">Serializer options.</param>
    public override void Write(Utf8JsonWriter writer, Guid value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}