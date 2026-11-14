using Practice.DataStructures.Core.Collections;
using Xunit;

namespace Practice.DataStructures.Tests.Collections.DynamicArray;

public sealed class DynamicArraySearchTests
{
    [Theory]
    [InlineData(10, 0)]
    [InlineData(20, 1)]
    [InlineData(30, 3)]
    [InlineData(0, -1)]
    [InlineData(99, -1)]
    public void IndexOf_PresentOrMissingValue_ReturnsFirstIndexOrMinusOne(int item, int expected)
    {
        // Arrange
        var array = new DynamicArray<int> { 10, 20, 10, 30 };

        // Act
        int actual = array.IndexOf((value, _) => value == item);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(10, true)]
    [InlineData(20, true)]
    [InlineData(30, true)]
    [InlineData(0, false)]
    [InlineData(99, false)]
    public void Contains_PresentOrMissingValue_ReturnsPresence(int item, bool expected)
    {
        // Arrange
        var array = new DynamicArray<int> { 10, 20, 10, 30 };

        // Act
        bool actual = array.Contains((value, _) => value == item);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(null, 1)]
    [InlineData("first", 0)]
    [InlineData("last", 3)]
    [InlineData("missing", -1)]
    public void Search_NullAndDuplicateReferences_UsesValueEquality(string? item, int expectedIndex)
    {
        // Arrange
        string first = new(['f', 'i', 'r', 's', 't']);
        var array = new DynamicArray<string?> { first, null, null, "last" };

        // Act
        int index = array.IndexOf((value, _) => value == item);
        bool contains = array.Contains((value, _) => value == item);

        // Assert
        Assert.Equal(expectedIndex, index);
        Assert.Equal(expectedIndex >= 0, contains);
    }

