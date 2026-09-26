using Core.SNBT;
using Core.SNBT.Nodes;
using Pidgin;
using Xunit;

namespace Core.Tests.SNBT;

/// <summary>
/// Unit tests for <see cref="SnbtParser"/> covering SNBT syntax, types, escapes, and complex structures.
/// </summary>
public class SnbtParserTests
{
    #region Primitives & Boolean Disambiguation Tests

    /// <summary>
    /// Verifies that unquoted boolean literals parse to <see cref="SnbtBool"/>.
    /// </summary>
    /// <param name="raw">The unquoted bool string.</param>
    /// <param name="expected">The expected bool value.</param>
    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    public void Parse_UnquotedBooleans_ReturnsSnbtBool(string raw, bool expected)
    {
        // Act
        var result = SnbtParser.Parse(raw);

        // Assert
        var boolNode = Assert.IsType<SnbtBool>(result);
        Assert.Equal(expected, boolNode.Value);
    }

    /// <summary>
    /// Regression test: Verifies that quoted "true" and "false" literals parse as strings (<see cref="SnbtString"/>), NOT booleans.
    /// </summary>
    /// <param name="raw">The quoted boolean-like string.</param>
    /// <param name="expected">The expected raw string value.</param>
    [Theory]
    [InlineData("\"true\"", "true")]
    [InlineData("\"false\"", "false")]
    [InlineData("'true'", "true")]
    [InlineData("'false'", "false")]
    public void Parse_QuotedBooleans_PreservesSnbtStringType(string raw, string expected)
    {
        // Act
        var result = SnbtParser.Parse(raw);

        // Assert
        var stringNode = Assert.IsType<SnbtString>(result);
        Assert.Equal(expected, stringNode.Value);
    }

    /// <summary>
    /// Verifies that unquoted strings containing allowed characters parse as <see cref="SnbtString"/>.
    /// </summary>
    /// <param name="raw">The unquoted text.</param>
    [Theory]
    [InlineData("stone")]
    [InlineData("diamond_sword")]
    [InlineData("custom.tag-name+1")]
    public void Parse_UnquotedStrings_ReturnsSnbtString(string raw)
    {
        // Act
        var result = SnbtParser.Parse(raw);

        // Assert
        var stringNode = Assert.IsType<SnbtString>(result);
        Assert.Equal(raw, stringNode.Value);
    }

    #endregion

    #region String Escapes Tests

    /// <summary>
    /// Verifies that standard escape sequences are parsed correctly into their ASCII equivalents.
    /// </summary>
    /// <param name="raw">The escaped string literal.</param>
    /// <param name="expected">The unescaped character value.</param>
    [Theory]
    [InlineData("\"Hello\\nWorld\"", "Hello\nWorld")]
    [InlineData("\"Tab\\tSeparated\"", "Tab\tSeparated")]
    [InlineData("\"Carriage\\rReturn\"", "Carriage\rReturn")]
    [InlineData("\"Back\\bspace\"", "Back\bspace")]
    [InlineData("\"Form\\ffeed\"", "Form\ffeed")]
    [InlineData("\"Space\\sSequence\"", "Space Sequence")]
    [InlineData("\"Backslash\\\\Path\"", "Backslash\\Path")]
    [InlineData("\"Quote\\\"Inside\"", "Quote\"Inside")]
    public void Parse_StandardEscapeSequences_UnescapesProperly(string raw, string expected)
    {
        // Act
        var result = SnbtParser.Parse(raw);

        // Assert
        var str = Assert.IsType<SnbtString>(result);
        Assert.Equal(expected, str.Value);
    }

    /// <summary>
    /// Verifies single and double quote nesting without escaping inner opposite quotes.
    /// </summary>
    [Fact]
    public void Parse_OppositeQuotesInside_DoesNotRequireEscaping()
    {
        // Arrange & Act
        var doubleWithSingle = SnbtParser.Parse("\"Hello 'World'!\"");
        var singleWithDouble = SnbtParser.Parse("'Hello \"World\"!'");

        // Assert
        Assert.Equal("Hello 'World'!", Assert.IsType<SnbtString>(doubleWithSingle).Value);
        Assert.Equal("Hello \"World\"!", Assert.IsType<SnbtString>(singleWithDouble).Value);
    }

    /// <summary>
    /// Verifies \x and \u hex unicode escapes.
    /// </summary>
    [Fact]
    public void Parse_UnicodeEscapes_ProducesExpectedCharacters()
    {
        // \x42 is 'B', \u2604 is comet '☄'
        var hexAscii = SnbtParser.Parse("\"\\x42\"");
        var unicodeComet = SnbtParser.Parse("\"\\u2604\"");

        Assert.Equal("B", Assert.IsType<SnbtString>(hexAscii).Value);
        Assert.Equal("\u2604", Assert.IsType<SnbtString>(unicodeComet).Value);
    }

