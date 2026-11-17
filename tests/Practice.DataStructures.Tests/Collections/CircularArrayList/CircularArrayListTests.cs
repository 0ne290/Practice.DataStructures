using Practice.DataStructures.Core.Collections;
using Xunit;

namespace Practice.DataStructures.Tests.Collections.CircularArrayList;

public sealed class CircularArrayListTests
{
    [Fact]
    public void Constructor_Default_UsesAcceptedCapacity()
    {
        // Arrange / Act
        var list = new CircularArrayList<int>();

        // Assert
        Assert.Equal(0, list.Count);
        Assert.Equal(8, list.Capacity);
        Assert.Empty(list);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(8)]
    public void InsertRemove_AllSmallLayoutsAndPositions_PreserveValuesAndEnds(int capacity)
    {
        // Arrange: every head offset, occupancy and valid position.
        for (int offset = 0; offset < capacity; offset++)
        {
            for (int count = 0; count <= capacity; count++)
            {
                for (int position = 0; position <= count; position++)
                {
                    CircularArrayList<int> list = CreateLayout(capacity, offset, count);
                    var expected = new List<int>(list);
                    int originalCapacity = list.Capacity;

                    // Act
                    list.Insert(position, -1);
                    expected.Insert(position, -1);
                    list.Add(-2);
                    expected.Add(-2);

                    // Assert
                    Assert.Equal(expected.Count, list.Count);
                    Assert.Equal(expected, list);
                    Assert.Equal(expected[0], list.First);
                    Assert.Equal(expected[^1], list.Last);
                    Assert.True(list.Capacity >= originalCapacity);
                }

                for (int position = 0; position < count; position++)
                {
                    CircularArrayList<int> list = CreateLayout(capacity, offset, count);
                    var expected = new List<int>(list);

                    // Act
                    list.RemoveAt(position);
                    expected.RemoveAt(position);

                    // Assert
                    Assert.Equal(capacity, list.Capacity);
                    Assert.Equal(expected.Count, list.Count);
                    Assert.Equal(expected, list);
                    if (expected.Count > 0)
                    {
                        Assert.Equal(expected[0], list.First);
                        Assert.Equal(expected[^1], list.Last);
                    }

                    // Act / Assert: append must use the newly freed space.
                    list.Add(-2);
                    expected.Add(-2);
                    Assert.Equal(expected, list);
                    Assert.Equal(expected[0], list.First);
                    Assert.Equal(expected[^1], list.Last);
                }
            }
        }
    }

    [Fact]
    public void Ends_ReadsPreserveEnumeratorAndEmptyReadsThrow()
    {
        // Arrange
        CircularArrayList<int> list = CreateLayout(4, 3, 3);
        int[] expected = [.. list];
        using IEnumerator<int> iterator = list.GetEnumerator();

        // Act / Assert
        Assert.Equal(expected[0], list.First);
        Assert.Equal(expected[^1], list.Last);
        Assert.True(iterator.MoveNext());
        Assert.Equal(expected[0], iterator.Current);
        list.Clear();
        using IEnumerator<int> empty = list.GetEnumerator();
        Assert.Throws<InvalidOperationException>(() => list.First);
        Assert.Throws<InvalidOperationException>(() => list.Last);
        Assert.False(empty.MoveNext());
        list.Add(99);
        Assert.Equal(99, list.First);
        Assert.Equal(99, list.Last);
        list.RemoveAt(0);
        Assert.Throws<InvalidOperationException>(() => list.First);
        Assert.Throws<InvalidOperationException>(() => list.Last);
    }

    private static CircularArrayList<int> CreateLayout(int capacity, int offset, int count)
    {
        var list = new CircularArrayList<int>(capacity);
        for (int value = 0; value < capacity; value++)
        {
            list.Add(value);
        }

        for (int step = 0; step < offset; step++)
        {
            list.RemoveAt(0);
            list.Add(capacity + step);
        }

        while (list.Count > count)
        {
            list.RemoveAt(list.Count - 1);
        }

        return list;
    }
}
