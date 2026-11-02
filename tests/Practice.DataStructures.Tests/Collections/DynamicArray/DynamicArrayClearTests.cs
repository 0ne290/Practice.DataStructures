using Practice.DataStructures.Core.Collections;
using Xunit;

namespace Practice.DataStructures.Tests.Collections.DynamicArray;

public sealed class DynamicArrayClearTests
{
    [Theory]
    [InlineData(1, 0)]
    [InlineData(1, 1)]
    [InlineData(2, 7)]
    [InlineData(8, 3)]
    [InlineData(8, 8)]
    public void Clear_EmptyOrPopulatedArray_PreservesCapacityAndSupportsReuse(int capacity, int count)
    {
        // Arrange
        var array = new DynamicArray<int>(capacity);
        for (int item = 0; item < count; item++)
        {
            array.Add(item);
        }

        int originalCapacity = array.Capacity;

        // Act
        array.Clear();

        // Assert
        Assert.Equal(0, array.Count);
        Assert.Equal(originalCapacity, array.Capacity);
        Assert.Empty(array);
        Assert.Throws<ArgumentOutOfRangeException>("index", () => array[0]);

        // Act / Assert: повторная очистка и заполнение до нового роста.
        array.Clear();
        Assert.Equal(0, array.Count);
        Assert.Equal(originalCapacity, array.Capacity);
        for (int item = 0; item <= originalCapacity; item++)
        {
            array.Add(item + 100);
        }

        Assert.Equal(originalCapacity + 1, array.Count);
        Assert.True(array.Capacity > originalCapacity);
        Assert.Equal(Enumerable.Range(100, originalCapacity + 1), array);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(0, 2)]
    [InlineData(1, 0)]
    [InlineData(1, 1)]
    [InlineData(1, 2)]
    [InlineData(3, 0)]
    [InlineData(3, 1)]
    [InlineData(3, 2)]
    public void Clear_AnyEnumerationPosition_InvalidatesOldEnumerators(int count, int position)
    {
        // Arrange
        var array = new DynamicArray<int>();
        for (int item = 0; item < count; item++)
        {
            array.Add(item);
        }

        using IEnumerator<int> iterator = array.GetEnumerator();
        if (position == 1)
        {
            Assert.True(iterator.MoveNext());
        }
        else if (position == 2)
        {
            while (iterator.MoveNext())
            {
            }
        }

        // Act
        array.Clear();

        // Assert
        Assert.Throws<InvalidOperationException>(() => iterator.MoveNext());
        Assert.Throws<InvalidOperationException>(() => iterator.Current);
        Assert.Throws<InvalidOperationException>(() => ((System.Collections.IEnumerator)iterator).Current);
        using IEnumerator<int> freshIterator = array.GetEnumerator();
        Assert.False(freshIterator.MoveNext());
    }
}
