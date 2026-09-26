using System.Text.Json;
using Core.SNBT;
using Core.SNBT.Nodes;

namespace Core.Tests.SNBT;

/// <summary>
/// Unit tests for <see cref="JsonToSnbtMapper"/>.
/// </summary>
public class JsonToSnbtMapperTests
{
    private static JsonElement ParseJson(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }

    #region Primitive Types Tests

    /// <summary>
    /// Verifies that JSON booleans map directly to <see cref="SnbtBool"/>.
    /// </summary>
    /// <param name="json">The JSON boolean literal.</param>
    /// <param name="expected">The expected bool value.</param>
    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    public void Map_BooleanLiterals_ReturnsSnbtBool(string json, bool expected)
    {
        // Arrange
        var element = ParseJson(json);

        // Act
        var result = JsonToSnbtMapper.Map(element);

        // Assert
        var boolNode = Assert.IsType<SnbtBool>(result);
        Assert.Equal(expected, boolNode.Value);
    }

    /// <summary>
    /// Verifies that JSON strings map to <see cref="SnbtString"/>.
    /// </summary>
    /// <param name="json">The JSON string literal.</param>
    /// <param name="expected">The expected string content.</param>
    [Theory]
    [InlineData("\"hello world\"", "hello world")]
    [InlineData("\"\"", "")]
    [InlineData("\"minecraft:diamond\"", "minecraft:diamond")]
    [InlineData("\"escaped \\\"quotes\\\" and \\n lines\"", "escaped \"quotes\" and \n lines")]
    public void Map_StringLiterals_ReturnsSnbtString(string json, string expected)
    {
        // Arrange
        var element = ParseJson(json);

        // Act
        var result = JsonToSnbtMapper.Map(element);

        // Assert
        var stringNode = Assert.IsType<SnbtString>(result);
        Assert.Equal(expected, stringNode.Value);
    }

    /// <summary>
    /// Verifies that JSON null and undefined tokens map to an empty <see cref="SnbtString"/>.
    /// </summary>
    [Fact]
    public void Map_NullToken_ReturnsEmptySnbtString()
    {
        // Arrange
        var element = ParseJson("null");

        // Act
        var result = JsonToSnbtMapper.Map(element);

        // Assert
        var stringNode = Assert.IsType<SnbtString>(result);
        Assert.Equal(string.Empty, stringNode.Value);
    }

    #endregion

    #region Number Mapping Tests

    /// <summary>
    /// Verifies that integer numbers fitting into Int32 map to <see cref="SnbtInt"/>.
    /// </summary>
    /// <param name="json">The JSON integer representation.</param>
    /// <param name="expected">The expected 32-bit integer.</param>
    [Theory]
    [InlineData("0", 0)]
    [InlineData("42", 42)]
    [InlineData("-100", -100)]
    [InlineData("2147483647", int.MaxValue)]
    [InlineData("-2147483648", int.MinValue)]
    public void Map_Int32Numbers_ReturnsSnbtInt(string json, int expected)
    {
        // Arrange
        var element = ParseJson(json);

        // Act
        var result = JsonToSnbtMapper.Map(element);

        // Assert
        var intNode = Assert.IsType<SnbtInt>(result);
        Assert.Equal(expected, intNode.Value);
    }

    /// <summary>
    /// Verifies that numbers exceeding Int32 but fitting into Int64 map to <see cref="SnbtLong"/>.
    /// </summary>
    /// <param name="json">The JSON long integer representation.</param>
    /// <param name="expected">The expected 64-bit integer.</param>
    [Theory]
    [InlineData("2147483648", 2147483648L)]          // int.MaxValue + 1
    [InlineData("-2147483649", -2147483649L)]        // int.MinValue - 1
    [InlineData("9223372036854775807", long.MaxValue)]
    [InlineData("-9223372036854775808", long.MinValue)]
    public void Map_Int64Numbers_ReturnsSnbtLong(string json, long expected)
    {
        // Arrange
        var element = ParseJson(json);

        // Act
        var result = JsonToSnbtMapper.Map(element);

        // Assert
        var longNode = Assert.IsType<SnbtLong>(result);
        Assert.Equal(expected, longNode.Value);
    }

    /// <summary>
    /// Verifies that floating-point numbers map to <see cref="SnbtDouble"/>.
    /// </summary>
    /// <param name="json">The JSON float representation.</param>
    /// <param name="expected">The expected double-precision value.</param>
    [Theory]
    [InlineData("0.5", 0.5)]
    [InlineData("-12.34", -12.34)]
    [InlineData("1.0", 1.0)]
    [InlineData("1e10", 10000000000.0)]
    [InlineData("3.141592653589793", 3.141592653589793)]
    public void Map_FloatingPointNumbers_ReturnsSnbtDouble(string json, double expected)
    {
        // Arrange
        var element = ParseJson(json);

        // Act
        var result = JsonToSnbtMapper.Map(element);

        // Assert
        var doubleNode = Assert.IsType<SnbtDouble>(result);
        Assert.Equal(expected, doubleNode.Value, precision: 10);
    }

