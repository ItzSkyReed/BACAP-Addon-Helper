using Core.DataComponents;
using Core.Items;
using Pidgin;

namespace Core.Tests.Items;

/// <summary>
/// Unit tests for <see cref="ItemStackParser"/> validating Minecraft 1.20.5+ item and component syntax.
/// </summary>
public class ItemStackParserTests
{
    #region Plain Item Identifier Tests

    /// <summary>
    /// Verifies parsing plain item IDs without components (both namespaced and shorthand).
    /// </summary>
    /// <param name="input">The raw item input string.</param>
    /// <param name="expectedId">The expected item identifier.</param>
    [Theory]
    [InlineData("minecraft:diamond_sword", "minecraft:diamond_sword")]
    [InlineData("stick", "stick")]
    [InlineData("custom_mod:magic_wand", "custom_mod:magic_wand")]
    public void Parse_PlainItemIdWithoutComponents_ReturnsItemStackWithDefaultComponents(string input, string expectedId)
    {
        // Act
        var stack = ItemStackParser.Parse(input);

        // Assert
        Assert.NotNull(stack);
        Assert.Equal(expectedId, stack.Id);
        Assert.Equal(1, stack.Count);
        Assert.True(stack.Components.IsEmpty);
    }

    /// <summary>
    /// Verifies that empty brackets "[]" produce an item stack with an empty component map.
    /// </summary>
    /// <param name="input">The item input with empty brackets.</param>
    [Theory]
    [InlineData("minecraft:stick[]")]
    [InlineData("minecraft:stick[   ]")]
    [InlineData("minecraft:stick []")]
    public void Parse_EmptyComponentsBlock_ReturnsItemWithEmptyComponents(string input)
    {
        // Act
        var stack = ItemStackParser.Parse(input);

        // Assert
        Assert.Equal("minecraft:stick", stack.Id);
        Assert.True(stack.Components.IsEmpty);
    }

    #endregion

    #region Component Assignments & Removals Tests

    /// <summary>
    /// Verifies that single and multiple component assignment operations are parsed and applied.
    /// </summary>
    [Fact]
    public void Parse_WithComponentAssignments_PopulatesComponentMap()
    {
        // Arrange
        const string input = "minecraft:diamond_sword[damage=15, unbreakable={}]";

        // Act
        var stack = ItemStackParser.Parse(input);

        // Assert
        Assert.Equal("minecraft:diamond_sword", stack.Id);
        Assert.False(stack.Components.IsEmpty);
    }

    /// <summary>
    /// Verifies that component removal syntax (!id) removes the component from active additions
    /// and records it in the explicitly removed set.
    /// </summary>
    [Fact]
    public void Parse_WithComponentRemoval_AppliesRemovalToComponentMap()
    {
        // Arrange: Add and subsequently remove a component
        const string input = "minecraft:stick[damage=10, !damage]";

        // Act
        var stack = ItemStackParser.Parse(input);

        // Assert
        Assert.Equal("minecraft:stick", stack.Id);
        Assert.False(stack.Components.Has("damage"));
        Assert.False(stack.Components.Has("minecraft:damage"));
        Assert.Empty(stack.Components.Added);
        Assert.Contains("minecraft:damage", stack.Components.Removed);
        Assert.False(stack.Components.IsEmpty); // IsEmpty is false because the removal patch is preserved!
    }

    /// <summary>
    /// Verifies that whitespace around brackets, equals, and commas is tolerated.
    /// </summary>
    [Fact]
    public void Parse_PermissiveWhitespace_ParsesSuccessfully()
    {
        // Arrange
        const string input = "  minecraft:stick [ damage = 10 , ! custom_name ]  ";

        // Act
        var stack = ItemStackParser.Parse(input);

        // Assert
        Assert.Equal("minecraft:stick", stack.Id);
        Assert.False(stack.Components.IsEmpty);
    }

    #endregion

    #region Standalone Components & ApplyComponents Tests

    /// <summary>
    /// Verifies that ApplyComponents applies bracketed component syntax to an existing map.
    /// </summary>
    [Fact]
    public void ApplyComponents_BracketedSyntax_AppliesToTargetMap()
    {
        // Arrange
        var map = new DataComponentMap();

        // Act
        ItemStackParser.ApplyComponents("[damage=25]", map);

        // Assert
        Assert.False(map.IsEmpty);
    }

    /// <summary>
    /// Verifies that ApplyComponents accepts bare comma-separated components without outer brackets.
    /// </summary>
    [Fact]
    public void ApplyComponents_BareSyntaxWithoutBrackets_AppliesToTargetMap()
    {
        // Arrange
        var map = new DataComponentMap();

        // Act
        ItemStackParser.ApplyComponents("damage=5, unbreakable={}", map);

        // Assert
        Assert.False(map.IsEmpty);
    }

    /// <summary>
    /// Verifies that passing an empty or whitespace string to ApplyComponents is a safe no-op.
    /// </summary>
    /// <param name="input">Whitespace or empty input.</param>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ApplyComponents_EmptyOrWhitespace_DoesNotModifyMap(string input)
    {
        // Arrange
        var map = new DataComponentMap();

        // Act
        ItemStackParser.ApplyComponents(input, map);

        // Assert
        Assert.True(map.IsEmpty);
    }

    #endregion

    #region Error Handling & Boundary Tests

    /// <summary>
    /// Verifies that passing null throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [Fact]
    public void GuardClauses_NullArguments_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ItemStackParser.Parse(null!));
        Assert.Throws<ArgumentNullException>(() => ItemStackParser.ApplyComponents(null!, new DataComponentMap()));
        Assert.Throws<ArgumentNullException>(() => ItemStackParser.ApplyComponents("damage=1", null!));
    }

    /// <summary>
    /// Verifies that malformed item syntax throws <see cref="ParseException{Char}"/>.
    /// </summary>
    /// <param name="invalidInput">Malformed item string.</param>
    [Theory]
    [InlineData("minecraft:stick[")]             // Unclosed bracket
    [InlineData("minecraft:stick[damage=]")]     // Missing value after equals
    [InlineData("minecraft:stick[=10]")]        // Missing component ID
    [InlineData("minecraft:stick[!")]           // Incomplete removal operator
    [InlineData("minecraft:stick[damage=10,]")] // Trailing comma
    [InlineData("minecraft:stick[damage=10] trailing")] // Trailing unparsed tokens
    public void Parse_MalformedSyntax_ThrowsParseException(string invalidInput)
    {
        // Act & Assert
        Assert.ThrowsAny<ParseException>(() => ItemStackParser.Parse(invalidInput));
    }

    #endregion
}