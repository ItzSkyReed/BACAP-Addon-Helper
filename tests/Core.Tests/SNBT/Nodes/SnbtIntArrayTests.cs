using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;

namespace Core.Tests.SNBT.Nodes;

/// <summary>
/// Unit tests for <see cref="SnbtIntArray"/>.
/// </summary>
public class SnbtIntArrayTests
{
    /// <summary>
    /// Verifies that an empty int array renders strictly as "[I;]".
    /// </summary>
    /// <param name="pretty">Whether pretty printing is enabled.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ToSnbtString_EmptyCollection_RendersCanonicalSyntax(bool pretty)
    {
        // Arrange
        var array = new SnbtIntArray([]);

        // Act
        var actual = array.ToSnbtString(pretty);

        // Assert
        Assert.Equal("[I;]", actual);
    }

    /// <summary>
    /// Verifies that compact mode renders without spaces after prefix and separators.
    /// </summary>
    [Fact]
    public void ToSnbtString_CompactMode_FormatsWithoutWhitespace()
    {
        // Arrange
        List<ISnbtNode> items = [new SnbtInt(1), new SnbtInt(2), new SnbtInt(3)];
        var array = new SnbtIntArray(items);

        // Act
        var actual = array.ToSnbtString(pretty: false);

        // Assert
        Assert.Equal("[I;1,2,3]", actual);
    }

    /// <summary>
    /// Verifies that pretty mode adds spaces after the prefix and between elements.
    /// </summary>
    [Fact]
    public void ToSnbtString_PrettyMode_FormatsWithSpaces()
    {
        // Arrange
        List<ISnbtNode> items = [new SnbtInt(100), new SnbtInt(-200)];
        var array = new SnbtIntArray(items);

        // Act
        var actual = array.ToSnbtString(pretty: true);

        // Assert
        Assert.Equal("[I; 100, -200]", actual);
    }

    /// <summary>
    /// Verifies that byte and short tags are valid elements inside an int array per SNBT specification.
    /// </summary>
    [Fact]
    public void Constructor_PermittedNarrowingTypes_SucceedsAndFormatsCorrectly()
    {
        // Arrange: Minecraft allows byte and short inside [I; ...]
        List<ISnbtNode> items = [new SnbtByte(1), new SnbtShort(2), new SnbtInt(3)];
        var array = new SnbtIntArray(items);

        // Act
        var actual = array.ToSnbtString(pretty: false);

        // Assert
        Assert.Equal("[I;1b,2s,3]", actual);
    }

    /// <summary>
    /// Verifies that disallowed node types (Long, Float, Double, Bool) are rejected.
    /// </summary>
    /// <param name="invalidNode">A non-permitted node.</param>
    [Theory]
    [MemberData(nameof(GetInvalidNodes))]
    public void Constructor_InvalidNodeTypes_ThrowsArgumentException(ISnbtNode invalidNode)
    {
        // Arrange
        List<ISnbtNode> items = [invalidNode];

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => new SnbtIntArray(items));
        Assert.Contains("Int arrays can only contain int, short, or byte tags", ex.Message);
    }

    public static TheoryData<ISnbtNode> GetInvalidNodes() => new()
    {
        new SnbtLong(100L),
        new SnbtFloat(1.5f),
        new SnbtDouble(2.5),
        new SnbtBool(true)
    };

    /// <summary>
    /// Verifies structural equality between two distinct int array instances with identical elements.
    /// </summary>
    [Fact]
    public void Equality_SameValues_AreEqualAndShareHashCode()
    {
        // Arrange
        var first = new SnbtIntArray([new SnbtInt(10), new SnbtInt(20)]);
        var second = new SnbtIntArray([new SnbtInt(10), new SnbtInt(20)]);
        var different = new SnbtIntArray([new SnbtInt(10), new SnbtInt(30)]);

        // Assert
        Assert.Equal(first, second);
        Assert.True(first == second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());

        Assert.NotEqual(first, different);
        Assert.True(first != different);
    }

    /// <summary>
    /// Verifies that arrays with different prefix types are not considered equal even if elements match.
    /// </summary>
    [Fact]
    public void Equality_DifferentArrayTypes_AreNotEqual()
    {
        // Arrange
        var intArray = new SnbtIntArray([new SnbtByte(1)]);
        var byteArray = new SnbtByteArray([new SnbtByte(1)]);

        // Assert
        Assert.False(intArray.Equals(byteArray));
        Assert.NotEqual<SnbtArrayNode>(intArray, byteArray);
    }
}