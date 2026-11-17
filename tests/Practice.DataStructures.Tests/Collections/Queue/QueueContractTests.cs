using System.Collections;
using Xunit;

namespace Practice.DataStructures.Tests.Collections.Queue;

public abstract class QueueContractTests
{
    protected abstract QueueSubject<T> CreateQueue<T>();

    [Theory]
    [InlineData(1)]
    [InlineData(8)]
    [InlineData(9)]
    public void EnqueueDequeue_InterleavedCycles_PreservesFifoAcrossWrapAndGrowth(int width)
    {
        // Arrange
        QueueSubject<int> queue = CreateQueue<int>();
        var expected = new Queue<int>();
        int next = 0;

        // Act / Assert: смена головы, повторное использование и рост непустой очереди.
        for (int cycle = 0; cycle < 20; cycle++)
        {
            for (int i = 0; i < width + cycle; i++)
            {
                queue.Enqueue(next);
                expected.Enqueue(next++);
            }
            for (int i = 0; i < width; i++)
            {
                Assert.Equal(expected.Peek(), queue.Peek());
                Assert.Equal(expected.Dequeue(), queue.Dequeue());
            }
            Assert.Equal(expected.ToArray(), queue.ToArray());
            Assert.Equal(expected.Count, queue.Count);
        }
        while (expected.Count > 0)
        {
            Assert.Equal(expected.Dequeue(), queue.Dequeue());
        }
        Assert.True(queue.IsEmpty);
    }

    [Fact]
    public void DequeueEnqueue_UnchangedCount_InvalidatesEnumerator()
    {
        // Arrange
        QueueSubject<int> queue = CreateQueue<int>();
        queue.Enqueue(1);
        queue.Enqueue(2);
        using IEnumerator<int> enumerator = queue.GetEnumerator();
        Assert.True(enumerator.MoveNext());

        // Act
        Assert.Equal(1, queue.Dequeue());
        queue.Enqueue(3);

        // Assert
        Assert.Equal(2, queue.Count);
        Assert.Equal([2, 3], queue.ToArray());
        Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
        Assert.Throws<InvalidOperationException>(() => enumerator.Current);
        Assert.Throws<InvalidOperationException>(() => ((IEnumerator)enumerator).Current);
    }

