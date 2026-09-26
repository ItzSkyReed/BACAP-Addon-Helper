using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;

namespace Core.Tests.SNBT.Nodes;

/// <summary>
/// Unit tests for <see cref="SnbtCompound"/>.
/// </summary>
public class SnbtCompoundTests
{
    #region Serialization Tests

    /// <summary>
    /// Verifies that an empty compound serializes to "{}" in both compact and pretty modes.
    /// </summary>
    /// <param name="pretty">Whether pretty printing is enabled.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ToSnbtString_EmptyCompound_ReturnsEmptyBraces(bool pretty)
    {
        // Arrange
        var compound = new SnbtCompound();

        // Act
        var actual = compound.ToSnbtString(pretty);

        // Assert
        Assert.Equal("{}", actual);
    }

    /// <summary>
    /// Verifies compact mode serializes key-value pairs without spaces.
    /// </summary>
    [Fact]
    public void ToSnbtString_CompactMode_FormatsWithoutWhitespace()
    {
        // Arrange
        var tags = new Dictionary<string, ISnbtNode>
        {
            ["id"] = new SnbtString("iron_sword"),
            ["count"] = new SnbtByte(1)
        };
        var compound = new SnbtCompound(tags);

        // Act
        var actual = compound.ToSnbtString(pretty: false);

        // Assert
        Assert.Equal("{id:\"iron_sword\",count:1b}", actual);
    }

