using Core.SNBT;
using Core.SNBT.Interfaces;
using Core.SNBT.Nodes;

namespace Core.Tests.SNBT;

/// <summary>
/// Unit tests for <see cref="SnbtCompoundBuilder"/>.
/// </summary>
public class SnbtCompoundBuilderTests
{
    /// <summary>
    /// Verifies that an unconfigured builder creates an empty compound.
    /// </summary>
    [Fact]
    public void Build_EmptyBuilder_ReturnsEmptySnbtCompound()
    {
        // Arrange
        var builder = new SnbtCompoundBuilder();

        // Act
        var result = builder.Build();

        // Assert
        Assert.Empty(result.Tags);
        Assert.Equal("{}", result.ToSnbtString());
    }

    /// <summary>
    /// Verifies that all primitive overloads add corresponding SNBT node types.
    /// </summary>
    [Fact]
    public void Put_PrimitiveOverloads_AddsCorrectNodeTypes()
    {
        // Arrange & Act
        var compound = new SnbtCompoundBuilder()
            .Put("b", (sbyte)1)
            .Put("s", (short)2)
            .Put("i", 3)
            .Put("l", 4L)
            .Put("f", 5.5f)
            .Put("d", 6.5)
            .Put("str", "value")
            .Put("flag", true)
            .Build();

        // Assert
        Assert.Equal(8, compound.Tags.Count);
        Assert.Equal(new SnbtByte(1), compound["b"]);
        Assert.Equal(new SnbtShort(2), compound["s"]);
        Assert.Equal(new SnbtInt(3), compound["i"]);
        Assert.Equal(new SnbtLong(4L), compound["l"]);
        Assert.Equal(new SnbtFloat(5.5f), compound["f"]);
        Assert.Equal(new SnbtDouble(6.5), compound["d"]);
        Assert.Equal(new SnbtString("value"), compound["str"]);
        Assert.Equal(new SnbtBool(true), compound["flag"]);
    }

    /// <summary>
    /// Verifies that putting a duplicate key overwrites previous node value.
    /// </summary>
    [Fact]
    public void Put_DuplicateKey_OverwritesPreviousValue()
    {
        // Arrange & Act
        var compound = new SnbtCompoundBuilder()
            .Put("key", 10)
            .Put("key", 20)
            .Build();

        // Assert
        Assert.Single(compound.Tags);
        Assert.Equal(new SnbtInt(20), compound["key"]);
    }

    /// <summary>
    /// Verifies that nested compounds and lists can be configured via delegate actions.
    /// </summary>
    [Fact]
    public void PutCompound_And_PutList_ConfigureNestedStructures()
    {
        // Arrange & Act
        var compound = new SnbtCompoundBuilder()
            .PutCompound("display", b => b.Put("name", "Diamond Sword"))
            .PutList("lore", l => l.Add("Line 1").Add("Line 2"))
            .Build();

        // Assert
        var display = Assert.IsType<SnbtCompound>(compound["display"]);
        Assert.Equal(new SnbtString("Diamond Sword"), display["name"]);

        var lore = Assert.IsType<SnbtList>(compound["lore"]);
        Assert.Equal(2, lore.Items.Count);
        Assert.Equal(new SnbtString("Line 1"), lore.Items[0]);
        Assert.Equal(new SnbtString("Line 2"), lore.Items[1]);
    }

    /// <summary>
    /// Verifies that array overloads create valid SNBT array nodes.
    /// </summary>
    [Fact]
    public void PutArray_Overloads_CreateValidArrays()
    {
        // Arrange & Act
        var compound = new SnbtCompoundBuilder()
            .PutByteArray("bytesSigned", 1, 2)
            .PutByteArray("bytesUnsigned", 0, 255) // 255 becomes -1 in signed byte
            .PutIntArray("ints", 100, 200)
            .PutLongArray("longs", 1000L, 2000L)
            .Build();

        // Assert
        var bytesSigned = Assert.IsType<SnbtByteArray>(compound["bytesSigned"]);
        Assert.Equal(new SnbtByte(1), bytesSigned.Items[0]);
        Assert.Equal(new SnbtByte(2), bytesSigned.Items[1]);

        var bytesUnsigned = Assert.IsType<SnbtByteArray>(compound["bytesUnsigned"]);
        Assert.Equal(new SnbtByte(0), bytesUnsigned.Items[0]);
        Assert.Equal(new SnbtByte(-1), bytesUnsigned.Items[1]);

        var ints = Assert.IsType<SnbtIntArray>(compound["ints"]);
        Assert.Equal(new SnbtInt(100), ints.Items[0]);
        Assert.Equal(new SnbtInt(200), ints.Items[1]);

        var longs = Assert.IsType<SnbtLongArray>(compound["longs"]);
        Assert.Equal(new SnbtLong(1000L), longs.Items[0]);
        Assert.Equal(new SnbtLong(2000L), longs.Items[1]);
    }

