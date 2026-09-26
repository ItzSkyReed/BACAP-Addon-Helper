using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;

namespace Core.Tests.SNBT.Nodes;

/// <summary>
/// Unit tests for <see cref="SnbtList"/>.
/// </summary>
public class SnbtListTests
{
    /// <summary>
    /// Verifies that an empty list serializes to "[]" regardless of pretty mode.
    /// </summary>
    /// <param name="pretty">Whether pretty printing is enabled.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ToSnbtString_EmptyList_ReturnsEmptyBrackets(bool pretty)
    {
        // Arrange
        var list = new SnbtList();

        // Act
        var actual = list.ToSnbtString(pretty);

        // Assert
        Assert.Equal("[]", actual);
    }

    /// <summary>
    /// Verifies that compact mode serializes items delimited by commas without extra spaces or line feeds.
    /// </summary>
    [Fact]
    public void ToSnbtString_CompactMode_DelimitsItemsWithoutSpaces()
    {
        // Arrange
        List<ISnbtNode> items = [new SnbtInt(1), new SnbtInt(2), new SnbtByte(3)];
        var list = new SnbtList(items);

        // Act
        var actual = list.ToSnbtString(pretty: false);

        // Assert
        Assert.Equal("[1,2,3b]", actual);
    }

    /// <summary>
    /// Verifies that pretty printing produces multi-line indented output with proper comma placement.
    /// </summary>
    [Fact]
    public void ToSnbtString_PrettyMode_ProducesExpectedIndentation()
    {
        // Arrange
        List<ISnbtNode> items = [new SnbtInt(10), new SnbtBool(true)];
        var list = new SnbtList(items);

        var expected = string.Join(Environment.NewLine, [
            "[",
            "  10,",
            "  true",
            "]"
        ]);

        // Act
        var actual = list.ToSnbtString(pretty: true);

        // Assert
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Verifies that nested lists correctly propagate indentation levels during pretty serialization.
    /// </summary>
    [Fact]
    public void ToSnbtString_NestedListPrettyMode_IndentsCorrectly()
    {
        // Arrange
        var innerList = new SnbtList([new SnbtInt(1), new SnbtInt(2)]);
        var outerList = new SnbtList([innerList]);

        var expected = string.Join(Environment.NewLine, [
            "[",
            "  [",
            "    1,",
            "    2",
            "  ]",
            "]"
        ]);

        // Act
        var actual = outerList.ToSnbtString(pretty: true);

        // Assert
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Verifies that heterogeneous elements (numbers, bools, arrays) are supported in SNBT lists.
    /// </summary>
    [Fact]
    public void ToSnbtString_HeterogeneousElements_SerializesSuccessfully()
    {
        // Arrange
        List<ISnbtNode> items =
        [
            new SnbtInt(1),
            new SnbtBool(false),
            new SnbtByteArray([new SnbtByte(127)])
        ];
        var list = new SnbtList(items);

        // Act
        var actual = list.ToSnbtString(pretty: false);

        // Assert
        Assert.Equal("[1,false,[B;127b]]", actual);
    }

    /// <summary>
    /// Ensures that passing a null list instance throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [Fact]
    public void Constructor_NullList_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new SnbtList(null!));
    }

    /// <summary>
    /// Ensures that a list containing a null element throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [Fact]
    public void Constructor_ListWithNullElement_ThrowsArgumentNullException()
    {
        // Arrange
        List<ISnbtNode> items = [new SnbtInt(1), null!];

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new SnbtList(items));
    }

    /// <summary>
    /// Verifies structural equality between two distinct list instances containing equivalent nodes.
    /// </summary>
    [Fact]
    public void Equality_TwoDistinctInstancesWithSameElements_AreEqualAndShareHashCode()
    {
        // Arrange
        var first = new SnbtList([new SnbtInt(1), new SnbtByte(2)]);
        var second = new SnbtList([new SnbtInt(1), new SnbtByte(2)]);
        var different = new SnbtList([new SnbtInt(1), new SnbtByte(3)]);

        // Assert
        Assert.Equal(first, second);
        Assert.True(first == second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());

        Assert.NotEqual(first, different);
        Assert.True(first != different);
    }

    /// <summary>
    /// Verifies that <see cref="SnbtList"/> implements <see cref="ISnbtNode"/>.
    /// </summary>
    [Fact]
    public void Implements_ISnbtNode_Interface()
    {
        // Arrange & Act
        var list = new SnbtList();

        // Assert
        Assert.IsAssignableFrom<ISnbtNode>(list);
    }
}