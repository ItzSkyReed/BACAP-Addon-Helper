using System.Globalization;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;

namespace Core.Tests.SNBT.Nodes;

/// <summary>
/// Unit tests for <see cref="SnbtFloat"/>.
/// </summary>
public class SnbtFloatTests
{
    /// <summary>
    /// Verifies that finite floating-point numbers are serialized with invariant formatting and the 'f' suffix.
    /// </summary>
    /// <param name="input">The float value to test.</param>
    /// <param name="expected">The expected SNBT string representation.</param>
    [Theory]
    [InlineData(0f, "0f")]
    [InlineData(1f, "1f")]
    [InlineData(-1f, "-1f")]
    [InlineData(0.5f, "0.5f")]
    [InlineData(-0.25f, "-0.25f")]
    [InlineData(3.14159f, "3.14159f")]
    public void ToSnbtString_ValidFiniteNumbers_ReturnsExpectedStringWithSuffix(float input, string expected)
    {
        // Arrange
        var node = new SnbtFloat(input);

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
        var node = new SnbtFloat(12.34f);

        try
        {
            // Culture that uses comma as decimal separator (e.g., Russian, French, German)
            CultureInfo.CurrentCulture = new CultureInfo("ru-RU");

            // Act
            var actual = node.ToSnbtString();

            // Assert
            Assert.Equal("12.34f", actual);
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
    /// <param name="invalidValue">Non-finite floating-point value.</param>
    [Theory]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    [InlineData(float.NegativeInfinity)]
    public void ToSnbtString_NonFiniteValue_ThrowsInvalidOperationException(float invalidValue)
    {
        // Arrange
        var node = new SnbtFloat(invalidValue);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => node.ToSnbtString());
        Assert.Contains("non-finite", ex.Message);
    }

    /// <summary>
    /// Verifies that formatting options do not affect scalar float serialization.
    /// </summary>
    /// <param name="value">The float value.</param>
    /// <param name="pretty">Whether pretty printing is enabled.</param>
    /// <param name="indent">The indentation string.</param>
    /// <param name="expected">The expected serialized string.</param>
    [Theory]
    [InlineData(5.5f, true, "    ", "5.5f")]
    [InlineData(-5.5f, true, "\t", "-5.5f")]
    [InlineData(0f, false, "  ", "0f")]
    public void ToSnbtString_WithPrettyAndIndent_IgnoresFormattingParameters(
        float value,
        bool pretty,
        string indent,
        string expected)
    {
        // Arrange
        var node = new SnbtFloat(value);

        // Act
        var actual = node.ToSnbtString(pretty, indent);

        // Assert
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Verifies that <see cref="SnbtFloat"/> implements <see cref="ISnbtNode"/>.
    /// </summary>
    [Fact]
    public void Implements_ISnbtNode_Interface()
    {
        // Arrange & Act
        var node = new SnbtFloat(1.0f);

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
        var first = new SnbtFloat(2.5f);
        var second = new SnbtFloat(2.5f);
        var different = new SnbtFloat(-2.5f);

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
    /// <param name="expected">The float value supplied to the constructor.</param>
    [Theory]
    [InlineData(float.MinValue)]
    [InlineData(0f)]
    [InlineData(float.MaxValue)]
    public void ValueProperty_ExposesConstructedValue(float expected)
    {
        // Arrange & Act
        var node = new SnbtFloat(expected);

        // Assert
        Assert.Equal(expected, node.Value);
    }
}