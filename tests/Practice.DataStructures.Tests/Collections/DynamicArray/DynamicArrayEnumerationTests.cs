using System.Collections;
using Practice.DataStructures.Core.Collections;
using Xunit;

namespace Practice.DataStructures.Tests.Collections.DynamicArray;

public sealed class DynamicArrayEnumerationTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(17)]
    public void Enumerate_VaryingCounts_YieldsOnlyStoredItemsInOrder(int count)
    {
        // Arrange
        var array = new DynamicArray<int>(8);
        int[] expected = [.. Enumerable.Range(0, count).Select(index => index % 3 - 1)];
        foreach (int item in expected)
        {
            array.Add(item);
        }

        int originalCapacity = array.Capacity;
        var actual = new List<int>();

        // Act
        foreach (int item in array)
        {
            actual.Add(item);
        }

        // Assert
        Assert.Equal(expected, actual);
        Assert.Equal(count, array.Count);
        Assert.Equal(originalCapacity, array.Capacity);
    }

    [Fact]
    public void GetEnumerator_ThroughGenericInterface_RemainsCompletedAfterLastItem()
    {
        // Arrange
        var array = new DynamicArray<int> { 10, 20 };
        IEnumerable<int> sequence = array;
        using IEnumerator<int> iterator = sequence.GetEnumerator();
        var actual = new List<int>();

        // Act
        while (iterator.MoveNext())
        {
            actual.Add(iterator.Current);
        }

        bool advancedAfterCompletion = iterator.MoveNext();

        // Assert
        Assert.Equal([10, 20], actual);
        Assert.False(advancedAfterCompletion);
    }

    [Fact]
    public void GetEnumerator_ThroughNonGenericInterface_PreservesReferencesAndNull()
    {
        // Arrange
        var array = new DynamicArray<object?>(8);
        object shared = new();
        object?[] expected = [shared, null, shared, new()];
        foreach (object? item in expected)
        {
            array.Add(item);
        }

        IEnumerable sequence = array;
        IEnumerator iterator = sequence.GetEnumerator();
        using var resource = iterator as IDisposable;
        var actual = new List<object?>();

        // Act
        while (iterator.MoveNext())
        {
            actual.Add(iterator.Current);
        }

        // Assert
        Assert.Equal(expected.Length, actual.Count);
        for (int index = 0; index < expected.Length; index++)
        {
            Assert.Same(expected[index], actual[index]);
        }
    }

    [Fact]
    public void Enumerate_RepeatedPasses_ReturnsSameSequence()
    {
        // Arrange
        var array = new DynamicArray<int>(1);
        int[] expected = [7, 0, 7, -4];
        foreach (int item in expected)
        {
            array.Add(item);
        }

        // Act
        int[] firstPass = array.ToArray();
        int[] secondPass = array.ToArray();

        // Assert
        Assert.Equal(expected, firstPass);
        Assert.Equal(expected, secondPass);
    }

    [Fact]
    public void Enumerate_NestedLoops_UseIndependentState()
    {
        // Arrange
        var array = new DynamicArray<int> { 10, 20 };
        var actual = new List<(int Outer, int Inner)>();

        // Act
        foreach (int outer in array)
        {
            foreach (int inner in array)
            {
                actual.Add((outer, inner));
            }
        }

        // Assert
        Assert.Equal([(10, 10), (10, 20), (20, 10), (20, 20)], actual);
    }

    [Fact]
    public void Enumerate_EarlyBreak_DoesNotAffectNextPass()
    {
        // Arrange
        var array = new DynamicArray<int>();
        int[] expected = [10, 20, 30];
        foreach (int item in expected)
        {
            array.Add(item);
        }

        var firstPass = new List<int>();

        // Act
        foreach (int item in array)
        {
            firstPass.Add(item);
            break;
        }

        int[] secondPass = array.ToArray();

        // Assert
        Assert.Equal(10, Assert.Single(firstPass));
        Assert.Equal(expected, secondPass);
        Assert.Equal(expected.Length, array.Count);
    }
}