    /// <summary>
    /// Verifies pretty mode creates correctly indented multi-line representation.
    /// </summary>
    [Fact]
    public void ToSnbtString_PrettyMode_IndentsCorrectly()
    {
        // Arrange
        var tags = new Dictionary<string, ISnbtNode>
        {
            ["foo"] = new SnbtInt(1),
            ["bar"] = new SnbtBool(true)
        };
        var compound = new SnbtCompound(tags);

        var expected = string.Join(Environment.NewLine, [
            "{",
            "  foo: 1,",
            "  bar: true",
            "}"
        ]);

        // Act
        var actual = compound.ToSnbtString(pretty: true);

        // Assert
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Verifies nested compounds propagate indentation during pretty serialization.
    /// </summary>
    [Fact]
    public void ToSnbtString_NestedCompoundPrettyMode_IndentsRecursively()
    {
        // Arrange
        var inner = new SnbtCompound(new Dictionary<string, ISnbtNode>
        {
            ["sub"] = new SnbtInt(42)
        });
        var outer = new SnbtCompound(new Dictionary<string, ISnbtNode>
        {
            ["parent"] = inner
        });

        var expected = string.Join(Environment.NewLine, [
            "{",
            "  parent: {",
            "    sub: 42",
            "  }",
            "}"
        ]);

        // Act
        var actual = outer.ToSnbtString(pretty: true);

        // Assert
        Assert.Equal(expected, actual);
    }

    #endregion

    #region Key Formatting Specification Tests

    /// <summary>
    /// Verifies that keys containing only allowed characters and starting with letters remain unquoted.
    /// </summary>
    /// <param name="key">The tag name.</param>
    [Theory]
    [InlineData("valid_key")]
    [InlineData("valid-key")]
    [InlineData("valid.key")]
    [InlineData("valid+key")]
    [InlineData("ValidKey123")]
    [InlineData("a_b-c.d+e")]
    public void FormatKey_ValidUnquotedCharacters_RemainsUnquoted(string key)
    {
        // Act
        var actual = SnbtCompound.FormatKey(key);

        // Assert
        Assert.Equal(key, actual);
    }

    /// <summary>
    /// Verifies that keys starting with digits, minus, dot, or plus are quoted per SNBT specification.
    /// </summary>
    /// <param name="key">The invalid starting character key.</param>
    /// <param name="expected">The expected quoted key.</param>
    [Theory]
    [InlineData("123key", "\"123key\"")]
    [InlineData("-key", "\"-key\"")]
    [InlineData(".hidden", "\".hidden\"")]
    [InlineData("+modifier", "\"+modifier\"")]
    [InlineData("", "\"\"")]
    public void FormatKey_InvalidLeadingCharacterOrEmpty_RequiresQuotes(string key, string expected)
    {
        // Act
        var actual = SnbtCompound.FormatKey(key);

        // Assert
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Verifies that keys containing whitespace, special characters, or quotes are quoted and escaped.
    /// </summary>
    /// <param name="key">The key containing special characters.</param>
    /// <param name="expected">The expected escaped and quoted key.</param>
    [Theory]
    [InlineData("key with space", "\"key with space\"")]
    [InlineData("key:value", "\"key:value\"")]
    [InlineData("key\"quote", "\"key\\\"quote\"")]
    [InlineData("key\\slash", "\"key\\\\slash\"")]
    [InlineData("key\nline", "\"key\\nline\"")]
    public void FormatKey_SpecialCharacters_QuotesAndEscapes(string key, string expected)
    {
        // Act
        var actual = SnbtCompound.FormatKey(key);

        // Assert
        Assert.Equal(expected, actual);
    }

    #endregion

    #region Retrieval & Conversion Tests

    /// <summary>
    /// Verifies that direct node access via method and indexer returns the matching node.
    /// </summary>
    [Fact]
    public void GetNode_And_Indexer_ReturnStoredNode()
    {
        // Arrange
        var expected = new SnbtInt(42);
        var compound = new SnbtCompound(new Dictionary<string, ISnbtNode> { ["val"] = expected });

        // Act & Assert
        Assert.Same(expected, compound.GetNode("val"));
        Assert.Same(expected, compound["val"]);
        Assert.Null(compound.GetNode("non_existent"));
        Assert.Null(compound["non_existent"]);
    }

    /// <summary>
    /// Verifies boolean getters correctly extract SnbtBool and coerce SnbtByte (byte-as-boolean).
    /// </summary>
    [Fact]
    public void GetBool_And_GetOptionalBool_SupportBoolAndByteCoercion()
    {
        // Arrange
        var compound = new SnbtCompound(new Dictionary<string, ISnbtNode>
        {
            ["boolTrue"] = new SnbtBool(true),
            ["boolFalse"] = new SnbtBool(false),
            ["byteTrue"] = new SnbtByte(1),
            ["byteFalse"] = new SnbtByte(0),
            ["invalidType"] = new SnbtString("true")
        });

        // Assert
        Assert.True(compound.GetBool("boolTrue"));
        Assert.False(compound.GetBool("boolFalse"));
        Assert.True(compound.GetBool("byteTrue"));
        Assert.False(compound.GetBool("byteFalse"));

        Assert.Equal(true, compound.GetOptionalBool("boolTrue"));
        Assert.Equal(false, compound.GetOptionalBool("boolFalse"));
        Assert.Equal(true, compound.GetOptionalBool("byteTrue"));
        Assert.Equal(false, compound.GetOptionalBool("byteFalse"));

        // Fallbacks
        Assert.False(compound.GetBool("missing"));
        Assert.True(compound.GetBool("missing", defaultValue: true));
        Assert.Null(compound.GetOptionalBool("missing"));
        Assert.Null(compound.GetOptionalBool("invalidType"));
    }

    /// <summary>
    /// Verifies integer getters widen byte and short nodes to 32-bit integers.
    /// </summary>
    [Fact]
    public void GetInt_WideningConversion_ConvertsByteAndShort()
    {
        // Arrange
        var compound = new SnbtCompound(new Dictionary<string, ISnbtNode>
        {
            ["fromByte"] = new SnbtByte(120),
            ["fromShort"] = new SnbtShort(30000),
            ["fromInt"] = new SnbtInt(100000)
        });

        // Assert
        Assert.Equal(120, compound.GetInt("fromByte"));
        Assert.Equal(30000, compound.GetInt("fromShort"));
        Assert.Equal(100000, compound.GetInt("fromInt"));

        Assert.Equal(120, compound.GetOptionalInt("fromByte"));
        Assert.Equal(30000, compound.GetOptionalInt("fromShort"));
        Assert.Equal(100000, compound.GetOptionalInt("fromInt"));
        Assert.Null(compound.GetOptionalInt("missing"));
    }

    /// <summary>
    /// Verifies long getters widen smaller integer types.
    /// </summary>
    [Fact]
    public void GetLong_WideningConversion_ConvertsByteShortInt()
    {
        // Arrange
        var compound = new SnbtCompound(new Dictionary<string, ISnbtNode>
        {
            ["fromByte"] = new SnbtByte(10),
            ["fromShort"] = new SnbtShort(20),
            ["fromInt"] = new SnbtInt(30),
            ["fromLong"] = new SnbtLong(40L)
        });

        // Assert
        Assert.Equal(10L, compound.GetLong("fromByte"));
        Assert.Equal(20L, compound.GetLong("fromShort"));
        Assert.Equal(30L, compound.GetLong("fromInt"));
        Assert.Equal(40L, compound.GetLong("fromLong"));
        Assert.Null(compound.GetOptionalLong("missing"));
    }

    /// <summary>
    /// Verifies floating-point getters convert numeric integer and float nodes.
    /// </summary>
    [Fact]
    public void GetFloat_And_GetDouble_ConvertNumericTypes()
    {
        // Arrange
        var compound = new SnbtCompound(new Dictionary<string, ISnbtNode>
        {
            ["floatVal"] = new SnbtFloat(1.5f),
            ["doubleVal"] = new SnbtDouble(2.5),
            ["intVal"] = new SnbtInt(10)
        });

        // Assert
        Assert.Equal(1.5f, compound.GetFloat("floatVal"));
        Assert.Equal(2.5f, compound.GetFloat("doubleVal"));
        Assert.Equal(10f, compound.GetFloat("intVal"));

        Assert.Equal(2.5, compound.GetDouble("doubleVal"));
        Assert.Equal(1.5, compound.GetDouble("floatVal"), precision: 4);
        Assert.Equal(10.0, compound.GetDouble("intVal"));

        Assert.Null(compound.GetOptionalFloat("missing"));
        Assert.Null(compound.GetOptionalDouble("missing"));
    }

    /// <summary>
    /// Verifies string getters return underlying strings without quotes.
    /// </summary>
    [Fact]
    public void GetString_And_GetOptionalString_ReturnRawStringValue()
    {
        // Arrange
        var compound = new SnbtCompound(new Dictionary<string, ISnbtNode>
        {
            ["name"] = new SnbtString("Steve")
        });

        // Assert
        Assert.Equal("Steve", compound.GetString("name"));
        Assert.Equal("Steve", compound.GetOptionalString("name"));
        Assert.Equal("default", compound.GetString("missing", defaultValue: "default"));
        Assert.Null(compound.GetOptionalString("missing"));
    }

    #endregion

    #region Equality & Constructor Validation Tests

    /// <summary>
    /// Verifies that two distinct compound instances with identical key-value pairs are equal regardless of entry insertion order.
    /// </summary>
    [Fact]
    public void Equality_SameEntriesDifferentOrder_AreEqualAndShareHashCode()
    {
        // Arrange
        var first = new SnbtCompound(new Dictionary<string, ISnbtNode>
        {
            ["a"] = new SnbtInt(1),
            ["b"] = new SnbtInt(2)
        });

        var second = new SnbtCompound(new Dictionary<string, ISnbtNode>
        {
            ["b"] = new SnbtInt(2),
            ["a"] = new SnbtInt(1)
        });

        var different = new SnbtCompound(new Dictionary<string, ISnbtNode>
        {
            ["a"] = new SnbtInt(1),
            ["b"] = new SnbtInt(3)
        });

        // Assert
        Assert.Equal(first, second);
        Assert.True(first == second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());

        Assert.NotEqual(first, different);
        Assert.True(first != different);
    }

    /// <summary>
    /// Verifies that passing null dictionary or dictionary containing null keys/values throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [Fact]
    public void Constructor_NullArguments_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new SnbtCompound(null!));

        var dictWithNullValue = new Dictionary<string, ISnbtNode> { ["key"] = null! };
        Assert.Throws<ArgumentNullException>(() => new SnbtCompound(dictWithNullValue));
    }

    /// <summary>
    /// Verifies that <see cref="SnbtCompound"/> implements <see cref="ISnbtNode"/>.
    /// </summary>
    [Fact]
    public void Implements_ISnbtNode_Interface()
    {
        // Arrange & Act
        var compound = new SnbtCompound();

        // Assert
        Assert.IsType<ISnbtNode>(compound, exactMatch: false);
    }

    #endregion
}