    [Fact]
    public void Search_EqualDistinctObjects_UsesEqualityChosenByCaller()
    {
        // Arrange
        var stored = new SearchValue(7);
        var sought = new SearchValue(7);
        var array = new DynamicArray<SearchValue> { new(3), stored, new(7) };

        // Act
        int index = array.IndexOf((value, _) => value.Equals(sought));
        bool contains = array.Contains((value, _) => value.Equals(sought));

        // Assert
        Assert.NotSame(stored, sought);
        Assert.Equal(1, index);
        Assert.True(contains);
        Assert.Equal(sought.Value, array[index].Value);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Search_EmptyArray_IgnoresReservedDefaultValues(bool searchNull)
    {
        // Arrange
        var array = new DynamicArray<string?>();
        string? item = searchNull ? null : "missing";

        // Act
        int index = array.IndexOf((value, _) => value == item);
        bool contains = array.Contains((value, _) => value == item);

        // Assert
        Assert.Equal(-1, index);
        Assert.False(contains);
    }

    [Fact]
    public void Search_AfterRemovalAndReplacement_ReflectsCurrentContents()
    {
        // Arrange
        var array = new DynamicArray<int>(3) { 10, 20, 30 };
        array.RemoveAt(2);
        array[0] = 0;

        // Act / Assert
        Assert.Equal(-1, array.IndexOf((value, _) => value == 30));
        Assert.False(array.Contains((value, _) => value == 30));
        Assert.Equal(-1, array.IndexOf((value, _) => value == 10));
        Assert.False(array.Contains((value, _) => value == 10));
        Assert.Equal(0, array.IndexOf((value, _) => value == 0));
        Assert.True(array.Contains((value, _) => value == 0));
        Assert.Equal(1, array.IndexOf((value, _) => value == 20));
        Assert.True(array.Contains((value, _) => value == 20));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void Search_PresentAndMissingValues_PreservesArrayAndEnumerators(int position)
    {
        // Arrange
        var array = new DynamicArray<int>(8) { 10, 20 };
        using IEnumerator<int> iterator = array.GetEnumerator();
        if (position >= 1)
        {
            Assert.True(iterator.MoveNext());
        }

        if (position == 2)
        {
            Assert.True(iterator.MoveNext());
            Assert.False(iterator.MoveNext());
        }

        // Act
        int found = array.IndexOf((value, _) => value == 20);
        int missing = array.IndexOf((value, _) => value == 0);
        bool contains = array.Contains((value, _) => value == 10);
        bool absent = array.Contains((value, _) => value == 99);

        // Assert
        Assert.Equal(1, found);
        Assert.Equal(-1, missing);
        Assert.True(contains);
        Assert.False(absent);
        Assert.Equal(2, array.Count);
        Assert.Equal(8, array.Capacity);
        Assert.Equal([10, 20], array);
        if (position == 1)
        {
            Assert.Equal(10, iterator.Current);
        }

        for (int index = position; index < array.Count; index++)
        {
            Assert.True(iterator.MoveNext());
            Assert.Equal(array[index], iterator.Current);
        }

        Assert.False(iterator.MoveNext());
    }

    [Theory]
    [InlineData(false, -1, 3)]
    [InlineData(false, 0, 1)]
    [InlineData(false, 2, 3)]
    [InlineData(true, -1, 3)]
    [InlineData(true, 0, 1)]
    [InlineData(true, 2, 3)]
    public void Search_PositionCondition_VisitsInOrderAndStopsAtFirstMatch(bool containsSearch, int target, int expectedCalls)
    {
        // Arrange
        var array = new DynamicArray<int>(8) { 10, 20, 30 };
        var visited = new List<(int Item, int Index)>();
        bool Predicate(int item, int index)
        {
            visited.Add((item, index));
            return index == target;
        }

        // Act / Assert
        if (containsSearch)
        {
            Assert.Equal(target >= 0, array.Contains(Predicate));
        }
        else
        {
            Assert.Equal(target, array.IndexOf(Predicate));
        }

        Assert.Equal(expectedCalls, visited.Count);
        Assert.Equal(new[] { (10, 0), (20, 1), (30, 2) }.Take(expectedCalls), visited);
    }

    [Fact]
    public void Search_EmptyArray_DoesNotInvokePredicate()
    {
        // Arrange
        var array = new DynamicArray<int>();
        int calls = 0;
        bool Predicate(int item, int index)
        {
            calls++;
            return true;
        }

        // Act
        int index = array.IndexOf(Predicate);
        bool contains = array.Contains(Predicate);

        // Assert
        Assert.Equal(-1, index);
        Assert.False(contains);
        Assert.Equal(0, calls);
    }

    [Fact]
    public void Search_ReferenceAndFieldConditions_UsesCallerRules()
    {
        // Arrange
        var first = new SearchValue(7);
        var second = new SearchValue(7);
        var array = new DynamicArray<SearchValue> { first, second, new(9) };

        // Act
        int identityIndex = array.IndexOf((item, _) => ReferenceEquals(item, second));
        int fieldIndex = array.IndexOf((item, _) => item.Value == second.Value);
        bool matchesOtherElement = array.Contains((item, position) => position > 0 && item.Value == array[0].Value);

        // Assert
        Assert.Equal(1, identityIndex);
        Assert.Equal(0, fieldIndex);
        Assert.True(matchesOtherElement);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Search_PredicateThrows_PropagatesSameExceptionAndPreservesState(bool containsSearch)
    {
        // Arrange
        var array = new DynamicArray<int>(8) { 10, 20 };
        using IEnumerator<int> iterator = array.GetEnumerator();
        Assert.True(iterator.MoveNext());
        var expected = new ArithmeticException("Predicate failed.");
        bool Predicate(int item, int index) => throw expected;

        // Act
        Exception? actual = Record.Exception(() =>
        {
            if (containsSearch)
            {
                _ = array.Contains(Predicate);
            }
            else
            {
                _ = array.IndexOf(Predicate);
            }
        });

        // Assert
        Assert.Same(expected, actual);
        Assert.Equal([10, 20], array);
        Assert.Equal(2, array.Count);
        Assert.Equal(8, array.Capacity);
        Assert.Equal(10, iterator.Current);
        Assert.True(iterator.MoveNext());
        Assert.Equal(20, iterator.Current);
        Assert.False(iterator.MoveNext());
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Search_PredicateMutatesArray_ThrowsBeforeUsingResult(bool containsSearch, bool predicateResult)
    {
        // Arrange / Act / Assert: каждый вид изменения проверяется на новом массиве.
        for (int mutation = 0; mutation < 4; mutation++)
        {
            int currentMutation = mutation;
            var array = new DynamicArray<int>(2) { 10, 20 };
            int calls = 0;
            bool Predicate(int item, int index)
            {
                calls++;
                switch (currentMutation)
                {
                    case 0: array.Add(30); break;
                    case 1: array.RemoveAt(0); break;
                    case 2: array.Clear(); break;
                    case 3: array[0] = 10; break;
                }

                return predicateResult;
            }

            Assert.Throws<InvalidOperationException>(() =>
            {
                if (containsSearch)
                {
                    _ = array.Contains(Predicate);
                }
                else
                {
                    _ = array.IndexOf(Predicate);
                }
            });
            Assert.Equal(1, calls);
            int[][] expected = [[10, 20, 30], [20], [], [10, 20]];
            Assert.Equal(expected[mutation], array);
            Assert.Equal(expected[mutation].Length, array.Count);
        }
    }

    [Fact]
    public void Search_NestedSearch_PreservesOuterSearch()
    {
        // Arrange
        var array = new DynamicArray<int> { 10, 20, 30 };

        // Act
        int index = array.IndexOf((item, position) =>
            position > 0 && array.Contains((other, _) => other == item - 10));

        // Assert
        Assert.Equal(1, index);
        Assert.Equal([10, 20, 30], array);
    }

    private sealed record SearchValue(int Value);
}
