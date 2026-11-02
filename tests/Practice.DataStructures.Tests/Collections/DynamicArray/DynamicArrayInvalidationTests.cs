using System.Collections;
using Practice.DataStructures.Core.Collections;
using Xunit;

namespace Practice.DataStructures.Tests.Collections.DynamicArray;

public sealed class DynamicArrayInvalidationTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 1)]
    [InlineData(1, 4)]
    public void MoveNext_ItemAddedBeforeFirstMove_ThrowsInvalidOperationException(int count, int capacity)
    {
        // Arrange
        var array = new DynamicArray<int>(capacity);
        for (int item = 0; item < count; item++)
        {
            array.Add(item);
        }

        using IEnumerator<int> iterator = array.GetEnumerator();

        // Act
        array.Add(99);
        // Assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            iterator.MoveNext();
        });
        Assert.Equal(count + 1, array.Count);
        Assert.Equal(99, array[count]);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(4)]
    public void MoveNext_ItemAddedDuringEnumeration_ThrowsInvalidOperationException(int capacity)
    {
        // Arrange
        var array = new DynamicArray<int>(capacity) { 10 };
        using IEnumerator<int> iterator = array.GetEnumerator();
        Assert.True(iterator.MoveNext());

        // Act
        array.Add(20);
        // Assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            iterator.MoveNext();
        });
        Assert.Equal(2, array.Count);
        Assert.Equal(20, array[1]);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(0, 99)]
    [InlineData(1, 20)]
    [InlineData(1, 99)]
    public void MoveNext_ItemAssignedDuringEnumeration_ThrowsInvalidOperationException(int index, int value)
    {
        // Arrange
        var array = new DynamicArray<int> { 10, 20 };
        using IEnumerator<int> iterator = array.GetEnumerator();
        Assert.True(iterator.MoveNext());

        // Act
        array[index] = value;
        // Assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            iterator.MoveNext();
        });
        Assert.Equal(2, array.Count);
        Assert.Equal(value, array[index]);
    }

    [Fact]
    public void MoveNext_ItemAssignedAfterLastYield_ThrowsInvalidOperationException()
    {
        // Arrange
        var array = new DynamicArray<int> { 10 };
        using IEnumerator<int> iterator = array.GetEnumerator();
        Assert.True(iterator.MoveNext());

        // Act
        array[0] = 20;
        // Assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            iterator.MoveNext();
        });
    }

    [Fact]
    public void MoveNext_ItemAddedAfterCompletion_ThrowsInvalidOperationException()
    {
        // Arrange
        var array = new DynamicArray<int> { 10 };
        using IEnumerator<int> iterator = array.GetEnumerator();
        Assert.True(iterator.MoveNext());
        Assert.False(iterator.MoveNext());

        // Act
        array.Add(20);
        // Assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            iterator.MoveNext();
        });
    }

    [Fact]
    public void MoveNext_ItemAssignedAfterCompletion_ThrowsInvalidOperationException()
    {
        // Arrange
        var array = new DynamicArray<int> { 10 };
        using IEnumerator<int> iterator = array.GetEnumerator();
        Assert.True(iterator.MoveNext());
        Assert.False(iterator.MoveNext());

        // Act
        array[0] = 20;
        // Assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            iterator.MoveNext();
        });
    }

    [Fact]
    public void MoveNext_NonGenericEnumeratorInvalidated_ThrowsInvalidOperationException()
    {
        // Arrange
        var array = new DynamicArray<int> { 10 };
        IEnumerator iterator = ((IEnumerable)array).GetEnumerator();
        using var resource = iterator as IDisposable;

        // Act
        array.Add(20);
        // Assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            iterator.MoveNext();
        });
    }

    [Fact]
    public void Current_EnumeratorInvalidated_ThrowsInvalidOperationException()
    {
        // Arrange
        var array = new DynamicArray<int> { 10, 20 };
        using IEnumerator<int> iterator = array.GetEnumerator();
        Assert.True(iterator.MoveNext());

        // Act
        array[1] = 99;
        // Assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            _ = iterator.Current;
        });
    }

    [Fact]
    public void MoveNext_ReadAndRejectedWrite_DoNotInvalidateEnumerator()
    {
        // Arrange
        var array = new DynamicArray<int> { 10, 20 };
        using IEnumerator<int> iterator = array.GetEnumerator();
        Assert.True(iterator.MoveNext());

        // Act
        int readValue = array[0];
        Exception? writeException = Record.Exception(() =>
        {
            array[array.Count] = 99;
        });
        bool advanced = iterator.MoveNext();
        int nextValue = iterator.Current;

        // Assert
        Assert.Equal(10, readValue);
        Assert.IsType<ArgumentOutOfRangeException>(writeException);
        Assert.True(advanced);
        Assert.Equal(20, nextValue);
    }

    [Theory]
    [InlineData(1, 0, 0)]
    [InlineData(1, 0, 1)]
    [InlineData(1, 0, 2)]
    [InlineData(3, 0, 0)]
    [InlineData(3, 1, 1)]
    [InlineData(3, 2, 3)]
    [InlineData(3, 1, 4)]
    public void RemoveAt_EnumeratorInAnyPosition_InvalidatesMoveNextAndCurrent(int count, int index, int advanceCount)
    {
        // Arrange
        var array = new DynamicArray<int>();
        for (int item = 1; item <= count; item++)
        {
            array.Add(item);
        }

        using IEnumerator<int> iterator = array.GetEnumerator();
        for (int step = 0; step < advanceCount; step++)
        {
            Assert.Equal(step < count, iterator.MoveNext());
        }

        // Act
        array.RemoveAt(index);

        // Assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            iterator.MoveNext();
        });
        Assert.Throws<InvalidOperationException>(() =>
        {
            _ = iterator.Current;
        });
        Assert.Throws<InvalidOperationException>(() =>
        {
            _ = ((IEnumerator)iterator).Current;
        });
    }

    [Fact]
    public void RemoveAt_NonGenericEnumerator_InvalidatesMoveNextAndCurrent()
    {
        // Arrange
        var array = new DynamicArray<int> { 10, 20 };
        IEnumerator iterator = ((IEnumerable)array).GetEnumerator();
        using var resource = iterator as IDisposable;
        Assert.True(iterator.MoveNext());

        // Act
        array.RemoveAt(1);

        // Assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            iterator.MoveNext();
        });
        Assert.Throws<InvalidOperationException>(() =>
        {
            _ = iterator.Current;
        });
    }

    [Theory]
    [InlineData(0, 0, -1)]
    [InlineData(0, 1, 0)]
    [InlineData(3, 0, -1)]
    [InlineData(3, 1, 3)]
    [InlineData(3, 3, int.MaxValue)]
    [InlineData(3, 4, int.MinValue)]
    public void RemoveAt_InvalidIndex_PreservesEnumeratorState(int count, int advanceCount, int invalidIndex)
    {
        // Arrange
        int[] expected = [.. Enumerable.Range(1, count)];
        var array = new DynamicArray<int>();
        foreach (int item in expected)
        {
            array.Add(item);
        }

        using IEnumerator<int> iterator = array.GetEnumerator();
        IEnumerator nonGenericIterator = ((IEnumerable)array).GetEnumerator();
        using var resource = nonGenericIterator as IDisposable;
        for (int step = 0; step < advanceCount; step++)
        {
            Assert.Equal(step < count, iterator.MoveNext());
            Assert.Equal(step < count, nonGenericIterator.MoveNext());
        }

        // Act
        Exception? exception = Record.Exception(() => array.RemoveAt(invalidIndex));

        // Assert
        ArgumentOutOfRangeException argumentException = Assert.IsType<ArgumentOutOfRangeException>(exception);
        Assert.Equal("index", argumentException.ParamName);
        if (advanceCount > 0 && advanceCount <= count)
        {
            Assert.Equal(expected[advanceCount - 1], iterator.Current);
            Assert.Equal(expected[advanceCount - 1], Assert.IsType<int>(nonGenericIterator.Current));
        }
        else
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                _ = iterator.Current;
            });
            Assert.Throws<InvalidOperationException>(() =>
            {
                _ = nonGenericIterator.Current;
            });
        }

        for (int remainingIndex = advanceCount; remainingIndex < count; remainingIndex++)
        {
            Assert.True(iterator.MoveNext());
            Assert.True(nonGenericIterator.MoveNext());
            Assert.Equal(expected[remainingIndex], iterator.Current);
            Assert.Equal(expected[remainingIndex], Assert.IsType<int>(nonGenericIterator.Current));
        }

        Assert.False(iterator.MoveNext());
        Assert.False(nonGenericIterator.MoveNext());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Current_OutsideActivePosition_ThrowsInvalidOperationException(bool completeEnumeration)
    {
        // Arrange
        var array = new DynamicArray<int> { 10 };
        using IEnumerator<int> iterator = array.GetEnumerator();
        if (completeEnumeration)
        {
            Assert.True(iterator.MoveNext());
            Assert.False(iterator.MoveNext());
        }

        // Act / Assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            _ = iterator.Current;
        });
    }
}
