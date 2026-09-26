using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;

namespace Core.Tests.SNBT.Nodes;

/// <summary>
/// Unit tests for <see cref="SnbtByteArray"/>.
/// </summary>
public class SnbtByteArrayTests
{
    /// <summary>
    /// Verifies that an empty byte array renders as "[B;]" in both compact and pretty modes.
    /// </summary>
    /// <param name="pretty">Whether pretty printing is enabled.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ToSnbtString_EmptyCollection_RendersCanonicalEmptySyntax(bool pretty)
    {
        // Arrange
        var array = new SnbtByteArray([]);

        // Act
        var actual = array.ToSnbtString(pretty);

        // Assert
        Assert.Equal("[B;]", actual);
    }

    /// <summary>
    /// Verifies that compact mode formats without any whitespace.
    /// </summary>
    [Fact]
    public void ToSnbtString_CompactMode_FormatsWithoutSpaces()
    {
        // Arrange
        List<ISnbtNode> items = [new SnbtByte(1), new SnbtByte(2), new SnbtByte(3)];
        var array = new SnbtByteArray(items);

        // Act
        var actual = array.ToSnbtString(pretty: false);

        // Assert
        Assert.Equal("[B;1b,2b,3b]", actual);
    }

    /// <summary>
    /// Verifies that pretty mode adds a space after prefix and commas.
    /// </summary>
    [Fact]
    public void ToSnbtString_PrettyMode_FormatsWithSpaces()
    {
        // Arrange
        List<ISnbtNode> items = [new SnbtByte(10), new SnbtByte(-20)];
        var array = new SnbtByteArray(items);

        // Act
        var actual = array.ToSnbtString(pretty: true);

        // Assert
        Assert.Equal("[B; 10b, -20b]", actual);
    }

    /// <summary>
    /// Ensures null list argument throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [Fact]
    public void Constructor_NullList_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new SnbtByteArray(null!));
    }

    /// <summary>
    /// Ensures null element inside list throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [Fact]
    public void Constructor_ListWithNullElement_ThrowsArgumentNullException()
    {
        // Arrange
        List<ISnbtNode> items = [new SnbtByte(1), null!];

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new SnbtByteArray(items));
    }

    /// <summary>
    /// Ensures non-byte nodes throw <see cref="ArgumentException"/>.
    /// </summary>
    /// <param name="invalidNode">A non-byte SNBT node instance.</param>
    [Theory]
    [MemberData(nameof(GetInvalidNodes))]
    public void Constructor_NonByteNodes_ThrowsArgumentException(ISnbtNode invalidNode)
    {
        // Arrange
        List<ISnbtNode> items = [invalidNode];

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => new SnbtByteArray(items));
        Assert.Contains("Byte arrays can only contain byte tags", ex.Message);
    }

    public static TheoryData<ISnbtNode> GetInvalidNodes() => new()
    {
        new SnbtBool(true),
        new SnbtInt(42),
        new SnbtShort(10),
        new SnbtFloat(1.5f),
        new SnbtDouble(2.5)
    };

    /// <summary>
    /// Verifies value equality when comparing two instances with matching elements.
    /// </summary>
    [Fact]
    public void Equality_TwoDistinctInstancesWithSameElements_AreEqualAndShareHashCode()
    {
        // Arrange
        var first = new SnbtByteArray([new SnbtByte(1), new SnbtByte(2)]);
        var second = new SnbtByteArray([new SnbtByte(1), new SnbtByte(2)]);
        var different = new SnbtByteArray([new SnbtByte(1), new SnbtByte(3)]);

        // Assert
        Assert.Equal(first, second);
        Assert.True(first == second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());

        Assert.NotEqual(first, different);
        Assert.True(first != different);
    }

    /// <summary>
    /// Verifies that <see cref="SnbtByteArray"/> implements <see cref="ISnbtNode"/>.
    /// </summary>
    [Fact]
    public void Implements_ISnbtNode_Interface()
    {
        // Arrange & Act
        var array = new SnbtByteArray([]);

        // Assert
        Assert.IsAssignableFrom<ISnbtNode>(array);
    }
}