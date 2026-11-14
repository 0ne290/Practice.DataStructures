using Practice.DataStructures.Core.Collections;
using Xunit;

namespace Practice.DataStructures.Tests.Collections.DoublyLinkedList;

public sealed class DoublyLinkedListTests
{
    [Fact]
    public void Constructor_Empty_HasNoEndpointsOrMatches()
    {
        // Arrange / Act
        var list = new DoublyLinkedList<int>();

        // Assert
        Assert.Equal(0, list.Count);
        Assert.Null(list.First);
        Assert.Null(list.Last);
        Assert.Null(list.FindFirst((_, _) => throw new InvalidOperationException()));
        Assert.Null(list.FindLast((_, _) => throw new InvalidOperationException()));
        Assert.Empty(list);
        Assert.Empty(list.Reverse());
        Assert.False(list.TryRemoveFirst(out int first));
        Assert.False(list.TryRemoveLast(out int last));
        Assert.Equal(0, first);
        Assert.Equal(0, last);
    }

    [Fact]
    public void AddAndRemove_BothEnds_PreserveOrderAndAllowReuse()
    {
        // Arrange
        var list = new DoublyLinkedList<int>();

        // Act
        list.AddFirst(2);
        list.AddFirst(1);
        list.AddLast(3);

        // Assert
        Assert.Equal([1, 2, 3], list);
        Assert.Equal([3, 2, 1], list.Reverse());
        Assert.True(list.TryRemoveLast(out int last));
        Assert.Equal(3, last);
        Assert.True(list.TryRemoveFirst(out int first));
        Assert.Equal(1, first);
        Assert.Equal(2, Assert.IsType<DoublyLinkedList<int>.Cursor>(list.First).Value);
        Assert.True(list.TryRemoveLast(out int only));
        Assert.Equal(2, only);
        Assert.Null(list.First);
        Assert.Null(list.Last);
        Assert.Equal(0, list.Count);
        list.AddLast(4);
        Assert.True(list.TryRemoveFirst(out int reused));
        Assert.Equal(4, reused);
        Assert.Empty(list);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Remove_StoredNullAndDefault_DistinguishesAbsence(bool fromEnd)
    {
        // Arrange
        var list = new DoublyLinkedList<string?>();
        list.AddLast(null);
        list.AddLast("value");

        // Act
        bool removed = fromEnd ? list.TryRemoveLast(out string? value) : list.TryRemoveFirst(out value);

        // Assert
        Assert.True(removed);
        Assert.Equal(fromEnd ? "value" : null, value);
        Assert.Equal(1, list.Count);
        Assert.True(list.TryRemoveFirst(out string? remaining));
        Assert.Equal(fromEnd ? null : "value", remaining);
        Assert.False(list.TryRemoveLast(out string? absent));
        Assert.Null(absent);
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(4, 0)]
    [InlineData(4, 1)]
    [InlineData(4, 2)]
    [InlineData(4, 3)]
    public void CursorAt_ValidPosition_SelectsElementAndBothNeighbors(int count, int index)
    {
        // Arrange
        var list = new DoublyLinkedList<int>();
        foreach (int value in Enumerable.Range(0, count))
        {
            list.AddLast(value);
        }

        // Act
        DoublyLinkedList<int>.Cursor cursor = list.GetCursorAt(index);

        // Assert
        Assert.Equal(index, cursor.Value);
        Assert.Equal(index > 0, cursor.MovePrevious());
        Assert.Equal(index > 0 ? index - 1 : index, cursor.Value);
        cursor = list.GetCursorAt(index);
        Assert.Equal(index < count - 1, cursor.MoveNext());
        Assert.Equal(index < count - 1 ? index + 1 : index, cursor.Value);
    }

    [Theory]
    [InlineData(0, -1)]
    [InlineData(0, 0)]
    [InlineData(3, -1)]
    [InlineData(3, 3)]
    [InlineData(3, int.MaxValue)]
    public void CursorAt_InvalidPosition_ThrowsWithoutChangingList(int count, int index)
    {
        // Arrange
        var list = new DoublyLinkedList<int>();
        foreach (int value in Enumerable.Range(0, count))
        {
            list.AddLast(value);
        }

        // Act / Assert
        Assert.Throws<ArgumentOutOfRangeException>(nameof(index), () => list.GetCursorAt(index));
        Assert.Equal(Enumerable.Range(0, count), list);
    }

    [Theory]
    [InlineData(12345)]
    [InlineData(98765)]
    public void Operations_FixedRandomSequence_MatchReferenceList(int seed)
    {
        // Arrange
        var random = new Random(seed);
        var list = new DoublyLinkedList<int>();
        var expected = new List<int>();

        // Act / Assert
        for (int step = 0; step < 750; step++)
        {
            int value = random.Next(-10, 11);
            int index = expected.Count == 0 ? 0 : random.Next(expected.Count);
            switch (random.Next(9))
            {
                case 0:
                    list.AddFirst(value);
                    expected.Insert(0, value);
                    break;
                case 1:
                    list.AddLast(value);
                    expected.Add(value);
                    break;
                case 2:
                    Assert.Equal(expected.Count > 0, list.TryRemoveFirst(out int first));
                    if (expected.Count > 0)
                    {
                        Assert.Equal(expected[0], first);
                        expected.RemoveAt(0);
                    }
                    break;
                case 3:
                    Assert.Equal(expected.Count > 0, list.TryRemoveLast(out int last));
                    if (expected.Count > 0)
                    {
                        Assert.Equal(expected[^1], last);
                        expected.RemoveAt(expected.Count - 1);
                    }
                    break;
                case 4 when expected.Count > 0:
                    list.GetCursorAt(index).InsertBefore(value);
                    expected.Insert(index, value);
                    break;
                case 5 when expected.Count > 0:
                    list.GetCursorAt(index).InsertAfter(value);
                    expected.Insert(index + 1, value);
                    break;
                case 6 when expected.Count > 0:
                    list.GetCursorAt(index).Remove();
                    expected.RemoveAt(index);
                    break;
                case 7 when expected.Count > 0:
                    list.GetCursorAt(index).Value = value;
                    expected[index] = value;
                    break;
                case 8:
                    list.Clear();
                    expected.Clear();
                    break;
            }

            Assert.Equal(expected.Count, list.Count);
            Assert.Equal(expected, list);
            Assert.Equal(expected.AsEnumerable().Reverse(), list.Reverse());
            Assert.Equal(expected.Count == 0 ? null : expected[0], list.First?.Value);
            Assert.Equal(expected.Count == 0 ? null : expected[^1], list.Last?.Value);
        }
    }
}
