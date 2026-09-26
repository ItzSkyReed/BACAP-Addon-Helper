using Core.SNBT;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;

namespace Core.Tests.SNBT;

/// <summary>
/// Unit tests for <see cref="SnbtListBuilder"/>.
/// </summary>
public class SnbtListBuilderTests
{
    /// <summary>
    /// Verifies that calling <see cref="SnbtListBuilder.Build"/> without adding elements produces an empty list.
    /// </summary>
    [Fact]
    public void Build_EmptyBuilder_ReturnsEmptySnbtList()
    {
        // Arrange
        var builder = new SnbtListBuilder();

        // Act
        var result = builder.Build();

        // Assert
        Assert.Empty(result.Items);
        Assert.Equal("[]", result.ToSnbtString());
    }

    /// <summary>
    /// Verifies that all primitive overloads add corresponding SNBT node types.
    /// </summary>
    [Fact]
    public void Add_PrimitiveOverloads_AddsCorrectNodeTypes()
    {
        // Arrange & Act
        var list = new SnbtListBuilder()
            .Add(true)
            .Add((sbyte)1)
            .Add((short)2)
            .Add(3)
            .Add(4L)
            .Add(5.5f)
            .Add(6.5)
            .Add("text")
            .Build();

        // Assert
        Assert.Equal(8, list.Items.Count);
        Assert.Equal(new SnbtBool(true), list.Items[0]);
        Assert.Equal(new SnbtByte(1), list.Items[1]);
        Assert.Equal(new SnbtShort(2), list.Items[2]);
        Assert.Equal(new SnbtInt(3), list.Items[3]);
        Assert.Equal(new SnbtLong(4L), list.Items[4]);
        Assert.Equal(new SnbtFloat(5.5f), list.Items[5]);
        Assert.Equal(new SnbtDouble(6.5), list.Items[6]);
        Assert.Equal(new SnbtString("text"), list.Items[7]);
    }

    /// <summary>
    /// Verifies that AddRange overloads correctly append items sequentially.
    /// </summary>
    [Fact]
    public void AddRange_Collections_AppendsExpectedNodes()
    {
        // Arrange & Act
        var list = new SnbtListBuilder()
            .AddRange(["a", "b"])
            .AddRange([10, 20])
            .AddRange([1.1, 2.2])
            .AddRange([new SnbtBool(false)])
            .Build();

        // Assert
        Assert.Equal(7, list.Items.Count);
        Assert.Equal(new SnbtString("a"), list.Items[0]);
        Assert.Equal(new SnbtString("b"), list.Items[1]);
        Assert.Equal(new SnbtInt(10), list.Items[2]);
        Assert.Equal(new SnbtInt(20), list.Items[3]);
        Assert.Equal(new SnbtDouble(1.1), list.Items[4]);
        Assert.Equal(new SnbtDouble(2.2), list.Items[5]);
        Assert.Equal(new SnbtBool(false), list.Items[6]);
    }

    /// <summary>
    /// Verifies that nested compounds can be added using the fluent configuration delegate.
    /// </summary>
    [Fact]
    public void AddCompound_ConfiguredAction_AppendsPopulatedCompound()
    {
        // Arrange & Act
        var list = new SnbtListBuilder()
            .AddCompound(compound => compound
                .Put("id", "minecraft:stone")
                .Put("count", (sbyte)64))
            .Build();

        // Assert
        Assert.Single(list.Items);
        var compound = Assert.IsType<SnbtCompound>(list.Items[0]);
        Assert.Equal(new SnbtString("minecraft:stone"), compound["id"]);
        Assert.Equal(new SnbtByte(64), compound["count"]);
    }

    /// <summary>
    /// Verifies that nested lists can be added using the fluent configuration delegate.
    /// </summary>
    [Fact]
    public void AddList_ConfiguredAction_AppendsPopulatedList()
    {
        // Arrange & Act
        var list = new SnbtListBuilder()
            .AddList(nested => nested.Add(1).Add(2))
            .Build();

        // Assert
        Assert.Single(list.Items);
        var nestedList = Assert.IsType<SnbtList>(list.Items[0]);
        Assert.Equal(2, nestedList.Items.Count);
        Assert.Equal(new SnbtInt(1), nestedList.Items[0]);
        Assert.Equal(new SnbtInt(2), nestedList.Items[1]);
    }

    /// <summary>
    /// Verifies that mutating the builder after calling Build does not affect the previously created list.
    /// </summary>
    [Fact]
    public void Build_SnapshotIsolation_DoesNotMutatePreviouslyBuiltList()
    {
        // Arrange
        var builder = new SnbtListBuilder().Add(1).Add(2);
        var firstList = builder.Build();

        // Act: modify builder after build
        builder.Add(3);
        var secondList = builder.Build();

        // Assert: first list remains immutable snapshot
        Assert.Equal(2, firstList.Items.Count);
        Assert.Equal(3, secondList.Items.Count);
    }

    /// <summary>
    /// Verifies that null arguments immediately throw <see cref="ArgumentNullException"/>.
    /// </summary>
    [Fact]
    public void GuardClauses_NullArguments_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new SnbtListBuilder();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.Add((ISnbtNode)null!));
        Assert.Throws<ArgumentNullException>(() => builder.Add((string)null!));
        Assert.Throws<ArgumentNullException>(() => builder.AddRange((IEnumerable<string>)null!));
        Assert.Throws<ArgumentNullException>(() => builder.AddRange((IEnumerable<int>)null!));
        Assert.Throws<ArgumentNullException>(() => builder.AddRange((IEnumerable<double>)null!));
        Assert.Throws<ArgumentNullException>(() => builder.AddRange((IEnumerable<ISnbtNode>)null!));
        Assert.Throws<ArgumentNullException>(() => builder.AddCompound(null!));
        Assert.Throws<ArgumentNullException>(() => builder.AddList(null!));
    }
}