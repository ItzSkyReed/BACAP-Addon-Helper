using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;

namespace Core.Tests.SNBT.Nodes;

/// <summary>
/// Unit tests for <see cref="SnbtBool"/>.
/// </summary>
public class SnbtBoolTests
{
    /// <summary>
    /// Verifies that <see cref="SnbtBool.ToSnbtString"/> returns valid lowercase SNBT strings.
    /// </summary>
    /// <param name="input">The boolean value to encapsulate.</param>
    /// <param name="expected">The expected SNBT string representation.</param>
    [Theory]
    [InlineData(true, "true")]
    [InlineData(false, "false")]
    public void ToSnbtString_DefaultParameters_ReturnsExpectedSnbtLiteral(bool input, string expected)
    {
        // Arrange
        var node = new SnbtBool(input);

        // Act
        var actual = node.ToSnbtString();

        // Assert
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Ensures that pretty printing flags and indentation prefixes do not alter scalar boolean representation.
    /// </summary>
    /// <param name="input">The boolean value.</param>
    /// <param name="pretty">Whether pretty printing is enabled.</param>
    /// <param name="indent">An indentation string.</param>
    /// <param name="expected">The expected SNBT string.</param>
    [Theory]
    [InlineData(true, true, "    ", "true")]
    [InlineData(false, true, "\t", "false")]
    [InlineData(true, false, "  ", "true")]
    [InlineData(false, false, "", "false")]
    public void ToSnbtString_WithPrettyAndIndent_IgnoresFormattingForScalar(
        bool input,
        bool pretty,
        string indent,
        string expected)
    {
        // Arrange
        var node = new SnbtBool(input);

        // Act
        var actual = node.ToSnbtString(pretty, indent);

        // Assert
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Verifies that <see cref="SnbtBool"/> implements the <see cref="ISnbtNode"/> contract.
    /// </summary>
    [Fact]
    public void Implements_ISnbtNode_Interface()
    {
        // Arrange & Act
        var node = new SnbtBool(true);

        // Assert
        Assert.IsType<ISnbtNode>(node, exactMatch: false);
    }

    /// <summary>
    /// Checks value equality semantics of the record type.
    /// </summary>
    [Fact]
    public void ValueEquality_TwoInstancesWithSameValue_AreEqualAndShareHashCode()
    {
        // Arrange
        var first = new SnbtBool(true);
        var second = new SnbtBool(true);
        var different = new SnbtBool(false);

        // Assert
        Assert.Equal(first, second);
        Assert.True(first == second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());

        Assert.NotEqual(first, different);
        Assert.True(first != different);
    }

    /// <summary>
    /// Ensures positional record property access returns the stored value.
    /// </summary>
    /// <param name="expected">Expected bool value.</param>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ValueProperty_ExposesConstructedState(bool expected)
    {
        // Arrange & Act
        var node = new SnbtBool(expected);

        // Assert
        Assert.Equal(expected, node.Value);
    }
}