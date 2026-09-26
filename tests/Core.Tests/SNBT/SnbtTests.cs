using Core.SNBT;
using Core.SNBT.Nodes;

namespace Core.Tests.SNBT;

/// <summary>
/// Unit tests for the <see cref="Snbt"/> factory entry point.
/// </summary>
public class SnbtTests
{
    /// <summary>
    /// Verifies that <see cref="Snbt.Compound()"/> returns a fresh builder instance on each invocation.
    /// </summary>
    [Fact]
    public void Compound_Parameterless_ReturnsNewBuilderInstance()
    {
        // Act
        var first = Snbt.Compound();
        var second = Snbt.Compound();

        // Assert
        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.NotSame(first, second);
    }

    /// <summary>
    /// Verifies that <see cref="Snbt.Compound(Action{SnbtCompoundBuilder})"/> executes the action and returns a built compound.
    /// </summary>
    [Fact]
    public void Compound_WithConfigureAction_BuildsPopulatedCompound()
    {
        // Act
        var compound = Snbt.Compound(b => b
            .Put("name", "Steve")
            .Put("score", 100));

        // Assert
        Assert.NotNull(compound);
        Assert.Equal(2, compound.Tags.Count);
        Assert.Equal(new SnbtString("Steve"), compound["name"]);
        Assert.Equal(new SnbtInt(100), compound["score"]);
    }

    /// <summary>
    /// Verifies that <see cref="Snbt.Compound(Action{SnbtCompoundBuilder})"/> throws <see cref="ArgumentNullException"/> when action is null.
    /// </summary>
    [Fact]
    public void Compound_NullAction_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Snbt.Compound(null!));
    }

    /// <summary>
    /// Verifies that <see cref="Snbt.List()"/> returns a fresh builder instance on each invocation.
    /// </summary>
    [Fact]
    public void List_Parameterless_ReturnsNewBuilderInstance()
    {
        // Act
        var first = Snbt.List();
        var second = Snbt.List();

        // Assert
        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.NotSame(first, second);
    }

    /// <summary>
    /// Verifies that <see cref="Snbt.List(Action{SnbtListBuilder})"/> executes the action and returns a built list.
    /// </summary>
    [Fact]
    public void List_WithConfigureAction_BuildsPopulatedList()
    {
        // Act
        var list = Snbt.List(l => l
            .Add(10)
            .Add(20)
            .Add("thirty"));

        // Assert
        Assert.NotNull(list);
        Assert.Equal(3, list.Items.Count);
        Assert.Equal(new SnbtInt(10), list.Items[0]);
        Assert.Equal(new SnbtInt(20), list.Items[1]);
        Assert.Equal(new SnbtString("thirty"), list.Items[2]);
    }

    /// <summary>
    /// Verifies that <see cref="Snbt.List(Action{SnbtListBuilder})"/> throws <see cref="ArgumentNullException"/> when action is null.
    /// </summary>
    [Fact]
    public void List_NullAction_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Snbt.List(null!));
    }
}