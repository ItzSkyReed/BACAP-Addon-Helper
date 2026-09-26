using Core.SNBT;
using Core.SNBT.Nodes;

namespace Core.Tests.SNBT;

/// <summary>
/// Unit tests for <see cref="SnbtNumberParser"/> validating all numeric literal syntax rules of Minecraft SNBT.
/// </summary>
public class SnbtNumberParserTests
{
    #region Hexadecimal & Binary Parsing Tests

    /// <summary>
    /// Verifies that hex numbers containing digits E, D, and F are parsed as integers, NOT floats.
    /// </summary>
    [Theory]
    [InlineData("0xbad", 2989)]
    [InlineData("0xCAFE", 51966)]
    [InlineData("0x1E", 30)]
    [InlineData("0xDEAD", 57005)]
    [InlineData("0x1f", 31)]
    [InlineData("-0x10", -16)]
    public void Parse_HexIntegersWithFloatLikeDigits_ParsesAsSnbtInt(string raw, int expected)
    {
        // Act
        var result = SnbtNumberParser.Parse(raw);

        // Assert
        var node = Assert.IsType<SnbtInt>(result);
        Assert.Equal(expected, node.Value);
    }

    /// <summary>
    /// Verifies binary integer parsing with and without type suffixes.
    /// </summary>
    [Theory]
    [InlineData("0b101", 5)]
    [InlineData("0b0", 0)]
    [InlineData("0b1111", 15)]
    [InlineData("-0b10", -2)]
    public void Parse_BinaryIntegers_ReturnsSnbtInt(string raw, int expected)
    {
        // Act
        var result = SnbtNumberParser.Parse(raw);

        // Assert
        var node = Assert.IsType<SnbtInt>(result);
        Assert.Equal(expected, node.Value);
    }

    /// <summary>
    /// Verifies that binary numbers with byte suffix produce <see cref="SnbtByte"/>.
    /// </summary>
    [Fact]
    public void Parse_BinaryWithByteSuffix_ReturnsSnbtByte()
    {
        // Act
        var result = SnbtNumberParser.Parse("0b101b");

        // Assert
        var node = Assert.IsType<SnbtByte>(result);
        Assert.Equal((sbyte)5, node.Value);
    }

    /// <summary>
    /// Verifies canonical byte zero representations.
    /// </summary>
    [Theory]
    [InlineData("0b")]
    [InlineData("0B")]
    [InlineData("-0b")]
    [InlineData("+0b")]
    public void Parse_ByteZeroSpecialCases_ReturnsSnbtByteZero(string raw)
    {
        // Act
        var result = SnbtNumberParser.Parse(raw);

        // Assert
        var node = Assert.IsType<SnbtByte>(result);
        Assert.Equal((sbyte)0, node.Value);
    }

    /// <summary>
    /// Verifies that non-binary digits in binary literals throw <see cref="FormatException"/>.
    /// </summary>
    [Theory]
    [InlineData("0b123")]
    [InlineData("0b9")]
    [InlineData("0bA")]
    public void Parse_InvalidBinaryDigits_ThrowsFormatException(string raw)
    {
        // Act & Assert
        Assert.Throws<FormatException>(() => SnbtNumberParser.Parse(raw));
    }

    /// <summary>
    /// Verifies that incomplete hex/bin prefixes throw <see cref="FormatException"/>.
    /// </summary>
    [Theory]
    [InlineData("0x")]
    [InlineData("-0x")]
    public void Parse_IncompletePrefix_ThrowsFormatException(string raw)
    {
        // Act & Assert
        Assert.Throws<FormatException>(() => SnbtNumberParser.Parse(raw));
    }

    #endregion

    #region Signedness Suffixes & Two's Complement Tests

    /// <summary>
    /// Verifies equivalent byte representations from Minecraft specification (-16b, -16sb, 240uB).
    /// </summary>
    [Theory]
    [InlineData("-16b", (sbyte)-16)]
    [InlineData("-16sb", (sbyte)-16)]
    [InlineData("240uB", (sbyte)-16)] // 240 unsigned byte -> -16 two's complement
    [InlineData("34B", (sbyte)34)]
    [InlineData("-20b", (sbyte)-20)]
    [InlineData("127b", (sbyte)127)]
    [InlineData("-128b", (sbyte)-128)]
    public void Parse_ByteSignednessAndTwosComplement_ReturnsExpectedSnbtByte(string raw, sbyte expected)
    {
        // Act
        var result = SnbtNumberParser.Parse(raw);

        // Assert
        var node = Assert.IsType<SnbtByte>(result);
        Assert.Equal(expected, node.Value);
    }

    /// <summary>
    /// Verifies equivalent short representations from Minecraft specification (15s, 15sS, 15Us).
    /// </summary>
    [Theory]
    [InlineData("15s", (short)15)]
    [InlineData("15sS", (short)15)]
    [InlineData("15Us", (short)15)]
    [InlineData("31415s", (short)31415)]
    [InlineData("-27183s", (short)-27183)]
    public void Parse_ShortSignednessVariants_ReturnsExpectedSnbtShort(string raw, short expected)
    {
        // Act
        var result = SnbtNumberParser.Parse(raw);

        // Assert
        var node = Assert.IsType<SnbtShort>(result);
        Assert.Equal(expected, node.Value);
    }

