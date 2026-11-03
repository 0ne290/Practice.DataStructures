using System.Collections;
using Practice.DataStructures.Core.Collections;
using Xunit;

namespace Practice.DataStructures.Tests.Collections.Stack;

public sealed class ArrayStackEnumerationTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(25)]
    public void Foreach_AnyCount_VisitsTopToBottomWithoutRemoving(int count)
    {
        // Arrange
        var stack = new ArrayStack<int>();
        for (int i = 0; i < count; i++)
        {
            stack.Push(i);
        }
        int[] expected = [.. Enumerable.Range(0, count).Reverse()];
        var actual = new List<int>();

        // Act
        foreach (int item in stack)
        {
            actual.Add(item);
        }

        // Assert
        Assert.Equal(expected, actual);
        Assert.Equal(expected, stack.ToArray());
        Assert.Equal(expected, ((IEnumerable)stack).Cast<int>().ToArray());
        Assert.Equal(count, stack.Count);
        foreach (int item in expected)
        {
            Assert.Equal(item, stack.Pop());
        }
    }

    [Fact]
    public void Foreach_NestedAndEarlyExit_KeepsIndependentPositions()
    {
        // Arrange
        var stack = new ArrayStack<int>();
        stack.Push(1);
        stack.Push(2);
        var pairs = new List<(int, int)>();

        // Act
        foreach (int outer in stack)
        {
            foreach (int inner in stack)
            {
                pairs.Add((outer, inner));
            }
        }
        foreach (int item in stack)
        {
            Assert.Equal(2, item);
            break;
        }

        // Assert
        Assert.Equal([(2, 2), (2, 1), (1, 2), (1, 1)], pairs);
        Assert.Equal([2, 1], stack.ToArray());
    }

    [Fact]
    public void Enumerator_NullAndDuplicates_PreservesIdentity()
    {
        // Arrange
        var stack = new ArrayStack<object?>();
        object value = new();
        stack.Push(value);
        stack.Push(null);
        stack.Push(value);

        // Act
        object?[] actual = [.. stack];

        // Assert
        Assert.Equal(3, actual.Length);
        Assert.Same(value, actual[0]);
        Assert.Null(actual[1]);
        Assert.Same(value, actual[2]);
    }

    [Fact]
    public void Enumerator_InvalidPositions_RejectsCurrentAndReset()
    {
        // Arrange
        var stack = new ArrayStack<int>();
        stack.Push(7);
        using IEnumerator<int> enumerator = stack.GetEnumerator();
        IEnumerator untyped = enumerator;

        // Act / Assert
        Assert.Throws<InvalidOperationException>(() => enumerator.Current);
        Assert.Throws<InvalidOperationException>(() => untyped.Current);
        Assert.Throws<NotSupportedException>(() => untyped.Reset());
        Assert.True(enumerator.MoveNext());
        Assert.Equal(7, enumerator.Current);
        Assert.Equal(7, untyped.Current);
        Assert.False(enumerator.MoveNext());
        Assert.False(enumerator.MoveNext());
        Assert.Throws<InvalidOperationException>(() => enumerator.Current);
        Assert.Throws<InvalidOperationException>(() => untyped.Current);
        Assert.Throws<NotSupportedException>(() => untyped.Reset());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Dispose_BeforeOrDuringEnumeration_CompletesEnumerator(bool moveFirst)
    {
        // Arrange
        var stack = new ArrayStack<int>();
        stack.Push(1);
        using IEnumerator<int> enumerator = stack.GetEnumerator();
        if (moveFirst)
        {
            Assert.True(enumerator.MoveNext());
        }

        // Act
        enumerator.Dispose();
        enumerator.Dispose();

        // Assert
        Assert.False(enumerator.MoveNext());
        Assert.Throws<InvalidOperationException>(() => enumerator.Current);
        Assert.Equal([1], stack.ToArray());
    }

    [Theory]
    [InlineData("Push", 0)]
    [InlineData("Push", 1)]
    [InlineData("Push", 2)]
    [InlineData("Pop", 0)]
    [InlineData("Pop", 1)]
    [InlineData("Pop", 2)]
    [InlineData("Clear", 0)]
    [InlineData("Clear", 1)]
    [InlineData("Clear", 2)]
    public void Mutation_AnyEnumerationStage_InvalidatesEnumerator(string operation, int stage)
    {
        // Arrange
        var stack = new ArrayStack<int>();
        stack.Push(1);
        using IEnumerator<int> enumerator = stack.GetEnumerator();
        if (stage >= 1)
        {
            Assert.True(enumerator.MoveNext());
        }
        if (stage == 2)
        {
            Assert.False(enumerator.MoveNext());
        }

        // Act
        switch (operation)
        {
            case "Push": stack.Push(2); break;
            case "Pop": stack.Pop(); break;
            case "Clear": stack.Clear(); break;
        }

        // Assert
        Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
        Assert.Throws<InvalidOperationException>(() => enumerator.Current);
        Assert.Throws<InvalidOperationException>(() => ((IEnumerator)enumerator).Current);
    }

    [Fact]
    public void Clear_EmptyStack_InvalidatesExistingEnumerator()
    {
        // Arrange
        var stack = new ArrayStack<int>();
        using IEnumerator<int> enumerator = stack.GetEnumerator();

        // Act
        stack.Clear();

        // Assert
        Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
        Assert.Throws<InvalidOperationException>(() => enumerator.Current);
    }

    [Fact]
    public void ReadsAndRejectedPop_ExistingEnumerators_PreserveEnumeration()
    {
        // Arrange
        var stack = new ArrayStack<int>();
        using IEnumerator<int> empty = stack.GetEnumerator();

        // Act / Assert
        Assert.Throws<InvalidOperationException>(() => stack.Pop());
        Assert.Throws<InvalidOperationException>(() => stack.Peek());
        Assert.False(empty.MoveNext());
        stack.Push(4);
        using IEnumerator<int> filled = stack.GetEnumerator();
        Assert.Equal(4, stack.Peek());
        Assert.Equal(1, stack.Count);
        Assert.False(stack.IsEmpty);
        Assert.True(filled.MoveNext());
        Assert.Equal(4, filled.Current);
        Assert.Equal(4, stack.Peek());
        Assert.False(filled.MoveNext());
    }
}