    #endregion

    #region Numeric Routing Tests

    /// <summary>
    /// Verifies that numeric strings with various suffixes and bases route to their corresponding node types.
    /// </summary>
    [Theory]
    [InlineData("123", typeof(SnbtInt))]
    [InlineData("12b", typeof(SnbtByte))]
    [InlineData("31415s", typeof(SnbtShort))]
    [InlineData("10000000000L", typeof(SnbtLong))]
    [InlineData("1.5f", typeof(SnbtFloat))]
    [InlineData("2.5d", typeof(SnbtDouble))]
    [InlineData("0xCAFE", typeof(SnbtInt))]
    [InlineData("0b101", typeof(SnbtInt))]
    public void Parse_NumericLiterals_RoutesToExpectedNodeTypes(string raw, Type expectedType)
    {
        // Act
        var result = SnbtParser.Parse(raw);

        // Assert
        Assert.IsType(expectedType, result);
    }

    #endregion

    #region Arrays Tests

    /// <summary>
    /// Verifies parsing of canonical empty arrays.
    /// </summary>
    /// <param name="raw">The empty array string.</param>
    /// <param name="expectedType">The expected AST array node type.</param>
    [Theory]
    [InlineData("[B;]", typeof(SnbtByteArray))]
    [InlineData("[B;   ]", typeof(SnbtByteArray))]
    [InlineData("[I;]", typeof(SnbtIntArray))]
    [InlineData("[L;]", typeof(SnbtLongArray))]
    public void Parse_EmptyArrays_ReturnsEmptyArrayNodes(string raw, Type expectedType)
    {
        // Act
        var result = SnbtParser.Parse(raw);

        // Assert
        var arrayNode = Assert.IsType<SnbtArrayNode>(result, exactMatch: false);
        Assert.IsType(expectedType, arrayNode);
        Assert.Empty(arrayNode.Items);
    }

    /// <summary>
    /// Verifies populated byte array parsing.
    /// </summary>
    [Fact]
    public void Parse_PopulatedByteArray_ReturnsSnbtByteArray()
    {
        // Act
        var result = SnbtParser.Parse("[B; 1b, -2b, 127b]");

        // Assert
        var byteArray = Assert.IsType<SnbtByteArray>(result);
        Assert.Equal(3, byteArray.Items.Count);
        Assert.Equal(new SnbtByte(1), byteArray.Items[0]);
        Assert.Equal(new SnbtByte(-2), byteArray.Items[1]);
        Assert.Equal(new SnbtByte(127), byteArray.Items[2]);
    }

    /// <summary>
    /// Verifies int array parsing with narrowing types (e.g. byte and short tags).
    /// </summary>
    [Fact]
    public void Parse_IntArrayWithNarrowingTypes_ParsesSuccessfully()
    {
        // Act
        var result = SnbtParser.Parse("[I; 1b, 2s, 3]");

        // Assert
        var intArray = Assert.IsType<SnbtIntArray>(result);
        Assert.Equal(3, intArray.Items.Count);
        Assert.Equal(new SnbtByte(1), intArray.Items[0]);
        Assert.Equal(new SnbtShort(2), intArray.Items[1]);
        Assert.Equal(new SnbtInt(3), intArray.Items[2]);
    }

    #endregion

    #region Lists Tests

    /// <summary>
    /// Verifies that empty bracket syntax parses to empty <see cref="SnbtList"/>.
    /// </summary>
    [Theory]
    [InlineData("[]")]
    [InlineData("[   ]")]
    public void Parse_EmptyList_ReturnsEmptySnbtList(string raw)
    {
        // Act
        var result = SnbtParser.Parse(raw);

        // Assert
        var list = Assert.IsType<SnbtList>(result);
        Assert.Empty(list.Items);
    }

    /// <summary>
    /// Verifies parsing of heterogeneous lists and nested lists.
    /// </summary>
    [Fact]
    public void Parse_HeterogeneousAndNestedLists_ParsesHierarchy()
    {
        // Arrange
        const string raw = "[10, \"sword\", true, [1, 2]]";

        // Act
        var result = SnbtParser.Parse(raw);

        // Assert
        var list = Assert.IsType<SnbtList>(result);
        Assert.Equal(4, list.Items.Count);
        Assert.Equal(new SnbtInt(10), list.Items[0]);
        Assert.Equal(new SnbtString("sword"), list.Items[1]);
        Assert.Equal(new SnbtBool(true), list.Items[2]);

        var nested = Assert.IsType<SnbtList>(list.Items[3]);
        Assert.Equal(2, nested.Items.Count);
        Assert.Equal(new SnbtInt(1), nested.Items[0]);
        Assert.Equal(new SnbtInt(2), nested.Items[1]);
    }

    #endregion

    #region Compounds & Datapack Scenarios

