using System.Globalization;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;

namespace Core.Tests.SNBT.Nodes;

/// <summary>
/// Unit tests for <see cref="SnbtInt"/>.
/// </summary>
public class SnbtIntTests
{
    /// <summary>
    /// Verifies that <see cref="SnbtInt.ToSnbtString"/> returns canonical decimal representations without type suffixes.
    /// </summary>
    /// <param name="input">The 32-bit integer value.</param>
    /// <param name="expected">The expected SNBT string representation.</param>
    [Theory]
    [InlineData(0, "0")]
    [InlineData(1, "1")]
    [InlineData(-1, "-1")]
    [InlineData(31415926, "31415926")]
    [InlineData(-31415926, "-31415926")]
    [InlineData(int.MaxValue, "2147483647")]
    [InlineData(int.MinValue, "-2147483648")]
    public void ToSnbtString_DefaultParameters_ReturnsExpectedCanonicalRepresentation(int input, string expected)
    {
        // Arrange
        var node = new SnbtInt(input);

        // Act
        var actual = node.ToSnbtString();

        // Assert
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Ensures that serialization produces invariant ASCII formatting across different cultures.
    /// </summary>
    [Fact]
    public void ToSnbtString_UnderCustomCulture_ProducesConsistentAsciiRepresentation()
    {
        // Arrange
        var previousCulture = CultureInfo.CurrentCulture;
        var node = new SnbtInt(-2147483648);

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("ar-EG");

            // Act
            var actual = node.ToSnbtString();

            // Assert
            Assert.Equal("-2147483648", actual);
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }
    }

    /// <summary>
    /// Verifies that scalar nodes ignore pretty printing flags and indentation prefixes.
    /// </summary>
    /// <param name="value">The integer value.</param>
    /// <param name="pretty">Whether pretty printing is enabled.</param>
    /// <param name="indent">The indentation prefix.</param>
    /// <param name="expected">The expected serialized string.</param>
    [Theory]
    [InlineData(1000, true, "    ", "1000")]
    [InlineData(-1000, true, "\t\t", "-1000")]
    [InlineData(0, false, "  ", "0")]
    public void ToSnbtString_WithPrettyAndIndent_IgnoresIndentation(
        int value,
        bool pretty,
        string indent,
        string expected)
    {
        // Arrange
        var node = new SnbtInt(value);

        // Act
        var actual = node.ToSnbtString(pretty, indent);

        // Assert
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Verifies that <see cref="SnbtInt"/> implements <see cref="ISnbtNode"/>.
    /// </summary>
    [Fact]
    public void Implements_ISnbtNode_Interface()
    {
        // Arrange & Act
        var node = new SnbtInt(42);

        // Assert
        Assert.IsType<ISnbtNode>(node, exactMatch: false);
    }

    /// <summary>
    /// Verifies value equality and hash code parity between identical record instances.
    /// </summary>
    [Fact]
    public void Equality_SameValues_AreEqualAndShareHashCode()
    {
        // Arrange
        var first = new SnbtInt(42);
        var second = new SnbtInt(42);
        var different = new SnbtInt(-42);

        // Assert
        Assert.Equal(first, second);
        Assert.True(first == second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());

        Assert.NotEqual(first, different);
        Assert.True(first != different);
    }

    /// <summary>
    /// Verifies that constructor argument is exposed via the public Value property.
    /// </summary>
    /// <param name="expected">The integer value passed to constructor.</param>
    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(0)]
    [InlineData(int.MaxValue)]
    public void ValueProperty_ExposesConstructedValue(int expected)
    {
        // Arrange & Act
        var node = new SnbtInt(expected);

        // Assert
        Assert.Equal(expected, node.Value);
    }
}