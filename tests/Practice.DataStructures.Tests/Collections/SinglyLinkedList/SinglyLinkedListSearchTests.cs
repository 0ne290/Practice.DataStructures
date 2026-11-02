using Practice.DataStructures.Core.Collections;
using Xunit;

namespace Practice.DataStructures.Tests.Collections.SinglyLinkedList;

public sealed class SinglyLinkedListSearchTests
{
    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void Search_SelectedPosition_VisitsInOrderAndStops(int target)
    {
        // Arrange
        var list = new SinglyLinkedList<int>();
        list.Values.AddLast(4);
        list.Values.AddLast(4);
        list.Values.AddLast(4);
        var visited = new List<(int Value, int Position)>();

        // Act
        bool found = list.Values.TryFindFirst((value, position) =>
        {
            visited.Add((value, position));
            return position == target;
        }, out int result);

        // Assert
        Assert.Equal(target >= 0, found);
        Assert.Equal(target >= 0 ? 4 : 0, result);
        Assert.Equal(Enumerable.Range(0, target >= 0 ? target + 1 : 3).Select(index => (4, index)), visited);
        Assert.Equal(target, list.Values.PositionOf((_, index) => index == target));
        Assert.Equal(target >= 0, list.Values.Contains((_, index) => index == target));
    }

    [Fact]
    public void TryRead_DefaultAndNull_DistinguishStoredValuesFromAbsence()
    {
        // Arrange
        var values = new SinglyLinkedList<int>();
        var references = new SinglyLinkedList<string?>();
        values.Values.AddLast(0);
        references.Values.AddLast(null);

        // Act / Assert
        Assert.True(values.Values.TryFirst(out int first));
        Assert.True(values.Values.TryLast(out int last));
        Assert.True(values.Values.TryFindFirst((value, _) => value == 0, out int found));
        Assert.Equal(0, first);
        Assert.Equal(0, last);
        Assert.Equal(0, found);
        Assert.True(references.Values.TryFirst(out string? nullFirst));
        Assert.True(references.Values.TryLast(out string? nullLast));
        Assert.True(references.Values.TryFindFirst((value, _) => value is null, out string? nullFound));
        Assert.Null(nullFirst);
        Assert.Null(nullLast);
        Assert.Null(nullFound);
        references.Values.Clear();
        Assert.False(references.Values.TryFindFirst((_, _) => true, out string? missing));
        Assert.Null(missing);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void Search_NoMatch_CallsPredicateOncePerElement(int count)
    {
        // Arrange
        var list = new SinglyLinkedList<int>();
        if (count == 1)
        {
            list.Values.AddLast(1);
        }
        int calls = 0;

        // Act
        bool found = list.Values.Contains((_, _) =>
        {
            calls++;
            return false;
        });

        // Assert
        Assert.False(found);
        Assert.Equal(count, calls);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Search_PredicateMutates_ThrowsBeforeUsingResult(bool predicateResult)
    {
        // Arrange
        var list = new SinglyLinkedList<int>();
        list.Values.AddLast(1);

        // Act
        Assert.Throws<InvalidOperationException>(() => list.Values.AddAfter((_, _) =>
        {
            list.Values.Clear();
            return predicateResult;
        }, 9));

        // Assert
        Assert.Empty(list);
        Assert.Equal(0, list.Count);
    }

    [Fact]
    public void Search_PredicateThrows_PropagatesOriginalException()
    {
        // Arrange
        var list = new SinglyLinkedList<int>();
        list.Values.AddLast(1);
        var expected = new ArithmeticException("Condition failed.");

        // Act
        ArithmeticException actual = Assert.Throws<ArithmeticException>(() =>
            list.Values.RemoveFirst((_, _) => throw expected));

        // Assert
        Assert.Same(expected, actual);
        Assert.Equal(1, Assert.Single(list));
    }

    [Fact]
    public void Search_NestedRead_PreservesOuterSearch()
    {
        // Arrange
        var list = new SinglyLinkedList<int>();
        list.Values.AddLast(1);
        list.Values.AddLast(2);

        // Act
        int position = list.Values.PositionOf((value, _) => list.Values.Contains((other, _) => other == value) && value == 2);

        // Assert
        Assert.Equal(1, position);
        Assert.Equal(new[] { 1, 2 }, list);
    }
}
