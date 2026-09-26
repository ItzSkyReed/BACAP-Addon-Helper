using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;

namespace Core.Tests.SNBT.Nodes;

/// <summary>
/// Unit tests for <see cref="SnbtString"/>.
/// </summary>
public class SnbtStringTests
{
    /// <summary>
    /// Verifies that an empty string serializes to empty double quotes.
    /// </summary>
    [Fact]
    public void ToSnbtString_EmptyString_ReturnsEmptyDoubleQuotes()
    {
        // Arrange
        var node = new SnbtString(string.Empty);

        // Act
        var actual = node.ToSnbtString();

        // Assert
        Assert.Equal("\"\"", actual);
    }

    /// <summary>
    /// Verifies that strings without escape characters are wrapped in double quotes as-is.
    /// </summary>
    /// <param name="input">The raw string.</param>
    /// <param name="expected">The expected SNBT string.</param>
    [Theory]
    [InlineData("hello", "\"hello\"")]
    [InlineData("minecraft:diamond_sword", "\"minecraft:diamond_sword\"")]
    [InlineData("123", "\"123\"")]
    [InlineData("true", "\"true\"")]
    public void ToSnbtString_PlainStrings_WrapsInQuotesWithoutModifications(string input, string expected)
    {
        // Arrange
        var node = new SnbtString(input);

        // Act
        var actual = node.ToSnbtString();

        // Assert
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Verifies that supported escape sequences (\b, \f, \n, \r, \t, \, ") are properly escaped.
    /// </summary>
    /// <param name="input">String containing characters to escape.</param>
    /// <param name="expected">Expected escaped SNBT string.</param>
    [Theory]
    [InlineData("Hello\nWorld", "\"Hello\\nWorld\"")]
    [InlineData("Tab\tSeparated", "\"Tab\\tSeparated\"")]
    [InlineData("Carriage\rReturn", "\"Carriage\\rReturn\"")]
    [InlineData("Back\bspace", "\"Back\\bspace\"")]
    [InlineData("Form\ffeed", "\"Form\\ffeed\"")]
    [InlineData("Quote: \"value\"", "\"Quote: \\\"value\\\"\"")]
    [InlineData(@"Path\To\File", "\"Path\\\\To\\\\File\"")]
    public void ToSnbtString_EscapableCharacters_EscapesCorrectly(string input, string expected)
    {
        // Arrange
        var node = new SnbtString(input);

        // Act
        var actual = node.ToSnbtString();

        // Assert
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Verifies that multiple complex escape sequences in one string are escaped in a single pass.
    /// </summary>
    [Fact]
    public void ToSnbtString_ComplexMixedEscapes_ProducesExpectedOutput()
    {
        // Arrange: "Hello \"World\"\nLine 2\\Path"
        var node = new SnbtString("Hello \"World\"\nLine 2\\Path");

        // Act
        var actual = node.ToSnbtString();

        // Assert
        Assert.Equal("\"Hello \\\"World\\\"\\nLine 2\\\\Path\"", actual);
    }

    /// <summary>
    /// Verifies that scalar nodes ignore pretty printing flags and indentation prefixes.
    /// </summary>
    /// <param name="pretty">Whether pretty printing is enabled.</param>
    /// <param name="indent">The indentation prefix.</param>
    [Theory]
    [InlineData(true, "    ")]
    [InlineData(true, "\t")]
    [InlineData(false, "  ")]
    public void ToSnbtString_WithPrettyAndIndent_IgnoresFormatting(bool pretty, string indent)
    {
        // Arrange
        var node = new SnbtString("test");

        // Act
        var actual = node.ToSnbtString(pretty, indent);

        // Assert
        Assert.Equal("\"test\"", actual);
    }

    /// <summary>
    /// Verifies that escape sequences positioned in the middle or end of the string trigger escaping.
    /// </summary>
    /// <param name="input">The string with escapable characters not at the start.</param>
    /// <param name="expected">The expected escaped SNBT output.</param>
    [Theory]
    [InlineData("start\nend", "\"start\\nend\"")]
    [InlineData("prefix\"suffix", "\"prefix\\\"suffix\"")]
    [InlineData("trailing\\", "\"trailing\\\\\"")]
    public void ToSnbtString_EscapesInMiddleOrEnd_EscapesProperly(string input, string expected)
    {
        // Arrange
        var node = new SnbtString(input);

        // Act
        var actual = node.ToSnbtString();

        // Assert
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Verifies that passing null to the constructor throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [Fact]
    public void Constructor_NullValue_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new SnbtString(null!));
    }

    /// <summary>
    /// Verifies that <see cref="SnbtString"/> implements <see cref="ISnbtNode"/>.
    /// </summary>
    [Fact]
    public void Implements_ISnbtNode_Interface()
    {
        // Arrange & Act
        var node = new SnbtString("test");

        // Assert
        Assert.IsAssignableFrom<ISnbtNode>(node);
    }

    /// <summary>
    /// Verifies record value equality and hash code consistency.
    /// </summary>
    [Fact]
    public void Equality_SameValues_AreEqualAndShareHashCode()
    {
        // Arrange
        var first = new SnbtString("value");
        var second = new SnbtString("value");
        var different = new SnbtString("different");

        // Assert
        Assert.Equal(first, second);
        Assert.True(first == second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());

        Assert.NotEqual(first, different);
        Assert.True(first != different);
    }

    /// <summary>
    /// Verifies that the public Value property exposes the encapsulated state.
    /// </summary>
    [Fact]
    public void ValueProperty_ExposesConstructedString()
    {
        // Arrange
        const string expected = "test_value";

        // Act
        var node = new SnbtString(expected);

        // Assert
        Assert.Equal(expected, node.Value);
    }
}