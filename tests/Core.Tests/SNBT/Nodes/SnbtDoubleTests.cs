using System.Globalization;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;

namespace Core.Tests.SNBT.Nodes;

/// <summary>
/// Unit tests for <see cref="SnbtDouble"/>.
/// </summary>
public class SnbtDoubleTests
{
    /// <summary>
    /// Verifies that finite floating-point values serialize correctly with invariant culture and the 'd' suffix.
    /// </summary>
    /// <param name="input">The double value to test.</param>
    /// <param name="expected">The expected SNBT string representation.</param>
    [Theory]
    [InlineData(0.0, "0d")]
    [InlineData(1.0, "1d")]
    [InlineData(-1.0, "-1d")]
    [InlineData(0.5, "0.5d")]
    [InlineData(-0.25, "-0.25d")]
    [InlineData(3.1415926, "3.1415926d")]
    [InlineData(-1234.5678, "-1234.5678d")]
    public void ToSnbtString_ValidFiniteNumbers_ReturnsExpectedStringWithSuffix(double input, string expected)
    {
        // Arrange
        var node = new SnbtDouble(input);

        // Act
        var actual = node.ToSnbtString();

        // Assert
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Ensures that floating-point numbers always use dot as the decimal separator regardless of thread culture.
    /// </summary>
    [Fact]
    public void ToSnbtString_CultureWithCommaSeparator_AlwaysUsesInvariantDot()
    {
        // Arrange
        var previousCulture = CultureInfo.CurrentCulture;
        var node = new SnbtDouble(99.99);

        try
        {
            // Culture that uses comma as decimal separator
            CultureInfo.CurrentCulture = new CultureInfo("ru-RU");

            // Act
            var actual = node.ToSnbtString();

            // Assert
            Assert.Equal("99.99d", actual);
            Assert.DoesNotContain(",", actual);
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }
    }

    /// <summary>
    /// Verifies that attempting to serialize NaN, PositiveInfinity, or NegativeInfinity throws an <see cref="InvalidOperationException"/>.
    /// </summary>
    /// <param name="invalidValue">Non-finite double value.</param>
    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void ToSnbtString_NonFiniteValue_ThrowsInvalidOperationException(double invalidValue)
    {
        // Arrange
        var node = new SnbtDouble(invalidValue);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => node.ToSnbtString());
        Assert.Contains("non-finite", ex.Message);
    }

    /// <summary>
    /// Verifies that formatting options do not affect scalar double serialization.
    /// </summary>
    /// <param name="value">The double value.</param>
    /// <param name="pretty">Whether pretty printing is enabled.</param>
    /// <param name="indent">The indentation string.</param>
    /// <param name="expected">The expected serialized string.</param>
    [Theory]
    [InlineData(12.34, true, "    ", "12.34d")]
    [InlineData(-12.34, true, "\t", "-12.34d")]
    [InlineData(0.0, false, "  ", "0d")]
    public void ToSnbtString_WithPrettyAndIndent_IgnoresFormattingParameters(
        double value,
        bool pretty,
        string indent,
        string expected)
    {
        // Arrange
        var node = new SnbtDouble(value);

        // Act
        var actual = node.ToSnbtString(pretty, indent);

        // Assert
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Verifies that <see cref="SnbtDouble"/> implements <see cref="ISnbtNode"/>.
    /// </summary>
    [Fact]
    public void Implements_ISnbtNode_Interface()
    {
        // Arrange & Act
        var node = new SnbtDouble(1.0);

        // Assert
        Assert.IsType<ISnbtNode>(node, exactMatch: false);
    }

    /// <summary>
    /// Verifies record value equality and hash code consistency.
    /// </summary>
    [Fact]
    public void Equality_SameValues_AreEqualAndShareHashCode()
    {
        // Arrange
        var first = new SnbtDouble(3.14159);
        var second = new SnbtDouble(3.14159);
        var different = new SnbtDouble(-3.14159);

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
    /// <param name="expected">The double value supplied to constructor.</param>
    [Theory]
    [InlineData(double.MinValue)]
    [InlineData(0.0)]
    [InlineData(double.MaxValue)]
    public void ValueProperty_ExposesConstructedValue(double expected)
    {
        // Arrange & Act
        var node = new SnbtDouble(expected);

        // Assert
        Assert.Equal(expected, node.Value);
    }
}