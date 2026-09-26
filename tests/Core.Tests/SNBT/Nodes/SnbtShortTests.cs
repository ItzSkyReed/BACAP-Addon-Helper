using System.Globalization;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;

namespace Core.Tests.SNBT.Nodes;

/// <summary>
/// Unit tests for <see cref="SnbtShort"/>.
/// </summary>
public class SnbtShortTests
{
    /// <summary>
    /// Verifies that <see cref="SnbtShort.ToSnbtString"/> appends the 's' suffix to signed 16-bit integers.
    /// </summary>
    /// <param name="input">The short value to test.</param>
    /// <param name="expected">The expected SNBT string representation.</param>
    [Theory]
    [InlineData((short)0, "0s")]
    [InlineData((short)1, "1s")]
    [InlineData((short)-1, "-1s")]
    [InlineData((short)32767, "32767s")]     // short.MaxValue
    [InlineData((short)-32768, "-32768s")]   // short.MinValue
    [InlineData((short)31415, "31415s")]
    [InlineData((short)-27183, "-27183s")]
    public void ToSnbtString_DefaultParameters_ReturnsExpectedValueWithSuffix(short input, string expected)
    {
        // Arrange
        var node = new SnbtShort(input);

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
        var node = new SnbtShort(-32768);

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("ar-EG");

            // Act
            var actual = node.ToSnbtString();

            // Assert
            Assert.Equal("-32768s", actual);
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }
    }

    /// <summary>
    /// Verifies that scalar nodes ignore indentation and pretty printing options.
    /// </summary>
    /// <param name="value">The short integer value.</param>
    /// <param name="pretty">Whether pretty printing is enabled.</param>
    /// <param name="indent">The indentation prefix.</param>
    /// <param name="expected">The expected serialized string.</param>
    [Theory]
    [InlineData((short)100, true, "    ", "100s")]
    [InlineData((short)-100, true, "\t", "-100s")]
    [InlineData((short)0, false, "  ", "0s")]
    public void ToSnbtString_WithPrettyAndIndent_IgnoresIndentation(
        short value,
        bool pretty,
        string indent,
        string expected)
    {
        // Arrange
        var node = new SnbtShort(value);

        // Act
        var actual = node.ToSnbtString(pretty, indent);

        // Assert
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Verifies that <see cref="SnbtShort"/> implements <see cref="ISnbtNode"/>.
    /// </summary>
    [Fact]
    public void Implements_ISnbtNode_Interface()
    {
        // Arrange & Act
        var node = new SnbtShort(10);

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
        var first = new SnbtShort(15);
        var second = new SnbtShort(15);
        var different = new SnbtShort(-15);

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
    /// <param name="expected">The short value passed to constructor.</param>
    [Theory]
    [InlineData((short)-32768)]
    [InlineData((short)0)]
    [InlineData((short)32767)]
    public void ValueProperty_ExposesConstructedValue(short expected)
    {
        // Arrange & Act
        var node = new SnbtShort(expected);

        // Assert
        Assert.Equal(expected, node.Value);
    }
}