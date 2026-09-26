using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;

namespace Core.Tests.SNBT.Nodes;

public class SnbtCompoundGenericOptionalTests
{
    /// <summary>
    /// Verifies that <see cref="SnbtCompound.GetOptional{T}"/> works seamlessly with reference types like string.
    /// </summary>
    [Fact]
    public void GetOptional_ReferenceTypeString_ReturnsExpectedStringOrNull()
    {
        // Arrange
        var compound = new SnbtCompound(new Dictionary<string, ISnbtNode>
        {
            ["text"] = new SnbtString("sample")
        });

        // Act
        var present = compound.GetOptional<string>("text");
        var missing = compound.GetOptional<string>("unknown");

        // Assert
        Assert.Equal("sample", present);
        Assert.Null(missing);
    }

    /// <summary>
    /// Verifies that <see cref="SnbtCompound.GetOptional{T}"/> returns default (0, false) for value types,
    /// while specific nullable getters return true null.
    /// </summary>
    [Fact]
    public void GetOptional_ValueTypes_ReturnsDefault_AndSpecificGettersReturnNull()
    {
        // Arrange
        var compound = new SnbtCompound(new Dictionary<string, ISnbtNode>
        {
            ["count"] = new SnbtInt(42),
            ["enabled"] = new SnbtBool(true)
        });

        // Generic GetOptional<T> returns T? (which is int/bool with default fallback 0/false)
        Assert.Equal(42, compound.GetOptional<int>("count"));
        Assert.Equal(0, compound.GetOptional<int>("missing"));

        Assert.True(compound.GetOptional<bool>("enabled"));
        Assert.False(compound.GetOptional<bool>("missing"));

        // Specific nullable getters return Nullable<T> (int?, bool?) with true null on missing
        Assert.Equal(42, compound.GetOptionalInt("count"));
        Assert.Null(compound.GetOptionalInt("missing"));

        Assert.Equal(true, compound.GetOptionalBool("enabled"));
        Assert.Null(compound.GetOptionalBool("missing"));
    }

    /// <summary>
    /// Verifies that <see cref="SnbtCompound.GetOptional{T}"/> can retrieve nested AST nodes directly.
    /// </summary>
    [Fact]
    public void GetOptional_ExactNodeReference_ReturnsNodeInstance()
    {
        // Arrange
        var innerCompound = new SnbtCompound();
        var compound = new SnbtCompound(new Dictionary<string, ISnbtNode>
        {
            ["child"] = innerCompound
        });

        // Act
        var node = compound.GetOptional<SnbtCompound>("child");
        var missingNode = compound.GetOptional<SnbtCompound>("unknown");

        // Assert
        Assert.Same(innerCompound, node);
        Assert.Null(missingNode);
    }
}