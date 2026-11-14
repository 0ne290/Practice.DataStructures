using Practice.DataStructures.Core.Collections;
using Xunit;

namespace Practice.DataStructures.Tests.Collections.SinglyLinkedList;

public sealed class SinglyLinkedListCursorTests
{
    [Fact]
    public void ApiCategories_SameOwner_PreserveSeparateEntryPoints()
    {
        // Arrange
        var list = new SinglyLinkedList<int>();
        SinglyLinkedList<int>.ValueApi values = list.Values;
        SinglyLinkedList<int>.CursorApi cursors = list.Cursors;

        // Act
        values.AddLast(2);
        cursors.BeforeFirst.InsertAfter(1);

        // Assert
        Assert.Same(values, list.Values);
        Assert.Same(cursors, list.Cursors);
        Assert.Equal([1, 2], list);
        Assert.Equal(2, list.Count);
        Assert.True(values.Contains((value, _) => value == 1));
        Assert.Null(typeof(SinglyLinkedList<int>).GetMethod("AddFirst"));
        Assert.Null(typeof(SinglyLinkedList<int>).GetMethod("RemoveFirst"));
        Assert.Null(typeof(SinglyLinkedList<int>).GetMethod("Clear"));
    }

    [Fact]
    public void BeforeFirst_EmptyList_SupportsHeadInsertionAndRemoval()
    {
        // Arrange
        var list = new SinglyLinkedList<string?>();
        SinglyLinkedList<string?>.Cursor before = list.Cursors.BeforeFirst;

        // Act / Assert
        Assert.Null(list.Cursors.First);
        Assert.Null(list.Cursors.Last);
        Assert.Null(list.Cursors.FindFirst((_, _) => throw new ApplicationException()));
        Assert.False(before.MoveNext());
        Assert.Throws<InvalidOperationException>(() => before.Value);
        Assert.Throws<InvalidOperationException>(() => before.Value = "x");
        Assert.False(before.TryRemoveAfter(out string? missing));
        Assert.Null(missing);
        before.InsertAfter(null);
        Assert.Equal(1, list.Count);
        Assert.Null(Assert.IsType<SinglyLinkedList<string?>.Cursor>(list.Cursors.First).Value);
        Assert.True(before.TryRemoveAfter(out string? stored));
        Assert.Null(stored);
        Assert.Null(list.Cursors.First);
        Assert.Null(list.Cursors.Last);
        before.InsertAfter("again");
        Assert.True(before.MoveNext());
        Assert.Equal("again", before.Value);
        Assert.False(before.MoveNext());
        Assert.Equal("again", before.Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void InsertAfterAndRemoveAfter_AtEachPosition_PreserveOrderAndTail(int position)
    {
        // Arrange
        SinglyLinkedList<int> list = CreateList();
        SinglyLinkedList<int>.Cursor cursor = Assert.IsType<SinglyLinkedList<int>.Cursor>(
            list.Cursors.FindFirst((_, index) => index == position));
        var expected = new List<int> { 1, 2, 3 };

        // Act
        cursor.InsertAfter(9);
        expected.Insert(position + 1, 9);

        // Assert
        Assert.Equal(position + 1, cursor.Value);
        Assert.Equal(expected, list);
        Assert.True(cursor.TryRemoveAfter(out int removed));
        Assert.Equal(9, removed);
        Assert.Equal([1, 2, 3], list);
        list.Values.AddLast(4);
        Assert.Equal([1, 2, 3, 4], list);
        Assert.Equal(4, list.Count);
    }

    [Fact]
    public void RemoveAfter_TailAndHead_PreserveBothEndpoints()
    {
        // Arrange
        var list = new SinglyLinkedList<int>();
        list.Values.AddLast(1);
        list.Values.AddLast(2);
        SinglyLinkedList<int>.Cursor first = Assert.IsType<SinglyLinkedList<int>.Cursor>(list.Cursors.First);
        SinglyLinkedList<int>.Cursor tail = Assert.IsType<SinglyLinkedList<int>.Cursor>(list.Cursors.Last);

        // Act
        Assert.True(first.TryRemoveAfter(out int removed));

        // Assert
        Assert.Equal(2, removed);
        Assert.Throws<InvalidOperationException>(() => tail.Value);
        Assert.Equal(1, Assert.IsType<SinglyLinkedList<int>.Cursor>(list.Cursors.Last).Value);
        Assert.False(first.TryRemoveAfter(out int missing));
        Assert.Equal(0, missing);
        Assert.True(list.Cursors.BeforeFirst.TryRemoveAfter(out int only));
        Assert.Equal(1, only);
        Assert.Throws<InvalidOperationException>(() => first.Value);
        Assert.Equal(0, list.Count);
        list.Values.AddLast(3);
        Assert.Equal(3, Assert.Single(list));
    }

    [Fact]
    public void Cursor_ValuesApiEditsElsewhere_PreservesPositionAndNewNeighbors()
    {
        // Arrange
        SinglyLinkedList<int> list = CreateList();
        SinglyLinkedList<int>.Cursor cursor = Assert.IsType<SinglyLinkedList<int>.Cursor>(list.Cursors.FindFirst((value, _) => value == 2));
        SinglyLinkedList<int>.Cursor alias = Assert.IsType<SinglyLinkedList<int>.Cursor>(list.Cursors.FindFirst((value, _) => value == 2));

        // Act
        list.Values.AddFirst(0);
        Assert.True(list.Values.AddBefore((value, _) => value == 2, 10));
        Assert.True(list.Values.RemoveFirst((value, _) => value == 1));
        Assert.True(list.Values.AddAfter((value, _) => value == 2, 20));
        Assert.True(list.Values.ReplaceFirst((value, _) => value == 2, 200));

        // Assert
        Assert.Equal(200, cursor.Value);
        Assert.Equal(200, alias.Value);
        Assert.True(cursor.MoveNext());
        Assert.Equal(20, cursor.Value);
        Assert.Equal(200, alias.Value);
        Assert.True(alias.TryRemoveAfter(out int removed));
        Assert.Equal(20, removed);
        Assert.Throws<InvalidOperationException>(() => cursor.Value);
        Assert.Equal([0, 10, 200, 3], list);
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(3, 0)]
    [InlineData(3, 1)]
    [InlineData(3, 2)]
    public void ValuesRemoval_SelectedElement_InvalidatesAllCursorOperations(int count, int position)
    {
        // Arrange
        var list = new SinglyLinkedList<int>();
        foreach (int value in Enumerable.Range(0, count))
        {
            list.Values.AddLast(value);
        }
        SinglyLinkedList<int>.Cursor cursor = Assert.IsType<SinglyLinkedList<int>.Cursor>(list.Cursors.FindFirst((_, index) => index == position));

        // Act
        Assert.True(list.Values.RemoveFirst((_, index) => index == position));

        // Assert
        Assert.Throws<InvalidOperationException>(() => cursor.Value);
        Assert.Throws<InvalidOperationException>(() => cursor.Value = 9);
        Assert.Throws<InvalidOperationException>(() => cursor.MoveNext());
        Assert.Throws<InvalidOperationException>(() => cursor.InsertAfter(9));
        Assert.Throws<InvalidOperationException>(() => cursor.TryRemoveAfter(out _));
        Assert.Equal(Enumerable.Range(0, count).Where(value => value != position), list);
    }

    [Fact]
    public void Clear_ExistingCursorsAndFacades_RejectOldPositionsAndAllowNewOnes()
    {
        // Arrange
        SinglyLinkedList<int> list = CreateList();
        SinglyLinkedList<int>.CursorApi api = list.Cursors;
        SinglyLinkedList<int>.Cursor before = api.BeforeFirst;
        SinglyLinkedList<int>.Cursor first = Assert.IsType<SinglyLinkedList<int>.Cursor>(api.First);

        // Act
        list.Values.Clear();
        api.BeforeFirst.InsertAfter(9);

        // Assert
        Assert.Throws<InvalidOperationException>(() => before.MoveNext());
        Assert.Throws<InvalidOperationException>(() => before.InsertAfter(8));
        Assert.Throws<InvalidOperationException>(() => first.Value);
        Assert.Equal(9, Assert.IsType<SinglyLinkedList<int>.Cursor>(api.Last).Value);
        Assert.Equal(9, Assert.Single(list));
    }

    [Theory]
    [InlineData("insert")]
    [InlineData("remove")]
    [InlineData("replace")]
    public void CursorMutation_ExistingEnumeratorAndPredicate_DetectSharedVersion(string mutation)
    {
        // Arrange
        SinglyLinkedList<int> list = CreateList();
        SinglyLinkedList<int>.Cursor cursor = Assert.IsType<SinglyLinkedList<int>.Cursor>(list.Cursors.First);
        using IEnumerator<int> enumerator = list.GetEnumerator();

        // Act / Assert
        Assert.Throws<InvalidOperationException>(() => list.Values.Contains((_, _) =>
        {
            switch (mutation)
            {
                case "insert": cursor.InsertAfter(9); break;
                case "remove": Assert.True(cursor.TryRemoveAfter(out _)); break;
                case "replace": cursor.Value = cursor.Value; break;
            }
            return true;
        }));
        Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
    }

    [Fact]
    public void CursorReadAndFailedRemoval_PreserveEnumerator()
    {
        // Arrange
        SinglyLinkedList<int> list = CreateList();
        SinglyLinkedList<int>.Cursor cursor = Assert.IsType<SinglyLinkedList<int>.Cursor>(list.Cursors.Last);
        using IEnumerator<int> enumerator = list.GetEnumerator();

        // Act
        Assert.False(cursor.MoveNext());
        Assert.False(cursor.TryRemoveAfter(out _));
        Assert.Null(list.Cursors.FindFirst((_, _) => false));
        Assert.Equal(3, cursor.Value);

        // Assert
        Assert.True(enumerator.MoveNext());
        Assert.Equal(1, enumerator.Current);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CursorSearch_ValuesMutation_DetectsChangeBeforeReturningCursor(bool matches)
    {
        // Arrange
        SinglyLinkedList<int> list = CreateList();

        // Act / Assert
        Assert.Throws<InvalidOperationException>(() => list.Cursors.FindFirst((_, _) =>
        {
            list.Values.Clear();
            return matches;
        }));
        Assert.Empty(list);
    }

    [Fact]
    public void MixedApis_FixedRandomSequence_MatchReferenceList()
    {
        // Arrange
        var list = new SinglyLinkedList<int>();
        var expected = new List<int>();
        var random = new Random(18673);

        // Act / Assert
        for (int step = 0; step < 500; step++)
        {
            int insertionIndex = random.Next(expected.Count + 1);
            int value = random.Next(-5, 6);
            SinglyLinkedList<int>.Cursor cursor = list.Cursors.BeforeFirst;
            for (int index = 0; index < insertionIndex; index++)
            {
                Assert.True(cursor.MoveNext());
            }
            switch (random.Next(5))
            {
                case 0:
                    cursor.InsertAfter(value);
                    expected.Insert(insertionIndex, value);
                    break;
                case 1:
                    Assert.Equal(insertionIndex < expected.Count, cursor.TryRemoveAfter(out int removed));
                    if (insertionIndex < expected.Count)
                    {
                        Assert.Equal(expected[insertionIndex], removed);
                        expected.RemoveAt(insertionIndex);
                    }
                    break;
                case 2:
                    list.Values.AddFirst(value);
                    expected.Insert(0, value);
                    break;
                case 3:
                    Assert.Equal(insertionIndex < expected.Count,
                        list.Values.RemoveFirst((_, position) => position == insertionIndex));
                    if (insertionIndex < expected.Count)
                    {
                        expected.RemoveAt(insertionIndex);
                    }
                    break;
                case 4:
                    list.Values.Clear();
                    expected.Clear();
                    break;
            }
            Assert.Equal(expected, list);
            Assert.Equal(expected.Count, list.Count);
            Assert.Equal(expected.Count > 0, list.Values.TryLast(out int tail));
            Assert.Equal(expected.LastOrDefault(), tail);
        }
    }

    private static SinglyLinkedList<int> CreateList()
    {
        var list = new SinglyLinkedList<int>();
        list.Values.AddLast(1);
        list.Values.AddLast(2);
        list.Values.AddLast(3);
        return list;
    }
}
