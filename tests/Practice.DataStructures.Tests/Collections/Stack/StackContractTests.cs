using System.Collections;
using Xunit;

namespace Practice.DataStructures.Tests.Collections.Stack;

public abstract class StackContractTests
{
    protected abstract StackSubject<T> CreateStack<T>();

    // Адаптер только связывает одинаковые операции двух публичных API.
    protected sealed class StackSubject<T>(
        Func<int> count,
        Func<bool> isEmpty,
        Action<T> push,
        Func<T> pop,
        Func<T> peek,
        Action clear,
        IEnumerable<T> items) : IEnumerable<T>
    {
        public int Count => count();
        public bool IsEmpty => isEmpty();
        public void Push(T item) => push(item);
        public T Pop() => pop();
        public T Peek() => peek();
        public void Clear() => clear();
        public IEnumerator<T> GetEnumerator() => items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)items).GetEnumerator();
    }

    [Fact]
    public void Constructor_NewStack_IsEmpty()
    {
        // Arrange / Act
        StackSubject<int> stack = CreateStack<int>();

        // Assert
        Assert.Equal(0, stack.Count);
        Assert.True(stack.IsEmpty);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(129)]
    public void PushPop_MultipleElements_PreservesLifoAcrossGrowth(int count)
    {
        // Arrange
        StackSubject<int> stack = CreateStack<int>();

        // Act
        for (int i = 0; i < count; i++)
        {
            stack.Push(i);
        }

        // Assert
        Assert.Equal(count, stack.Count);
        Assert.False(stack.IsEmpty);
        for (int i = count - 1; i >= 0; i--)
        {
            Assert.Equal(i, stack.Pop());
            Assert.Equal(i, stack.Count);
            Assert.Equal(i == 0, stack.IsEmpty);
        }
    }

    [Fact]
    public void Peek_NonEmptyStack_PreservesTopAndCount()
    {
        // Arrange
        StackSubject<int> stack = CreateStack<int>();
        stack.Push(10);
        stack.Push(20);

        // Act
        int first = stack.Peek();
        int second = stack.Peek();

        // Assert
        Assert.Equal(20, first);
        Assert.Equal(first, second);
        Assert.Equal(2, stack.Count);
        Assert.Equal(20, stack.Pop());
        Assert.Equal(10, stack.Pop());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PopPeek_EmptyStack_ThrowsAndRemainsReusable(bool drainFirst)
    {
        // Arrange
        StackSubject<int> stack = CreateStack<int>();
        if (drainFirst)
        {
            stack.Push(42);
            stack.Pop();
        }

        // Act
        Assert.Throws<InvalidOperationException>(() => stack.Pop());
        Assert.Throws<InvalidOperationException>(() => stack.Peek());

        // Assert
        Assert.Equal(0, stack.Count);
        Assert.True(stack.IsEmpty);
        stack.Push(17);
        Assert.Equal(17, stack.Peek());
        Assert.Equal(17, stack.Pop());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(40)]
    public void Clear_AnyCount_EmptiesStackAndAllowsReuse(int count)
    {
        // Arrange
        StackSubject<int> stack = CreateStack<int>();
        for (int i = 0; i < count; i++)
        {
            stack.Push(i);
        }

        // Act
        stack.Clear();
        stack.Clear();

        // Assert
        Assert.Equal(0, stack.Count);
        Assert.True(stack.IsEmpty);
        Assert.Throws<InvalidOperationException>(() => stack.Peek());
        Assert.Throws<InvalidOperationException>(() => stack.Pop());
        stack.Push(100);
        stack.Push(200);
        Assert.Equal(200, stack.Pop());
        Assert.Equal(100, stack.Pop());
        Assert.True(stack.IsEmpty);
    }

    [Fact]
    public void PushPop_NullDuplicatesAndReferences_PreservesValuesAndIdentity()
    {
        // Arrange
        StackSubject<object?> stack = CreateStack<object?>();
        object value = new();

        // Act
        stack.Push(null);
        stack.Push(value);
        stack.Push(value);

        // Assert
        Assert.Equal(3, stack.Count);
        Assert.Same(value, stack.Peek());
        Assert.Same(value, stack.Pop());
        Assert.Same(value, stack.Pop());
        Assert.Null(stack.Peek());
        Assert.Null(stack.Pop());
        Assert.True(stack.IsEmpty);
    }

    [Theory]
    [InlineData(12345)]
    [InlineData(9147)]
    public void MixedOperations_FixedSeed_MatchesReferenceStack(int seed)
    {
        // Arrange
        StackSubject<int> stack = CreateStack<int>();
        var expected = new Stack<int>();
        var random = new Random(seed);

        // Act / Assert: compare after each operation.
        for (int i = 0; i < 1000; i++)
        {
            switch (random.Next(10))
            {
                case < 5:
                    int value = random.Next(-10, 11);
                    stack.Push(value);
                    expected.Push(value);
                    break;
                case < 8:
                    if (expected.Count == 0)
                    {
                        Assert.Throws<InvalidOperationException>(() => stack.Pop());
                    }
                    else
                    {
                        Assert.Equal(expected.Pop(), stack.Pop());
                    }
                    break;
                case 8:
                    stack.Clear();
                    expected.Clear();
                    break;
                default:
                    if (expected.Count == 0)
                    {
                        Assert.Throws<InvalidOperationException>(() => stack.Peek());
                    }
                    else
                    {
                        Assert.Equal(expected.Peek(), stack.Peek());
                    }
                    break;
            }

            Assert.Equal(expected.Count, stack.Count);
            Assert.Equal(expected.Count == 0, stack.IsEmpty);
            if (expected.Count > 0)
            {
                Assert.Equal(expected.Peek(), stack.Peek());
            }
        }

        while (expected.Count > 0)
        {
            Assert.Equal(expected.Pop(), stack.Pop());
        }
        Assert.True(stack.IsEmpty);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(25)]
    public void Foreach_AnyCount_VisitsTopToBottomWithoutRemoving(int count)
    {
        // Arrange
        StackSubject<int> stack = CreateStack<int>();
        for (int i = 0; i < count; i++)
        {
            stack.Push(i);
        }
        int[] expected = [.. Enumerable.Range(0, count).Reverse()];
        var actual = new List<int>();
        var untypedActual = new List<int>();

        // Act
        foreach (int item in stack)
        {
            actual.Add(item);
        }
        foreach (int item in (IEnumerable)stack)
        {
            untypedActual.Add(item);
        }

        // Assert
        Assert.Equal(expected, actual);
        Assert.Equal(expected, stack.ToArray());
        Assert.Equal(expected, untypedActual);
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
        StackSubject<int> stack = CreateStack<int>();
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
        StackSubject<object?> stack = CreateStack<object?>();
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
        StackSubject<int> stack = CreateStack<int>();
        stack.Push(7);
        using IEnumerator<int> enumerator = stack.GetEnumerator();
        IEnumerator untyped = enumerator;

        // Act / Assert
        Assert.Throws<InvalidOperationException>(() => enumerator.Current);
        Assert.Throws<InvalidOperationException>(() => untyped.Current);
        Assert.Throws<NotSupportedException>(untyped.Reset);
        Assert.True(enumerator.MoveNext());
        Assert.Equal(7, enumerator.Current);
        Assert.Equal(7, untyped.Current);
        Assert.False(enumerator.MoveNext());
        Assert.False(enumerator.MoveNext());
        Assert.Throws<InvalidOperationException>(() => enumerator.Current);
        Assert.Throws<InvalidOperationException>(() => untyped.Current);
        Assert.Throws<NotSupportedException>(untyped.Reset);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Dispose_BeforeOrDuringEnumeration_CompletesEnumerator(bool moveFirst)
    {
        // Arrange
        StackSubject<int> stack = CreateStack<int>();
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
        StackSubject<int> stack = CreateStack<int>();
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
        StackSubject<int> stack = CreateStack<int>();
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
        StackSubject<int> stack = CreateStack<int>();
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
    [Theory]
    [InlineData("Push")]
    [InlineData("Pop")]
    [InlineData("Clear")]
    public void Mutation_AfterDispose_InvalidatesBothEnumeratorInterfaces(string operation)
    {
        // Arrange
        StackSubject<int> stack = CreateStack<int>();
        stack.Push(1);
        using IEnumerator<int> enumerator = stack.GetEnumerator();
        enumerator.Dispose();

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
    public void Enumerator_EmptyStack_RejectsCurrentBeforeAndAfterEnd()
    {
        // Arrange
        StackSubject<int> stack = CreateStack<int>();
        using IEnumerator<int> enumerator = stack.GetEnumerator();

        // Act / Assert
        Assert.Throws<InvalidOperationException>(() => enumerator.Current);
        Assert.Throws<InvalidOperationException>(() => ((IEnumerator)enumerator).Current);
        Assert.False(enumerator.MoveNext());
        Assert.False(enumerator.MoveNext());
        Assert.Throws<InvalidOperationException>(() => enumerator.Current);
        Assert.Throws<InvalidOperationException>(() => ((IEnumerator)enumerator).Current);
    }
}
