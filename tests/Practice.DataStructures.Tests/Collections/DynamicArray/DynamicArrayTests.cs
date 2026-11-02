using Practice.DataStructures.Core.Collections;
using Xunit;

namespace Practice.DataStructures.Tests.Collections.DynamicArray;

public sealed class DynamicArrayTests
{
    [Fact]
    public void Constructor_Default_CreatesEmptyArray()
    {
        // Arrange

        // Act
        var array = new DynamicArray<int>();

        // Assert
        Assert.Equal(0, array.Count);
        Assert.True(array.Capacity >= 1);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(8)]
    [InlineData(31)]
    public void Constructor_PositiveCapacity_CreatesEmptyArrayWithRequestedCapacity(int capacity)
    {
        // Arrange: capacity is supplied by the theory data.

        // Act
        var array = new DynamicArray<int>(capacity);

        // Assert
        Assert.Equal(0, array.Count);
        Assert.Equal(capacity, array.Capacity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void Constructor_NonPositiveCapacity_ThrowsArgumentOutOfRangeException(int capacity)
    {
        // Arrange: capacity is supplied by the theory data.

        // Act
        Exception? exception = Record.Exception(() =>
        {
            _ = new DynamicArray<int>(capacity);
        });

        // Assert
        ArgumentOutOfRangeException argumentException = Assert.IsType<ArgumentOutOfRangeException>(exception);
        Assert.Equal(nameof(capacity), argumentException.ParamName);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(8)]
    public void Add_AvailableCapacity_AppendsItemWithoutChangingCapacity(int capacity)
    {
        // Arrange
        var array = new DynamicArray<int>(capacity);
        const int item = 42;

        // Act
        array.Add(item);

        // Assert
        Assert.Equal(1, array.Count);
        Assert.Equal(capacity, array.Capacity);
        Assert.Equal(item, array[0]);
    }

    [Fact]
    public void Add_MinimumCapacity_GrowsWhenFull()
    {
        // Arrange
        var array = new DynamicArray<int>(1);
        const int firstItem = 42;
        const int nextItem = 99;
        array.Add(firstItem);

        // Act
        array.Add(nextItem);

        // Assert
        Assert.Equal(2, array.Count);
        Assert.True(array.Capacity >= array.Count);
        Assert.Equal(firstItem, array[0]);
        Assert.Equal(nextItem, array[1]);
    }

    [Fact]
    public void Add_DefaultConstructor_AppendsItem()
    {
        // Arrange
        var array = new DynamicArray<int>();
        const int item = -42;

        // Act
        array.Add(item);

        // Assert
        Assert.Equal(1, array.Count);
        Assert.True(array.Capacity >= array.Count);
        Assert.Equal(item, array[0]);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(8)]
    public void Add_RepeatedGrowth_PreservesOrderAndValues(int initialCapacity)
    {
        // Arrange
        var array = new DynamicArray<int>(initialCapacity);
        int[] expected = [.. Enumerable.Range(0, 257).Select(index => index % 17 - 8)];

        // Act
        foreach (int item in expected)
        {
            array.Add(item);
        }

        // Assert
        Assert.Equal(expected.Length, array.Count);
        Assert.True(array.Capacity > initialCapacity);
        Assert.True(array.Capacity >= array.Count);
        for (int index = 0; index < expected.Length; index++)
        {
            Assert.Equal(expected[index], array[index]);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void Add_IntegerValue_PreservesValue(int item)
    {
        // Arrange
        var array = new DynamicArray<int>();

        // Act
        array.Add(item);

        // Assert
        Assert.Equal(1, array.Count);
        Assert.Equal(item, array[0]);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("value")]
    public void Add_NullableReference_PreservesValue(string? item)
    {
        // Arrange
        var array = new DynamicArray<string?>();

        // Act
        array.Add(item);

        // Assert
        Assert.Equal(1, array.Count);
        Assert.Equal(item, array[0]);
    }

    [Fact]
    public void Add_ReferenceValuesWithGrowth_PreservesIdentity()
    {
        // Arrange
        var array = new DynamicArray<object>(1);
        object[] expected = [new(), new(), new()];

        // Act
        foreach (object item in expected)
        {
            array.Add(item);
        }

        // Assert
        Assert.Equal(expected.Length, array.Count);
        for (int index = 0; index < expected.Length; index++)
        {
            Assert.Same(expected[index], array[index]);
        }
    }

    [Theory]
    [InlineData(0, -1)]
    [InlineData(0, 0)]
    [InlineData(0, 7)]
    [InlineData(3, -1)]
    [InlineData(3, 3)]
    [InlineData(3, 7)]
    [InlineData(3, int.MinValue)]
    [InlineData(3, int.MaxValue)]
    public void IndexerGet_InvalidIndex_ThrowsArgumentOutOfRangeException(int count, int index)
    {
        // Arrange
        const int capacity = 8;
        var array = new DynamicArray<int>(capacity);
        for (int item = 1; item <= count; item++)
        {
            array.Add(item);
        }

        // Act
        Exception? exception = Record.Exception(() =>
        {
            _ = array[index];
        });

        // Assert
        ArgumentOutOfRangeException argumentException = Assert.IsType<ArgumentOutOfRangeException>(exception);
        Assert.Equal(nameof(index), argumentException.ParamName);
        Assert.Equal(count, array.Count);
        Assert.Equal(capacity, array.Capacity);
    }

    [Theory]
    [InlineData(0, -1)]
    [InlineData(0, 0)]
    [InlineData(0, 7)]
    [InlineData(3, -1)]
    [InlineData(3, 3)]
    [InlineData(3, 7)]
    [InlineData(3, int.MinValue)]
    [InlineData(3, int.MaxValue)]
    public void IndexerSet_InvalidIndex_ThrowsWithoutChangingArray(int count, int index)
    {
        // Arrange
        const int capacity = 8;
        var array = new DynamicArray<int>(capacity);
        for (int item = 1; item <= count; item++)
        {
            array.Add(item);
        }

        // Act
        Exception? exception = Record.Exception(() =>
        {
            array[index] = 99;
        });

        // Assert
        ArgumentOutOfRangeException argumentException = Assert.IsType<ArgumentOutOfRangeException>(exception);
        Assert.Equal(nameof(index), argumentException.ParamName);
        Assert.Equal(count, array.Count);
        Assert.Equal(capacity, array.Capacity);
        for (int existingIndex = 0; existingIndex < count; existingIndex++)
        {
            Assert.Equal(existingIndex + 1, array[existingIndex]);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void IndexerSet_ValidIndex_ReplacesOnlySelectedItem(int index)
    {
        // Arrange
        var array = new DynamicArray<int>(8);
        int[] expected = [10, 20, 30];
        foreach (int item in expected)
        {
            array.Add(item);
        }

        const int replacement = 99;
        expected[index] = replacement;
        int originalCapacity = array.Capacity;

        // Act
        array[index] = replacement;

        // Assert
        Assert.Equal(expected.Length, array.Count);
        Assert.Equal(originalCapacity, array.Capacity);
        for (int existingIndex = 0; existingIndex < expected.Length; existingIndex++)
        {
            Assert.Equal(expected[existingIndex], array[existingIndex]);
        }
    }
}
