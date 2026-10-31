using Practice.DataStructures.Core.Collections;
using Xunit;

namespace Practice.DataStructures.Tests.Collections;

public sealed class DynamicArrayRemovalTests
{
    [Theory]
    [InlineData(1, 1, 0)]
    [InlineData(4, 4, 0)]
    [InlineData(4, 4, 1)]
    [InlineData(4, 4, 3)]
    [InlineData(8, 4, 0)]
    [InlineData(8, 4, 1)]
    [InlineData(8, 4, 2)]
    [InlineData(8, 4, 3)]
    public void RemoveAt_ValidIndex_PreservesOrderAndCapacity(int capacity, int count, int index)
    {
        // Arrange
        var array = new DynamicArray<int>(capacity);
        var expected = new List<int>();
        for (int item = 1; item <= count; item++)
        {
            array.Add(item);
            expected.Add(item);
        }

        expected.RemoveAt(index);

        // Act
        array.RemoveAt(index);

        // Assert
        Assert.Equal(expected.Count, array.Count);
        Assert.Equal(capacity, array.Capacity);
        Assert.Equal(expected, array);
        for (int remainingIndex = 0; remainingIndex < expected.Count; remainingIndex++)
        {
            Assert.Equal(expected[remainingIndex], array[remainingIndex]);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void RemoveAt_ReferencesNullAndDuplicates_PreservesRemainingIdentities(int index)
    {
        // Arrange
        object repeated = new();
        var array = new DynamicArray<object?> { repeated, null, repeated, new() };
        var expected = new List<object?>(array);
        expected.RemoveAt(index);
        int originalCapacity = array.Capacity;

        // Act
        array.RemoveAt(index);

        // Assert
        Assert.Equal(expected.Count, array.Count);
        Assert.Equal(originalCapacity, array.Capacity);
        Assert.Equal(expected, array);
        for (int remainingIndex = 0; remainingIndex < expected.Count; remainingIndex++)
        {
            Assert.Same(expected[remainingIndex], array[remainingIndex]);
        }
    }

    [Theory]
    [InlineData(0, -1)]
    [InlineData(0, 0)]
    [InlineData(0, 7)]
    [InlineData(0, int.MinValue)]
    [InlineData(0, int.MaxValue)]
    [InlineData(3, -1)]
    [InlineData(3, 3)]
    [InlineData(3, 7)]
    [InlineData(3, int.MinValue)]
    [InlineData(3, int.MaxValue)]
    public void RemoveAt_InvalidIndex_ThrowsWithoutChangingArray(int count, int index)
    {
        // Arrange
        const int capacity = 8;
        int[] expected = [.. Enumerable.Range(1, count)];
        var array = new DynamicArray<int>(capacity);
        foreach (int item in expected)
        {
            array.Add(item);
        }

        // Act
        Exception? exception = Record.Exception(() => array.RemoveAt(index));

        // Assert
        ArgumentOutOfRangeException argumentException = Assert.IsType<ArgumentOutOfRangeException>(exception);
        Assert.Equal(nameof(index), argumentException.ParamName);
        Assert.Equal(count, array.Count);
        Assert.Equal(capacity, array.Capacity);
        Assert.Equal(expected, array);
        for (int existingIndex = 0; existingIndex < expected.Length; existingIndex++)
        {
            Assert.Equal(expected[existingIndex], array[existingIndex]);
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(8)]
    public void RemoveAt_RepeatedUntilEmpty_PreservesCapacityAndSupportsReuse(int initialCapacity)
    {
        // Arrange
        var array = new DynamicArray<int>(initialCapacity) { 10, 20, 30, 40, 50 };
        var expected = new List<int>(array);
        int originalCapacity = array.Capacity;

        // Act / Assert: удаление из середины, затем единственного элемента.
        while (expected.Count > 0)
        {
            int index = expected.Count / 2;
            array.RemoveAt(index);
            expected.RemoveAt(index);

            Assert.Equal(expected.Count, array.Count);
            Assert.Equal(originalCapacity, array.Capacity);
            Assert.Equal(expected, array);
        }

        Assert.Empty(array);

        // Act: повторное использование опустевшего массива.
        array.Add(99);

        // Assert
        Assert.Equal(1, array.Count);
        Assert.Equal(originalCapacity, array.Capacity);
        Assert.Equal(99, array[0]);
        Assert.Equal(99, Assert.Single(array));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void RemoveAt_FollowedByAddAndGrowth_PreservesRemainingAndNewItems(int index)
    {
        // Arrange
        var array = new DynamicArray<int>(3) { 10, 20, 30 };
        var expected = new List<int>(array);
        expected.RemoveAt(index);
        expected.Add(40);
        expected.Add(50);

        // Act
        array.RemoveAt(index);
        array.Add(40);
        array.Add(50);

        // Assert
        Assert.Equal(expected.Count, array.Count);
        Assert.True(array.Capacity >= array.Count);
        Assert.Equal(expected, array);
    }
}