    /// <summary>
    /// Verifies empty compound parsing.
    /// </summary>
    [Theory]
    [InlineData("{}")]
    [InlineData("{    }")]
    public void Parse_EmptyCompound_ReturnsEmptySnbtCompound(string raw)
    {
        // Act
        var result = SnbtParser.Parse(raw);

        // Assert
        var compound = Assert.IsType<SnbtCompound>(result);
        Assert.Empty(compound.Tags);
    }

    /// <summary>
    /// Verifies that compound keys support unquoted, double-quoted, and single-quoted formatting.
    /// </summary>
    [Fact]
    public void Parse_CompoundKeysVariants_ExtractsAllEntries()
    {
        // Arrange
        const string raw = """
        {
            key1: 1,
            "key 2": 2,
            'key:3': 3,
            empty_key: ""
        }
        """;

        // Act
        var result = SnbtParser.Parse(raw);

        // Assert
        var compound = Assert.IsType<SnbtCompound>(result);
        Assert.Equal(4, compound.Tags.Count);
        Assert.Equal(new SnbtInt(1), compound["key1"]);
        Assert.Equal(new SnbtInt(2), compound["key 2"]);
        Assert.Equal(new SnbtInt(3), compound["key:3"]);
        Assert.Equal(new SnbtString(""), compound["empty_key"]);
    }

    /// <summary>
    /// Verifies that duplicate keys in compounds overwrite previous values (last wins).
    /// </summary>
    [Fact]
    public void Parse_DuplicateKeys_OverwritesPreviousValue()
    {
        // Arrange
        const string raw = "{ score: 10, score: 20 }";

        // Act
        var result = SnbtParser.Parse(raw);

        // Assert
        var compound = Assert.IsType<SnbtCompound>(result);
        Assert.Single(compound.Tags);
        Assert.Equal(new SnbtInt(20), compound["score"]);
    }

    /// <summary>
    /// Verifies full real-world Minecraft datapack component structure parsing.
    /// </summary>
    [Fact]
    public void Parse_RealisticMinecraftItemCompound_ParsesCompleteAst()
    {
        // Arrange
        const string raw = """
        {
            id: "minecraft:diamond_sword",
            Count: 1b,
            components: {
                "minecraft:enchantments": {
                    levels: {
                        "minecraft:sharpness": 5,
                        "minecraft:unbreaking": 3
                    }
                },
                "minecraft:custom_data": {
                    CustomFlags: [B; 1b, 0b],
                    Score: 999999L
                }
            }
        }
        """;

        // Act
        var result = SnbtParser.Parse(raw);

        // Assert
        var root = Assert.IsType<SnbtCompound>(result);
        Assert.Equal("minecraft:diamond_sword", root.GetString("id"));
        Assert.Equal(1, root.GetInt("Count"));

        var components = Assert.IsType<SnbtCompound>(root["components"]);
        var enchantments = Assert.IsType<SnbtCompound>(components["minecraft:enchantments"]);
        var levels = Assert.IsType<SnbtCompound>(enchantments["levels"]);
        Assert.Equal(5, levels.GetInt("minecraft:sharpness"));
        Assert.Equal(3, levels.GetInt("minecraft:unbreaking"));

        var customData = Assert.IsType<SnbtCompound>(components["minecraft:custom_data"]);
        var flags = Assert.IsType<SnbtByteArray>(customData["CustomFlags"]);
        Assert.Equal(2, flags.Items.Count);
        Assert.Equal(999999L, customData.GetLong("Score"));
    }

    #endregion

    #region Error Handling & Boundary Tests

    /// <summary>
    /// Verifies that null input immediately throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [Fact]
    public void Parse_NullInput_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => SnbtParser.Parse(null!));
    }

    /// <summary>
    /// Verifies that unparsed trailing garbage after a valid node triggers a <see cref="ParseException"/>.
    /// </summary>
    /// <param name="raw">String containing trailing garbage.</param>
    [Theory]
    [InlineData("{id: 1} trailing_garbage")]
    [InlineData("[1, 2] extra")]
    [InlineData("100b invalid")]
    public void Parse_TrailingGarbage_ThrowsParseException(string raw)
    {
        // Act & Assert
        Assert.ThrowsAny<ParseException>(() => SnbtParser.Parse(raw));
    }

    /// <summary>
    /// Verifies that incomplete or malformed syntax triggers a <see cref="ParseException"/>.
    /// </summary>
    /// <param name="raw">Malformed SNBT input.</param>
    [Theory]
    [InlineData("{unclosed: 1")]
    [InlineData("[1, 2")]
    [InlineData("[B; 1b, 2b")]
    [InlineData("{key: }")]
    public void Parse_MalformedSyntax_ThrowsParseException(string raw)
    {
        // Act & Assert
        Assert.ThrowsAny<ParseException>(() => SnbtParser.Parse(raw));
    }

    #endregion
}