    // Адаптер только связывает одинаковые операции двух публичных API.
    protected sealed class QueueSubject<T>(
        Func<int> count,
        Func<bool> isEmpty,
        Action<T> enqueue,
        Func<T> dequeue,
        Func<T> peek,
        Action clear,
        IEnumerable<T> items) : IEnumerable<T>
    {
        public int Count => count();
        public bool IsEmpty => isEmpty();
        public void Enqueue(T item) => enqueue(item);
        public T Dequeue() => dequeue();
        public T Peek() => peek();
        public void Clear() => clear();
        public IEnumerator<T> GetEnumerator() => items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)items).GetEnumerator();
    }

    [Fact]
    public void Constructor_NewQueue_IsEmpty()
    {
        // Arrange / Act
        QueueSubject<int> queue = CreateQueue<int>();

        // Assert
        Assert.Equal(0, queue.Count);
        Assert.True(queue.IsEmpty);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(129)]
    public void EnqueueDequeue_MultipleElements_PreservesFifoAcrossGrowth(int count)
    {
        // Arrange
        QueueSubject<int> queue = CreateQueue<int>();

        // Act
        for (int i = 0; i < count; i++)
        {
            queue.Enqueue(i);
        }

        // Assert
        Assert.Equal(count, queue.Count);
        Assert.False(queue.IsEmpty);
        for (int i = 0; i < count; i++)
        {
            Assert.Equal(i, queue.Dequeue());
            Assert.Equal(count - i - 1, queue.Count);
            Assert.Equal(i == count - 1, queue.IsEmpty);
        }
    }

    [Fact]
    public void Peek_NonEmptyQueue_PreservesHeadAndCount()
    {
        // Arrange
        QueueSubject<int> queue = CreateQueue<int>();
        queue.Enqueue(10);
        queue.Enqueue(20);

        // Act
        int first = queue.Peek();
        int second = queue.Peek();

        // Assert
        Assert.Equal(10, first);
        Assert.Equal(first, second);
        Assert.Equal(2, queue.Count);
        Assert.Equal(10, queue.Dequeue());
        Assert.Equal(20, queue.Dequeue());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DequeuePeek_EmptyQueue_ThrowsAndRemainsReusable(bool drainFirst)
    {
        // Arrange
        QueueSubject<int> queue = CreateQueue<int>();
        if (drainFirst)
        {
            queue.Enqueue(42);
            queue.Dequeue();
        }

        // Act
        Assert.Throws<InvalidOperationException>(() => queue.Dequeue());
        Assert.Throws<InvalidOperationException>(() => queue.Peek());

        // Assert
        Assert.Equal(0, queue.Count);
        Assert.True(queue.IsEmpty);
        queue.Enqueue(17);
        Assert.Equal(17, queue.Peek());
        Assert.Equal(17, queue.Dequeue());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(40)]
    public void Clear_AnyCount_EmptiesQueueAndAllowsReuse(int count)
    {
        // Arrange
        QueueSubject<int> queue = CreateQueue<int>();
        for (int i = 0; i < count; i++)
        {
            queue.Enqueue(i);
        }

        // Act
        queue.Clear();
        queue.Clear();

        // Assert
        Assert.Equal(0, queue.Count);
        Assert.True(queue.IsEmpty);
        Assert.Throws<InvalidOperationException>(() => queue.Peek());
        Assert.Throws<InvalidOperationException>(() => queue.Dequeue());
        queue.Enqueue(100);
        queue.Enqueue(200);
        Assert.Equal(100, queue.Dequeue());
        Assert.Equal(200, queue.Dequeue());
        Assert.True(queue.IsEmpty);
    }

    [Fact]
    public void EnqueueDequeue_NullDuplicatesAndReferences_PreservesValuesAndIdentity()
    {
        // Arrange
        QueueSubject<object?> queue = CreateQueue<object?>();
        object value = new();

        // Act
        queue.Enqueue(null);
        queue.Enqueue(value);
        queue.Enqueue(value);

        // Assert
        Assert.Equal(3, queue.Count);
        Assert.Null(queue.Peek());
        Assert.Null(queue.Dequeue());
        Assert.Same(value, queue.Peek());
        Assert.Same(value, queue.Dequeue());
        Assert.Same(value, queue.Dequeue());
        Assert.True(queue.IsEmpty);
    }

    [Theory]
    [InlineData(12345)]
    [InlineData(9147)]
    public void MixedOperations_FixedSeed_MatchesReferenceQueue(int seed)
    {
        // Arrange
        QueueSubject<int> queue = CreateQueue<int>();
        var expected = new Queue<int>();
        var random = new Random(seed);

        // Act / Assert: compare after each operation.
        for (int i = 0; i < 1000; i++)
        {
            switch (random.Next(10))
            {
                case < 5:
                    int value = random.Next(-10, 11);
                    queue.Enqueue(value);
                    expected.Enqueue(value);
                    break;
                case < 8:
                    if (expected.Count == 0)
                    {
                        Assert.Throws<InvalidOperationException>(() => queue.Dequeue());
                    }
                    else
                    {
                        Assert.Equal(expected.Dequeue(), queue.Dequeue());
                    }
                    break;
                case 8:
                    queue.Clear();
                    expected.Clear();
                    break;
                default:
                    if (expected.Count == 0)
                    {
                        Assert.Throws<InvalidOperationException>(() => queue.Peek());
                    }
                    else
                    {
                        Assert.Equal(expected.Peek(), queue.Peek());
                    }
                    break;
            }

            Assert.Equal(expected.ToArray(), queue.ToArray());
            Assert.Equal(expected.Count, queue.Count);
            Assert.Equal(expected.Count == 0, queue.IsEmpty);
            if (expected.Count > 0)
            {
                Assert.Equal(expected.Peek(), queue.Peek());
            }
        }

        while (expected.Count > 0)
        {
            Assert.Equal(expected.Dequeue(), queue.Dequeue());
        }
        Assert.True(queue.IsEmpty);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(25)]
    public void Foreach_AnyCount_VisitsHeadToTailWithoutRemoving(int count)
    {
        // Arrange
        QueueSubject<int> queue = CreateQueue<int>();
        for (int i = 0; i < count; i++)
        {
            queue.Enqueue(i);
        }
        int[] expected = [.. Enumerable.Range(0, count)];
        var actual = new List<int>();
        var untypedActual = new List<int>();

        // Act
        foreach (int item in queue)
        {
            actual.Add(item);
        }
        foreach (int item in (IEnumerable)queue)
        {
            untypedActual.Add(item);
        }

        // Assert
        Assert.Equal(expected, actual);
        Assert.Equal(expected, queue.ToArray());
        Assert.Equal(expected, untypedActual);
        Assert.Equal(count, queue.Count);
        foreach (int item in expected)
        {
            Assert.Equal(item, queue.Dequeue());
        }
    }

    [Fact]
    public void Foreach_NestedAndEarlyExit_KeepsIndependentPositions()
    {
        // Arrange
        QueueSubject<int> queue = CreateQueue<int>();
        queue.Enqueue(1);
        queue.Enqueue(2);
        var pairs = new List<(int, int)>();

        // Act
        foreach (int outer in queue)
        {
            foreach (int inner in queue)
            {
                pairs.Add((outer, inner));
            }
        }
        foreach (int item in queue)
        {
            Assert.Equal(1, item);
            break;
        }

        // Assert
        Assert.Equal([(1, 1), (1, 2), (2, 1), (2, 2)], pairs);
        Assert.Equal([1, 2], [.. queue]);
    }

    [Fact]
    public void Enumerator_NullAndDuplicates_PreservesIdentity()
    {
        // Arrange
        QueueSubject<object?> queue = CreateQueue<object?>();
        object value = new();
        queue.Enqueue(value);
        queue.Enqueue(null);
        queue.Enqueue(value);

        // Act
        object?[] actual = [.. queue];

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
        QueueSubject<int> queue = CreateQueue<int>();
        queue.Enqueue(7);
        using IEnumerator<int> enumerator = queue.GetEnumerator();
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
        QueueSubject<int> queue = CreateQueue<int>();
        queue.Enqueue(1);
        using IEnumerator<int> enumerator = queue.GetEnumerator();
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
        Assert.Throws<InvalidOperationException>(() => ((IEnumerator)enumerator).Current);
        Assert.Equal([1], [.. queue]);
    }

    [Theory]
    [InlineData("Enqueue", 0)]
    [InlineData("Enqueue", 1)]
    [InlineData("Enqueue", 2)]
    [InlineData("Dequeue", 0)]
    [InlineData("Dequeue", 1)]
    [InlineData("Dequeue", 2)]
    [InlineData("Clear", 0)]
    [InlineData("Clear", 1)]
    [InlineData("Clear", 2)]
    public void Mutation_AnyEnumerationStage_InvalidatesEnumerator(string operation, int stage)
    {
        // Arrange
        QueueSubject<int> queue = CreateQueue<int>();
        queue.Enqueue(1);
        using IEnumerator<int> enumerator = queue.GetEnumerator();
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
            case "Enqueue": queue.Enqueue(2); break;
            case "Dequeue": queue.Dequeue(); break;
            case "Clear": queue.Clear(); break;
        }

        // Assert
        Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
        Assert.Throws<InvalidOperationException>(() => enumerator.Current);
        Assert.Throws<InvalidOperationException>(() => ((IEnumerator)enumerator).Current);
    }

    [Fact]
    public void Clear_EmptyQueue_InvalidatesExistingEnumerator()
    {
        // Arrange
        QueueSubject<int> queue = CreateQueue<int>();
        using IEnumerator<int> enumerator = queue.GetEnumerator();

        // Act
        queue.Clear();

        // Assert
        Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
        Assert.Throws<InvalidOperationException>(() => enumerator.Current);
    }

    [Fact]
    public void ReadsAndRejectedDequeue_ExistingEnumerators_PreserveEnumeration()
    {
        // Arrange
        QueueSubject<int> queue = CreateQueue<int>();
        using IEnumerator<int> empty = queue.GetEnumerator();

        // Act / Assert
        Assert.Throws<InvalidOperationException>(() => queue.Dequeue());
        Assert.Throws<InvalidOperationException>(() => queue.Peek());
        Assert.False(empty.MoveNext());
        queue.Enqueue(4);
        using IEnumerator<int> filled = queue.GetEnumerator();
        Assert.Equal(4, queue.Peek());
        Assert.Equal(1, queue.Count);
        Assert.False(queue.IsEmpty);
        Assert.True(filled.MoveNext());
        Assert.Equal(4, filled.Current);
        Assert.Equal(4, queue.Peek());
        Assert.False(filled.MoveNext());
    }
    [Theory]
    [InlineData("Enqueue")]
    [InlineData("Dequeue")]
    [InlineData("Clear")]
    public void Mutation_AfterDispose_InvalidatesBothEnumeratorInterfaces(string operation)
    {
        // Arrange
        QueueSubject<int> queue = CreateQueue<int>();
        queue.Enqueue(1);
        using IEnumerator<int> enumerator = queue.GetEnumerator();
        enumerator.Dispose();

        // Act
        switch (operation)
        {
            case "Enqueue": queue.Enqueue(2); break;
            case "Dequeue": queue.Dequeue(); break;
            case "Clear": queue.Clear(); break;
        }

        // Assert
        Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
        Assert.Throws<InvalidOperationException>(() => enumerator.Current);
        Assert.Throws<InvalidOperationException>(() => ((IEnumerator)enumerator).Current);
    }

    [Fact]
    public void Enumerator_EmptyQueue_RejectsCurrentBeforeAndAfterEnd()
    {
        // Arrange
        QueueSubject<int> queue = CreateQueue<int>();
        using IEnumerator<int> enumerator = queue.GetEnumerator();

        // Act / Assert
        Assert.Throws<InvalidOperationException>(() => enumerator.Current);
        Assert.Throws<InvalidOperationException>(() => ((IEnumerator)enumerator).Current);
        Assert.False(enumerator.MoveNext());
        Assert.False(enumerator.MoveNext());
        Assert.Throws<InvalidOperationException>(() => enumerator.Current);
        Assert.Throws<InvalidOperationException>(() => ((IEnumerator)enumerator).Current);
    }
}