    /// <summary>
    /// Verifies that PutOptional with default fallback only puts values when they differ from default.
    /// </summary>
    [Fact]
    public void PutOptional_WithDefaultValue_OnlyPutsWhenDifferent()
    {
        // Arrange & Act
        var compound = new SnbtCompoundBuilder()
            .PutOptional("skipInt", 0, defaultValue: 0)
            .PutOptional("includeInt", 5, defaultValue: 0)
            .PutOptional("skipFloat", 1.0f, defaultValue: 1.0f)
            .PutOptional("includeFloat", 2.5f, defaultValue: 1.0f)
            .PutOptional("skipBool", false, defaultValue: false)
            .PutOptional("includeBool", true, defaultValue: false)
            .PutOptional("skipStr", "default", defaultValue: "default")
            .PutOptional("includeStr", "custom", defaultValue: "default")
            .Build();

        // Assert
        Assert.Null(compound["skipInt"]);
        Assert.Equal(new SnbtInt(5), compound["includeInt"]);

        Assert.Null(compound["skipFloat"]);
        Assert.Equal(new SnbtFloat(2.5f), compound["includeFloat"]);

        Assert.Null(compound["skipBool"]);
        Assert.Equal(new SnbtBool(true), compound["includeBool"]);

        Assert.Null(compound["skipStr"]);
        Assert.Equal(new SnbtString("custom"), compound["includeStr"]);
    }

    /// <summary>
    /// Verifies that PutOptional with nullable types only puts values when HasValue or not null.
    /// </summary>
    [Fact]
    public void PutOptional_NullableOverloads_OnlyPutsWhenPresent()
    {
        // Arrange
        int? nullInt = null;
        int? presentInt = 42;
        string? nullStr = null;
        const string presentStr = "hello";
        ISnbtNode? nullNode = null;
        ISnbtNode presentNode = new SnbtBool(true);

        // Act
        var compound = new SnbtCompoundBuilder()
            .PutOptional("nullInt", nullInt)
            .PutOptional("presentInt", presentInt)
            .PutOptional("nullStr", nullStr)
            .PutOptional("presentStr", presentStr)
            .PutOptional("nullNode", nullNode)
            .PutOptional("presentNode", presentNode)
            .Build();

        // Assert
        Assert.Null(compound["nullInt"]);
        Assert.Equal(new SnbtInt(42), compound["presentInt"]);

        Assert.Null(compound["nullStr"]);
        Assert.Equal(new SnbtString("hello"), compound["presentStr"]);

        Assert.Null(compound["nullNode"]);
        Assert.Equal(new SnbtBool(true), compound["presentNode"]);
    }

    /// <summary>
    /// Verifies that mutating the builder after calling Build does not mutate previously returned compounds.
    /// </summary>
    [Fact]
    public void Build_SnapshotIsolation_MutatingBuilderDoesNotAffectBuiltInstance()
    {
        // Arrange
        var builder = new SnbtCompoundBuilder().Put("a", 1);
        var firstCompound = builder.Build();

        // Act: mutate builder
        builder.Put("b", 2);
        var secondCompound = builder.Build();

        // Assert: firstCompound remains isolated
        Assert.Single(firstCompound.Tags);
        Assert.Equal(2, secondCompound.Tags.Count);
    }

    /// <summary>
    /// Verifies that passing null arguments throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [Fact]
    public void GuardClauses_NullArguments_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new SnbtCompoundBuilder();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.Put(null!, new SnbtInt(1)));
        Assert.Throws<ArgumentNullException>(() => builder.Put("key", (ISnbtNode)null!));
        Assert.Throws<ArgumentNullException>(() => builder.Put("key", (string)null!));
        Assert.Throws<ArgumentNullException>(() => builder.PutCompound("key", null!));
        Assert.Throws<ArgumentNullException>(() => builder.PutList("key", null!));
        Assert.Throws<ArgumentNullException>(() => builder.PutByteArray("key", (sbyte[])null!));
        Assert.Throws<ArgumentNullException>(() => builder.PutByteArray("key", (byte[])null!));
        Assert.Throws<ArgumentNullException>(() => builder.PutIntArray("key", null!));
        Assert.Throws<ArgumentNullException>(() => builder.PutLongArray("key", null!));
    }
}