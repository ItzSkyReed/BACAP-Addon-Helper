using System.Globalization;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;

namespace Core.Tests.SNBT.Nodes;

/// <summary>
/// Unit tests for <see cref="SnbtByte"/>.
/// </summary>
public class SnbtByteTests
{
    /// <summary>
    /// Verifies that <see cref="SnbtByte.ToSnbtString"/> appends the 'b' suffix to the numeric value.
    /// </summary>
    /// <param name="input">The byte value to test.</param>
    /// <param name="expected">The expected SNBT representation.</param>
    [Theory]
    [InlineData((sbyte)0, "0b")]
    [InlineData((sbyte)1, "1b")]
    [InlineData((sbyte)-1, "-1b")]
    [InlineData((sbyte)127, "127b")]   // sbyte.MaxValue
    [InlineData((sbyte)-128, "-128b")] // sbyte.MinValue
    [InlineData((sbyte)42, "42b")]
    [InlineData((sbyte)-42, "-42b")]
    public void ToSnbtString_DefaultParameters_ReturnsExpectedValueWithSuffix(sbyte input, string expected)
    {
        // Arrange
        var node = new SnbtByte(input);

        // Act
        var actual = node.ToSnbtString();

        // Assert
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Ensures that numeric formatting remains invariant regardless of the current thread culture.
    /// </summary>
    [Fact]
    public void ToSnbtString_UnderDifferentCultures_ProducesConsistentAsciiRepresentation()
    {
        // Arrange
        var previousCulture = CultureInfo.CurrentCulture;
        var node = new SnbtByte(-128);

        try
        {
            // Culture with non-ASCII minus sign conventions or custom separators
            CultureInfo.CurrentCulture = new CultureInfo("ar-EG");

            // Act
            var actual = node.ToSnbtString();

            // Assert
            Assert.Equal("-128b", actual);
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }
    }

    /// <summary>
    /// Verifies that scalar nodes ignore pretty printing flags and indentation parameters.
    /// </summary>
    /// <param name="value">The byte value.</param>
    /// <param name="pretty">Whether pretty printing is enabled.</param>
    /// <param name="indent">The indentation prefix.</param>
    /// <param name="expected">The expected serialized string.</param>
    [Theory]
    [InlineData((sbyte)10, true, "    ", "10b")]
    [InlineData((sbyte)-10, true, "\t\t", "-10b")]
    [InlineData((sbyte)0, false, "  ", "0b")]
    public void ToSnbtString_WithPrettyAndIndent_IgnoresIndentation(
        sbyte value,
        bool pretty,
        string indent,
        string expected)
    {
        // Arrange
        var node = new SnbtByte(value);

        // Act
        var actual = node.ToSnbtString(pretty, indent);

        // Assert
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Verifies that <see cref="SnbtByte"/> implements <see cref="ISnbtNode"/>.
    /// </summary>
    [Fact]
    public void Implements_ISnbtNode_Interface()
    {
        // Arrange & Act
        var node = new SnbtByte(1);

        // Assert
        Assert.IsType<ISnbtNode>(node, exactMatch: false);
    }

    /// <summary>
    /// Verifies value equality and hash code parity between record instances.
    /// </summary>
    [Fact]
    public void Equality_SameValues_AreEqualAndHaveMatchingHashCodes()
    {
        // Arrange
        var first = new SnbtByte(64);
        var second = new SnbtByte(64);
        var different = new SnbtByte(-64);

        // Assert
        Assert.Equal(first, second);
        Assert.True(first == second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());

        Assert.NotEqual(first, different);
        Assert.True(first != different);
    }

    /// <summary>
    /// Verifies that the positional parameter exposes the correct stored value.
    /// </summary>
    /// <param name="expected">The value passed to constructor.</param>
    [Theory]
    [InlineData((sbyte)-128)]
    [InlineData((sbyte)0)]
    [InlineData((sbyte)127)]
    public void ValueProperty_ExposesConstructedValue(sbyte expected)
    {
        // Arrange & Act
        var node = new SnbtByte(expected);

        // Assert
        Assert.Equal(expected, node.Value);
    }
}