using Core.Items;
using Core.SNBT;
using Core.SNBT.Nodes;

namespace Core.Tests.Items;

/// <summary>
/// Unit tests for <see cref="ItemStack"/>.
/// </summary>
public class ItemStackTests
{
    #region Initialization & IsEmpty Tests

    /// <summary>
    /// Verifies default property values upon construction.
    /// </summary>
    [Fact]
    public void Constructor_DefaultParameters_InitializesWithCountOneAndEmptyComponents()
    {
        // Act
        var stack = new ItemStack("minecraft:diamond");

        // Assert
        Assert.Equal("minecraft:diamond", stack.Id);
        Assert.Equal(1, stack.Count);
        Assert.NotNull(stack.Components);
        Assert.True(stack.Components.IsEmpty);
        Assert.False(stack.IsEmpty);
    }

    /// <summary>
    /// Verifies the IsEmpty logic across different invalid or air item states.
    /// </summary>
    /// <param name="id">Item identifier.</param>
    /// <param name="count">Item count.</param>
    /// <param name="expected">Expected IsEmpty result.</param>
    [Theory]
    [InlineData("minecraft:air", 1, true)]
    [InlineData("", 1, true)]
    [InlineData(null, 1, true)]
    [InlineData("minecraft:stone", 0, true)]
    [InlineData("minecraft:stone", -5, true)]
    [InlineData("minecraft:stone", 1, false)]
    [InlineData("minecraft:diamond", 64, false)]
    public void IsEmpty_VariousStates_EvaluatesCorrectly(string? id, int count, bool expected)
    {
        // Arrange & Act
        var stack = new ItemStack(id!, count);

        // Assert
        Assert.Equal(expected, stack.IsEmpty);
    }

    #endregion

    #region Serialization Tests

    /// <summary>
    /// Verifies that an item stack with count 1 omits the "count" tag in SNBT output.
    /// </summary>
    [Fact]
    public void ToSnbt_CountOne_OmitsCountTag()
    {
        // Arrange
        // ReSharper disable once RedundantArgumentDefaultValue
        var stack = new ItemStack("minecraft:iron_ingot", 1);

        // Act
        var node = Assert.IsType<SnbtCompound>(stack.ToSnbt());

        // Assert
        Assert.Equal("minecraft:iron_ingot", node.GetString("id"));
        Assert.Null(node.GetNode("count"));
        Assert.Equal("{id:\"minecraft:iron_ingot\"}", stack.ToSnbt().ToSnbtString());
    }

    /// <summary>
    /// Verifies that an item stack with count greater than 1 includes the "count" tag.
    /// </summary>
    [Fact]
    public void ToSnbt_CountGreaterThanOne_SerializesCountTag()
    {
        // Arrange
        var stack = new ItemStack("minecraft:stick", 16);

        // Act
        var node = Assert.IsType<SnbtCompound>(stack.ToSnbt());

        // Assert
        Assert.Equal("minecraft:stick", node.GetString("id"));
        Assert.Equal(16, node.GetInt("count"));
        Assert.Equal("{id:\"minecraft:stick\",count:16}", stack.ToSnbt().ToSnbtString());
    }

    /// <summary>
    /// Verifies that an item with empty Id serializes as "minecraft:air".
    /// </summary>
    [Fact]
    public void ToSnbt_EmptyId_SerializesAsAir()
    {
        // Arrange
        // ReSharper disable once RedundantArgumentDefaultValue
        var stack = new ItemStack(string.Empty, 1);

        // Act
        var node = Assert.IsType<SnbtCompound>(stack.ToSnbt());

        // Assert
        Assert.Equal(ItemStack.AirId, node.GetString("id"));
    }

    /// <summary>
    /// Verifies that ToCommandString returns only Id when no components are present.
    /// </summary>
    [Fact]
    public void ToCommandString_NoComponents_ReturnsIdOnly()
    {
        // Arrange
        var stack = new ItemStack("minecraft:stick", 5);

        // Act
        var commandString = stack.ToCommandString();

        // Assert
        Assert.Equal("minecraft:stick", commandString);
    }

    #endregion

    #region Deserialization (Parse) Tests

    /// <summary>
    /// Verifies that passing null to Parse throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [Fact]
    public void Parse_NullCompound_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ItemStack.Parse(null!));
    }

    /// <summary>
    /// Verifies parsing an item compound containing modern lowercase "count".
    /// </summary>
    [Fact]
    public void Parse_ModernCompound_ExtractsIdAndCount()
    {
        // Arrange
        var compound = Snbt.Compound()
            .Put("id", "minecraft:gold_ingot")
            .Put("count", 32)
            .Build();

        // Act
        var stack = ItemStack.Parse(compound);

        // Assert
        Assert.Equal("minecraft:gold_ingot", stack.Id);
        Assert.Equal(32, stack.Count);
        Assert.False(stack.IsEmpty);
    }

    /// <summary>
    /// Verifies backward compatibility with uppercase "Count" tags.
    /// </summary>
    [Fact]
    public void Parse_LegacyCapitalCountCompound_ExtractsCountCorrectly()
    {
        // Arrange
        var compound = Snbt.Compound()
            .Put("id", "minecraft:diamond")
            .Put("Count", (sbyte)64)
            .Build();

        // Act
        var stack = ItemStack.Parse(compound);

        // Assert
        Assert.Equal("minecraft:diamond", stack.Id);
        Assert.Equal(64, stack.Count);
    }

    /// <summary>
    /// Verifies that an empty compound defaults to air with count 1.
    /// </summary>
    [Fact]
    public void Parse_EmptyCompound_DefaultsToAir()
    {
        // Arrange
        var compound = new SnbtCompound();

        // Act
        var stack = ItemStack.Parse(compound);

        // Assert
        Assert.Equal(ItemStack.AirId, stack.Id);
        Assert.Equal(1, stack.Count);
        Assert.True(stack.IsEmpty);
    }

    #endregion

    #region Conversion & ToString Tests

    /// <summary>
    /// Verifies implicit conversion from string identifier to <see cref="ItemStack"/>.
    /// </summary>
    [Fact]
    public void ImplicitOperator_FromString_ConstructsStackWithCountOne()
    {
        // Act
        ItemStack stack = "minecraft:torch";

        // Assert
        Assert.Equal("minecraft:torch", stack.Id);
        Assert.Equal(1, stack.Count);
        Assert.False(stack.IsEmpty);
    }

    /// <summary>
    /// Verifies ToString representations for empty and non-empty stacks.
    /// </summary>
    [Fact]
    public void ToString_ReturnsFormattedDescription()
    {
        // Arrange
        var airStack = new ItemStack(ItemStack.AirId);
        var zeroStack = new ItemStack("minecraft:stone", 0);
        var validStack = new ItemStack("minecraft:apple", 5);

        // Assert
        Assert.Equal("Air", airStack.ToString());
        Assert.Equal("Air", zeroStack.ToString());
        Assert.Equal("5x minecraft:apple", validStack.ToString());
    }

    #endregion
}