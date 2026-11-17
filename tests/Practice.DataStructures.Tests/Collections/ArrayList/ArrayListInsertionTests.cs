using Practice.DataStructures.Core.Collections;
using Xunit;

namespace Practice.DataStructures.Tests.Collections.ArrayList;

public sealed class ArrayListInsertionTests
{
    [Theory]
    [InlineData(1, 0, 0)]
    [InlineData(1, 1, 0)]
    [InlineData(1, 1, 1)]
    [InlineData(4, 4, 0)]
    [InlineData(4, 4, 2)]
    [InlineData(4, 4, 4)]
    [InlineData(8, 4, 0)]
    [InlineData(8, 4, 2)]
    [InlineData(8, 4, 4)]
    public void Insert_ValidPosition_PreservesOrderAndGrowsOnlyWhenFull(int capacity, int count, int index)
    {
        // Arrange
        var array = new ArrayList<int>(capacity);
        var expected = new List<int>(Enumerable.Range(1, count));
        foreach (int item in expected)
        {
            array.Add(item);
        }

        expected.Insert(index, 99);

        // Act
        array.Insert(index, 99);

        // Assert
        Assert.Equal(count + 1, array.Count);
        Assert.Equal(expected, array);
        Assert.True(array.Capacity >= array.Count);
        if (count < capacity)
        {
            Assert.Equal(capacity, array.Capacity);
        }
        else
        {
            Assert.True(array.Capacity > capacity);
        }

        for (int position = 0; position < expected.Count; position++)
        {
            Assert.Equal(expected[position], array[position]);
        }
    }

    [Theory]
    [InlineData(0, -1)]
    [InlineData(0, 1)]
    [InlineData(0, int.MinValue)]
    [InlineData(0, int.MaxValue)]
    [InlineData(3, -1)]
    [InlineData(3, 4)]
    [InlineData(3, int.MinValue)]
    [InlineData(3, int.MaxValue)]
    public void Insert_InvalidPosition_PreservesListAndEnumerator(int count, int index)
    {
        // Arrange
        var array = new ArrayList<int>(Math.Max(1, count));
        int[] expected = [.. Enumerable.Range(1, count)];
        foreach (int item in expected)
        {
            array.Add(item);
        }

        int capacity = array.Capacity;
        using IEnumerator<int> iterator = array.GetEnumerator();

        // Act
        Exception? exception = Record.Exception(() => array.Insert(index, 99));

        // Assert
        ArgumentOutOfRangeException argumentException = Assert.IsType<ArgumentOutOfRangeException>(exception);
        Assert.Equal(nameof(index), argumentException.ParamName);
        Assert.Equal(index, argumentException.ActualValue);
        Assert.Equal(count, array.Count);
        Assert.Equal(capacity, array.Capacity);
        Assert.Equal(expected, array);
        foreach (int item in expected)
        {
            Assert.True(iterator.MoveNext());
            Assert.Equal(item, iterator.Current);
        }

        Assert.False(iterator.MoveNext());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Insert_NullAndDuplicateReferences_PreservesIdentities(int index)
    {
        // Arrange
        object repeated = new();
        var array = new ArrayList<object?>(3) { repeated, null, repeated };
        var expected = new List<object?>(array);
        expected.Insert(index, null);

        // Act
        array.Insert(index, null);

        // Assert
        Assert.Equal(expected.Count, array.Count);
        for (int position = 0; position < expected.Count; position++)
        {
            Assert.Same(expected[position], array[position]);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void Insert_ExistingEnumerators_InvalidatesEveryTraversalState(int index)
    {
        // Arrange
        var array = new ArrayList<int>(2) { 10, 20 };
        using IEnumerator<int> beforeFirst = array.GetEnumerator();
        using IEnumerator<int> active = array.GetEnumerator();
        using IEnumerator<int> completed = array.GetEnumerator();
        using IEnumerator<int> disposed = array.GetEnumerator();
        Assert.True(active.MoveNext());
        while (completed.MoveNext())
        {
        }

        disposed.Dispose();

        // Act
        array.Insert(index, 0);

        // Assert
        foreach (IEnumerator<int> iterator in new[] { beforeFirst, active, completed, disposed })
        {
            Assert.Throws<InvalidOperationException>(() => iterator.MoveNext());
            Assert.Throws<InvalidOperationException>(() => iterator.Current);
        }
    }

    [Fact]
    public void Insert_FromSearchPredicate_DetectsMutationAndKeepsInsertedElement()
    {
        // Arrange
        var array = new ArrayList<int> { 10, 20 };

        // Act
        Exception? exception = Record.Exception(() => array.IndexOf((_, position) =>
        {
            array.Insert(position, 99);
            return true;
        }));

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
        Assert.Equal(3, array.Count);
        Assert.Equal(new[] { 99, 10, 20 }, array);
    }

    [Theory]
    [InlineData(12345)]
    [InlineData(781)]
    public void Insert_MixedWithOtherOperations_MatchesReferenceList(int seed)
    {
        // Arrange
        var random = new Random(seed);
        var array = new ArrayList<int>(1);
        var expected = new List<int>();

        // Act / Assert
        for (int step = 0; step < 500; step++)
        {
            int value = random.Next(-5, 6);
            int operation = random.Next(6);
            if (operation < 2 || expected.Count == 0)
            {
                int position = random.Next(expected.Count + 1);
                array.Insert(position, value);
                expected.Insert(position, value);
            }
            else if (operation == 2)
            {
                array.Add(value);
                expected.Add(value);
            }
            else if (operation == 3)
            {
                int position = random.Next(expected.Count);
                array.RemoveAt(position);
                expected.RemoveAt(position);
            }
            else if (operation == 4)
            {
                int position = random.Next(expected.Count);
                array[position] = value;
                expected[position] = value;
            }
            else
            {
                array.Clear();
                expected.Clear();
            }

            Assert.Equal(expected.Count, array.Count);
            Assert.True(array.Capacity >= array.Count);
            Assert.Equal(expected, array);
            for (int position = 0; position < expected.Count; position++)
            {
                Assert.Equal(expected[position], array[position]);
            }

            Assert.Equal(expected.IndexOf(value), array.IndexOf((item, _) => item == value));
        }
    }
}
