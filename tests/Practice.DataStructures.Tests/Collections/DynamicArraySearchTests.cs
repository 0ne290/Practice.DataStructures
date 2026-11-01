using Practice.DataStructures.Core.Collections;
using Xunit;

namespace Practice.DataStructures.Tests.Collections;

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
        int actual = array.IndexOf(item);

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
        bool actual = array.Contains(item);

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
        int index = array.IndexOf(item);
        bool contains = array.Contains(item);

        // Assert
        Assert.Equal(expectedIndex, index);
        Assert.Equal(expectedIndex >= 0, contains);
    }

    [Fact]
    public void Search_EqualDistinctObjects_UsesDefaultEqualityComparer()
    {
        // Arrange
        var stored = new SearchValue(7);
        var sought = new SearchValue(7);
        var array = new DynamicArray<SearchValue> { new(3), stored, new(7) };

        // Act
        int index = array.IndexOf(sought);
        bool contains = array.Contains(sought);

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
        int index = array.IndexOf(item);
        bool contains = array.Contains(item);

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
        Assert.Equal(-1, array.IndexOf(30));
        Assert.False(array.Contains(30));
        Assert.Equal(-1, array.IndexOf(10));
        Assert.False(array.Contains(10));
        Assert.Equal(0, array.IndexOf(0));
        Assert.True(array.Contains(0));
        Assert.Equal(1, array.IndexOf(20));
        Assert.True(array.Contains(20));
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
        int found = array.IndexOf(20);
        int missing = array.IndexOf(0);
        bool contains = array.Contains(10);
        bool absent = array.Contains(99);

        // Assert
        Assert.Equal(1, found);
        Assert.Equal(-1, missing);
        Assert.True(contains);
        Assert.False(absent);
        Assert.Equal(2, array.Count);
        Assert.Equal(8, array.Capacity);
        Assert.Equal(new[] { 10, 20 }, array);
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

    private sealed record SearchValue(int Value);
}
