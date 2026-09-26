using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;

namespace Core.Tests.SNBT.Nodes;

/// <summary>
/// Unit tests for <see cref="SnbtLongArray"/>.
/// </summary>
public class SnbtLongArrayTests
{
    /// <summary>
    /// Verifies that an empty long array renders as "[L;]".
    /// </summary>
    /// <param name="pretty">Whether pretty printing is enabled.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ToSnbtString_EmptyCollection_RendersCanonicalSyntax(bool pretty)
    {
        // Arrange
        var array = new SnbtLongArray([]);

        // Act
        var actual = array.ToSnbtString(pretty);

        // Assert
        Assert.Equal("[L;]", actual);
    }

    /// <summary>
    /// Verifies compact output formatting.
    /// </summary>
    [Fact]
    public void ToSnbtString_CompactMode_FormatsWithoutWhitespace()
    {
        // Arrange
        List<ISnbtNode> items = [new SnbtLong(10L), new SnbtLong(20L)];
        var array = new SnbtLongArray(items);

        // Act
        var actual = array.ToSnbtString(pretty: false);

        // Assert
        Assert.Equal("[L;10L,20L]", actual);
    }

    /// <summary>
    /// Verifies pretty output formatting.
    /// </summary>
    [Fact]
    public void ToSnbtString_PrettyMode_FormatsWithSpaces()
    {
        // Arrange
        List<ISnbtNode> items = [new SnbtLong(10L), new SnbtLong(20L)];
        var array = new SnbtLongArray(items);

        // Act
        var actual = array.ToSnbtString(pretty: true);

        // Assert
        Assert.Equal("[L; 10L, 20L]", actual);
    }

    /// <summary>
    /// Verifies that byte, short, int, and long tags are all acceptable in a long array per SNBT specification.
    /// </summary>
    [Fact]
    public void Constructor_PermittedNarrowingTypes_SucceedsAndFormatsCorrectly()
    {
        // Arrange: [L;1b,2s,3i,4l]
        List<ISnbtNode> items = [new SnbtByte(1), new SnbtShort(2), new SnbtInt(3), new SnbtLong(4L)];
        var array = new SnbtLongArray(items);

        // Act
        var actual = array.ToSnbtString(pretty: false);

        // Assert
        Assert.Equal("[L;1b,2s,3,4L]", actual);
    }

    /// <summary>
    /// Verifies that non-integer nodes (Float, Double, Bool) are rejected.
    /// </summary>
    /// <param name="invalidNode">A non-integer node.</param>
    [Theory]
    [MemberData(nameof(GetInvalidNodes))]
    public void Constructor_InvalidNodeTypes_ThrowsArgumentException(ISnbtNode invalidNode)
    {
        // Arrange
        List<ISnbtNode> items = [invalidNode];

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => new SnbtLongArray(items));
        Assert.Contains("Long arrays can only contain long, int, short, or byte tags", ex.Message);
    }

    public static TheoryData<ISnbtNode> GetInvalidNodes() => new()
    {
        new SnbtFloat(1.5f),
        new SnbtDouble(2.5),
        new SnbtBool(false)
    };

    /// <summary>
    /// Verifies structural equality between matching long arrays.
    /// </summary>
    [Fact]
    public void Equality_SameValues_AreEqualAndShareHashCode()
    {
        // Arrange
        var first = new SnbtLongArray([new SnbtLong(1L), new SnbtLong(2L)]);
        var second = new SnbtLongArray([new SnbtLong(1L), new SnbtLong(2L)]);
        var different = new SnbtLongArray([new SnbtLong(1L), new SnbtLong(3L)]);

        // Assert
        Assert.Equal(first, second);
        Assert.True(first == second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());

        Assert.NotEqual(first, different);
        Assert.True(first != different);
    }
}