using System.Globalization;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;

namespace Core.Tests.SNBT.Nodes;

/// <summary>
/// Unit tests for <see cref="SnbtLong"/>.
/// </summary>
public class SnbtLongTests
{
    /// <summary>
    /// Verifies that <see cref="SnbtLong.ToSnbtString"/> appends the 'L' suffix to 64-bit integer values.
    /// </summary>
    /// <param name="input">The long value to test.</param>
    /// <param name="expected">The expected SNBT string representation.</param>
    [Theory]
    [InlineData(0L, "0L")]
    [InlineData(1L, "1L")]
    [InlineData(-1L, "-1L")]
    [InlineData(31415926L, "31415926L")]
    [InlineData(-31415926L, "-31415926L")]
    [InlineData(long.MaxValue, "9223372036854775807L")]
    [InlineData(long.MinValue, "-9223372036854775808L")]
    public void ToSnbtString_DefaultParameters_ReturnsExpectedValueWithSuffix(long input, string expected)
    {
        // Arrange
        var node = new SnbtLong(input);

        // Act
        var actual = node.ToSnbtString();

        // Assert
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Ensures that serialization produces invariant ASCII formatting across different thread cultures.
    /// </summary>
    [Fact]
    public void ToSnbtString_UnderCustomCulture_ProducesConsistentAsciiRepresentation()
    {
        // Arrange
        var previousCulture = CultureInfo.CurrentCulture;
        var node = new SnbtLong(-9223372036854775808L);

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("ar-EG");

            // Act
            var actual = node.ToSnbtString();

            // Assert
            Assert.Equal("-9223372036854775808L", actual);
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }
    }

    /// <summary>
    /// Verifies that scalar nodes ignore pretty printing flags and indentation prefixes.
    /// </summary>
    /// <param name="value">The long value.</param>
    /// <param name="pretty">Whether pretty printing is enabled.</param>
    /// <param name="indent">The indentation prefix.</param>
    /// <param name="expected">The expected serialized string.</param>
    [Theory]
    [InlineData(5000000000L, true, "    ", "5000000000L")]
    [InlineData(-5000000000L, true, "\t\t", "-5000000000L")]
    [InlineData(0L, false, "  ", "0L")]
    public void ToSnbtString_WithPrettyAndIndent_IgnoresIndentation(
        long value,
        bool pretty,
        string indent,
        string expected)
    {
        // Arrange
        var node = new SnbtLong(value);

        // Act
        var actual = node.ToSnbtString(pretty, indent);

        // Assert
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Verifies that <see cref="SnbtLong"/> implements <see cref="ISnbtNode"/>.
    /// </summary>
    [Fact]
    public void Implements_ISnbtNode_Interface()
    {
        // Arrange & Act
        var node = new SnbtLong(100L);

        // Assert
        Assert.IsAssignableFrom<ISnbtNode>(node);
    }

    /// <summary>
    /// Verifies value equality and hash code parity between identical record instances.
    /// </summary>
    [Fact]
    public void Equality_SameValues_AreEqualAndShareHashCode()
    {
        // Arrange
        var first = new SnbtLong(1000L);
        var second = new SnbtLong(1000L);
        var different = new SnbtLong(-1000L);

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
    /// <param name="expected">The long value passed to constructor.</param>
    [Theory]
    [InlineData(long.MinValue)]
    [InlineData(0L)]
    [InlineData(long.MaxValue)]
    public void ValueProperty_ExposesConstructedValue(long expected)
    {
        // Arrange & Act
        var node = new SnbtLong(expected);

        // Assert
        Assert.Equal(expected, node.Value);
    }
}