    /// <summary>
    /// Verifies that numbers exceeding double range fall back to an <see cref="SnbtString"/> of the raw text.
    /// </summary>
    [Fact]
    public void Map_NumberExceedingDoubleRange_ReturnsSnbtStringWithRawText()
    {
        // Arrange: A number with hundreds of digits that overflows double
        var bigNumber = "1" + new string('0', 400);
        var element = ParseJson(bigNumber);

        // Act
        var result = JsonToSnbtMapper.Map(element);

        // Assert
        var fallbackNode = Assert.IsType<SnbtString>(result);
        Assert.Equal(bigNumber, fallbackNode.Value);
    }

    #endregion

    #region Array Mapping Tests

    /// <summary>
    /// Verifies that an empty JSON array maps to an empty <see cref="SnbtList"/>.
    /// </summary>
    [Fact]
    public void Map_EmptyArray_ReturnsEmptySnbtList()
    {
        // Arrange
        var element = ParseJson("[]");

        // Act
        var result = JsonToSnbtMapper.Map(element);

        // Assert
        var list = Assert.IsType<SnbtList>(result);
        Assert.Empty(list.Items);
    }

    /// <summary>
    /// Verifies that a JSON array with heterogeneous values correctly maps all items sequentially.
    /// </summary>
    [Fact]
    public void Map_HeterogeneousArray_ReturnsPopulatedSnbtList()
    {
        // Arrange
        var element = ParseJson("[10, \"text\", true, 2.5]");

        // Act
        var result = JsonToSnbtMapper.Map(element);

        // Assert
        var list = Assert.IsType<SnbtList>(result);
        Assert.Equal(4, list.Items.Count);
        Assert.Equal(new SnbtInt(10), list.Items[0]);
        Assert.Equal(new SnbtString("text"), list.Items[1]);
        Assert.Equal(new SnbtBool(true), list.Items[2]);
        Assert.Equal(new SnbtDouble(2.5), list.Items[3]);
    }

    #endregion

    #region Object Mapping Tests

    /// <summary>
    /// Verifies that an empty JSON object maps to an empty <see cref="SnbtCompound"/>.
    /// </summary>
    [Fact]
    public void Map_EmptyObject_ReturnsEmptySnbtCompound()
    {
        // Arrange
        var element = ParseJson("{}");

        // Act
        var result = JsonToSnbtMapper.Map(element);

        // Assert
        var compound = Assert.IsType<SnbtCompound>(result);
        Assert.Empty(compound.Tags);
    }

    /// <summary>
    /// Verifies that a flat JSON object maps properties to key-value tags in <see cref="SnbtCompound"/>.
    /// </summary>
    [Fact]
    public void Map_FlatObject_MapsPropertiesCorrectly()
    {
        // Arrange
        var element = ParseJson("""
        {
            "id": "minecraft:stone",
            "count": 64,
            "unbreakable": true
        }
        """);

        // Act
        var result = JsonToSnbtMapper.Map(element);

        // Assert
        var compound = Assert.IsType<SnbtCompound>(result);
        Assert.Equal(3, compound.Tags.Count);
        Assert.Equal(new SnbtString("minecraft:stone"), compound["id"]);
        Assert.Equal(new SnbtInt(64), compound["count"]);
        Assert.Equal(new SnbtBool(true), compound["unbreakable"]);
    }

    /// <summary>
    /// Verifies that if a JSON object contains duplicate keys, the last occurring property overwrites previous values.
    /// </summary>
    [Fact]
    public void Map_ObjectWithDuplicateKeys_TakesLastOccurringValue()
    {
        // Arrange
        var element = ParseJson("""
        {
            "key": 1,
            "key": 2
        }
        """);

        // Act
        var result = JsonToSnbtMapper.Map(element);

        // Assert
        var compound = Assert.IsType<SnbtCompound>(result);
        Assert.Single(compound.Tags);
        Assert.Equal(new SnbtInt(2), compound["key"]);
    }

    #endregion

    #region Complex & Nested Hierarchy Tests

    /// <summary>
    /// Verifies that deeply nested objects, arrays, and primitives map completely to corresponding SNBT AST nodes.
    /// </summary>
    [Fact]
    public void Map_ComplexNestedStructure_PreservesFullHierarchy()
    {
        // Arrange
        var element = ParseJson("""
        {
            "name": "Sword",
            "enchantments": [
                { "id": "sharpness", "lvl": 5 },
                { "id": "unbreaking", "lvl": 3 }
            ],
            "settings": {
                "display": {
                    "lore": ["Line 1", "Line 2"]
                }
            }
        }
        """);

        // Act
        var result = JsonToSnbtMapper.Map(element);

        // Assert
        var root = Assert.IsType<SnbtCompound>(result);
        Assert.Equal(new SnbtString("Sword"), root["name"]);

        var enchList = Assert.IsType<SnbtList>(root["enchantments"]);
        Assert.Equal(2, enchList.Items.Count);

        var firstEnch = Assert.IsType<SnbtCompound>(enchList.Items[0]);
        Assert.Equal(new SnbtString("sharpness"), firstEnch["id"]);
        Assert.Equal(new SnbtInt(5), firstEnch["lvl"]);

        var settings = Assert.IsType<SnbtCompound>(root["settings"]);
        var display = Assert.IsType<SnbtCompound>(settings["display"]);
        var loreList = Assert.IsType<SnbtList>(display["lore"]);

        Assert.Equal(2, loreList.Items.Count);
        Assert.Equal(new SnbtString("Line 1"), loreList.Items[0]);
        Assert.Equal(new SnbtString("Line 2"), loreList.Items[1]);
    }

    #endregion
}