    /// <summary>
    /// Verifies that invalid signedness syntax specified in Minecraft rules throws <see cref="FormatException"/>.
    /// </summary>
    [Theory]
    [InlineData("82u")]   // Missing data type suffix after 'u'
    [InlineData("-87uI")] // Unsigned integer cannot have a negative sign
    [InlineData("30bu")]  // Wrong suffix order ('u' must precede 'b')
    public void Parse_InvalidSignednessSyntax_ThrowsFormatException(string raw)
    {
        // Act & Assert
        Assert.Throws<FormatException>(() => SnbtNumberParser.Parse(raw));
    }

    /// <summary>
    /// Verifies that values exceeding type range throw <see cref="OverflowException"/> (e.g., 253sb).
    /// </summary>
    [Theory]
    [InlineData("253sb")]   // 253 exceeds signed byte range [-128, 127]
    [InlineData("256uB")]   // 256 exceeds unsigned byte range [0, 255]
    [InlineData("65536uS")] // 65536 exceeds unsigned short range [0, 65535]
    [InlineData("32768s")]  // 32768 exceeds signed short range [-32768, 32767]
    public void Parse_OutOfRangeValues_ThrowsOverflowException(string raw)
    {
        // Act & Assert
        Assert.Throws<OverflowException>(() => SnbtNumberParser.Parse(raw));
    }

    #endregion

    #region Floating-Point & Scientific Notation Tests

    /// <summary>
    /// Verifies floating-point literals with omitted whole or fraction parts (.1, 1., .5f).
    /// </summary>
    [Theory]
    [InlineData(".1", 0.1)]
    [InlineData("1.", 1.0)]
    [InlineData("3.1415926", 3.1415926)]
    [InlineData("10d", 10.0)]
    public void Parse_DoubleVariants_ReturnsSnbtDouble(string raw, double expected)
    {
        // Act
        var result = SnbtNumberParser.Parse(raw);

        // Assert
        var node = Assert.IsType<SnbtDouble>(result);
        Assert.Equal(expected, node.Value, precision: 6);
    }

    /// <summary>
    /// Verifies 32-bit float literals with the 'f' suffix.
    /// </summary>
    [Theory]
    [InlineData(".5f", 0.5f)]
    [InlineData("1.f", 1.0f)]
    [InlineData("3.1415926f", 3.1415926f)]
    [InlineData("10f", 10.0f)]
    public void Parse_FloatVariants_ReturnsSnbtFloat(string raw, float expected)
    {
        // Act
        var result = SnbtNumberParser.Parse(raw);

        // Assert
        var node = Assert.IsType<SnbtFloat>(result);
        Assert.Equal(expected, node.Value, precision: 5);
    }

    /// <summary>
    /// Verifies numbers with scientific E notation.
    /// </summary>
    [Theory]
    [InlineData("1.2e3", 1200.0)]
    [InlineData("87E48", 87e48)]
    [InlineData("0.1e-1", 0.01)]
    public void Parse_ScientificNotation_ReturnsSnbtDouble(string raw, double expected)
    {
        // Act
        var result = SnbtNumberParser.Parse(raw);

        // Assert
        var node = Assert.IsType<SnbtDouble>(result);
        Assert.Equal(expected, node.Value, precision: 6);
    }

    #endregion

    #region Underscore Placement Tests

    /// <summary>
    /// Verifies valid underscore placement between digits per SNBT specification.
    /// </summary>
    [Theory]
    [InlineData("0b10_01", 9)]
    [InlineData("0xAB_CD", 43981)]
    [InlineData("1_000_000", 1000000)]
    public void Parse_ValidUnderscoresInIntegers_ParsesCorrectly(string raw, int expected)
    {
        // Act
        var result = SnbtNumberParser.Parse(raw);

        // Assert
        var node = Assert.IsType<SnbtInt>(result);
        Assert.Equal(expected, node.Value);
    }

    /// <summary>
    /// Verifies valid multiple underscores in floats (e.g. 1_2.3_4__5f).
    /// </summary>
    [Fact]
    public void Parse_ValidUnderscoresInFloat_ParsesCorrectly()
    {
        // Act
        var result = SnbtNumberParser.Parse("1_2.3_4__5f");

        // Assert
        var node = Assert.IsType<SnbtFloat>(result);
        Assert.Equal(12.345f, node.Value, precision: 3);
    }

    /// <summary>
    /// Verifies that invalid underscore placements (start, end, adjacent to symbols/prefixes) throw <see cref="FormatException"/>.
    /// </summary>
    [Theory]
    [InlineData("_123")]
    [InlineData("123_")]
    [InlineData("0x_12")]
    [InlineData("0b_10")]
    [InlineData("1_.2")]
    [InlineData("1._2")]
    [InlineData("123_b")]
    [InlineData("1.5_f")]
    public void Parse_InvalidUnderscorePlacement_ThrowsFormatException(string raw)
    {
        // Act & Assert
        Assert.Throws<FormatException>(() => SnbtNumberParser.Parse(raw));
    }

    #endregion
}