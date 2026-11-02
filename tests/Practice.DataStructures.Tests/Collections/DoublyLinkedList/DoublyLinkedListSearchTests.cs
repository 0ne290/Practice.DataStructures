using Practice.DataStructures.Core.Collections;
using Xunit;

namespace Practice.DataStructures.Tests.Collections.DoublyLinkedList;

public sealed class DoublyLinkedListSearchTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Find_Duplicates_VisitsFromSelectedEndAndStopsAtMatch(bool backwards)
    {
        // Arrange
        DoublyLinkedList<int> list = CreateList();
        var positions = new List<int>();
        bool Matches(int value, int position)
        {
            positions.Add(position);
            return value == 2;
        }

        // Act
        DoublyLinkedList<int>.Cursor? result = backwards ? list.FindLast(Matches) : list.FindFirst(Matches);

        // Assert
        DoublyLinkedList<int>.Cursor cursor = Assert.IsType<DoublyLinkedList<int>.Cursor>(result);
        Assert.Equal(2, cursor.Value);
        Assert.Equal(backwards ? new[] { 4, 3 } : new[] { 0, 1 }, positions);
        cursor.Value = 9;
        Assert.Equal(backwards ? new[] { 1, 2, 3, 9, 5 } : new[] { 1, 9, 3, 2, 5 }, list);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Find_NoMatch_VisitsEachPositionOnce(bool backwards)
    {
        // Arrange
        DoublyLinkedList<int> list = CreateList();
        var positions = new List<int>();
        bool Matches(int _, int position)
        {
            positions.Add(position);
            return false;
        }

        // Act
        DoublyLinkedList<int>.Cursor? result = backwards ? list.FindLast(Matches) : list.FindFirst(Matches);

        // Assert
        Assert.Null(result);
        Assert.Equal(backwards ? new[] { 4, 3, 2, 1, 0 } : new[] { 0, 1, 2, 3, 4 }, positions);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    [InlineData(true, false)]
    public void Find_PredicateChangesList_DetectsChangeBeforeUsingResult(bool backwards, bool matches)
    {
        // Arrange
        DoublyLinkedList<int> list = CreateList();
        bool Matches(int _, int __)
        {
            list.Clear();
            return matches;
        }

        // Act / Assert
        Assert.Throws<InvalidOperationException>(() => backwards ? list.FindLast(Matches) : list.FindFirst(Matches));
        Assert.Equal(0, list.Count);
        Assert.Empty(list);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Find_ThrowingPredicate_PropagatesOriginalException(bool backwards)
    {
        // Arrange
        DoublyLinkedList<int> list = CreateList();
        var failure = new ApplicationException("predicate");
        bool Matches(int _, int __)
        {
            list.AddLast(6);
            throw failure;
        }

        // Act
        ApplicationException actual = Assert.Throws<ApplicationException>(() => backwards ? list.FindLast(Matches) : list.FindFirst(Matches));

        // Assert
        Assert.Same(failure, actual);
        Assert.Equal(6, list.Count);
    }

    [Fact]
    public void Find_NestedReadAndPositionSelection_PreserveEnumeration()
    {
        // Arrange
        DoublyLinkedList<int> list = CreateList();
        using IEnumerator<int> enumerator = list.GetEnumerator();

        // Act
        DoublyLinkedList<int>.Cursor? result = list.FindLast((value, position) =>
            list.FindFirst((inner, _) => inner == value) is not null && position == 1);

        // Assert
        Assert.Equal(2, Assert.IsType<DoublyLinkedList<int>.Cursor>(result).Value);
        Assert.True(enumerator.MoveNext());
        Assert.Equal(1, enumerator.Current);
    }

    [Fact]
    public void Find_NullAndDefaultValues_ReturnsCursorForStoredValue()
    {
        // Arrange
        var list = new DoublyLinkedList<string?>();
        list.AddLast(null);
        list.AddLast("x");

        // Act / Assert
        Assert.Null(Assert.IsType<DoublyLinkedList<string?>.Cursor>(list.FindFirst((value, _) => value is null)).Value);
        Assert.Null(Assert.IsType<DoublyLinkedList<string?>.Cursor>(list.FindLast((value, _) => value is null)).Value);
        Assert.Null(list.FindFirst((value, _) => value == "missing"));
    }

    private static DoublyLinkedList<int> CreateList()
    {
        var list = new DoublyLinkedList<int>();
        foreach (int value in new[] { 1, 2, 3, 2, 5 })
        {
            list.AddLast(value);
        }
        return list;
    }
}
