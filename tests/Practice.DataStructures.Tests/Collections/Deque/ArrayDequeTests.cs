using Practice.DataStructures.Core.Collections;
using Xunit;

namespace Practice.DataStructures.Tests.Collections.Deque;

public sealed class ArrayDequeTests
{
    [Fact]
    public void Constructor_NewDeque_IsEmpty()
    {
        // Arrange / Act
        var deque = new ArrayDeque<int>();

        // Assert
        Assert.Equal(0, deque.Count);
        Assert.True(deque.IsEmpty);
        Assert.Empty(deque);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void PushPop_SelectedEnds_PreservesExpectedOrder(bool pushFront, bool popFront)
    {
        // Arrange
        var deque = new ArrayDeque<int>();
        const int count = 129;

        // Act
        for (int i = 0; i < count; i++)
        {
            if (pushFront)
            {
                deque.PushFront(i);
            }
            else
            {
                deque.PushBack(i);
            }
        }

        // Assert
        Assert.Equal(count, deque.Count);
        Assert.False(deque.IsEmpty);
        for (int i = 0; i < count; i++)
        {
            int expected = pushFront == popFront ? count - i - 1 : i;
            Assert.Equal(expected, popFront ? deque.PeekFront() : deque.PeekBack());
            Assert.Equal(expected, popFront ? deque.PopFront() : deque.PopBack());
            Assert.Equal(count - i - 1, deque.Count);
            Assert.Equal(i == count - 1, deque.IsEmpty);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PopPeek_EmptyDeque_ThrowsWithoutInvalidatingEnumeration(bool drainFirst)
    {
        // Arrange
        var deque = new ArrayDeque<int>();
        if (drainFirst)
        {
            deque.PushFront(42);
            Assert.Equal(42, deque.PopBack());
        }
        using IEnumerator<int> enumerator = deque.GetEnumerator();

        // Act / Assert
        Assert.Throws<InvalidOperationException>(() => deque.PopFront());
        Assert.Throws<InvalidOperationException>(() => deque.PopBack());
        Assert.Throws<InvalidOperationException>(() => deque.PeekFront());
        Assert.Throws<InvalidOperationException>(() => deque.PeekBack());
        Assert.False(enumerator.MoveNext());
        Assert.True(deque.IsEmpty);
        Assert.Equal(0, deque.Count);
        deque.PushBack(7);
        Assert.Equal(7, deque.PopFront());
    }

    [Fact]
    public void PushPop_RepeatedWrapAndGrowth_MatchesLinkedList()
    {
        // Arrange
        var deque = new ArrayDeque<int>();
        var expected = new LinkedList<int>();

        // Act / Assert
        for (int i = 0; i < 1024; i++)
        {
            deque.PushBack(i);
            expected.AddLast(i);
            if (i % 3 == 0)
            {
                Assert.Equal(expected.First?.Value, deque.PopFront());
                expected.RemoveFirst();
            }
            if (i % 5 == 0)
            {
                deque.PushFront(-i);
                expected.AddFirst(-i);
            }
            Assert.Equal(expected.ToArray(), deque.ToArray());
            Assert.Equal(expected.Count, deque.Count);
        }
        while (expected.Count > 0)
        {
            if (expected.Count % 2 == 0)
            {
                Assert.Equal(expected.First?.Value, deque.PopFront());
                expected.RemoveFirst();
            }
            else
            {
                Assert.Equal(expected.Last?.Value, deque.PopBack());
                expected.RemoveLast();
            }
        }
        Assert.Empty(deque);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(12345)]
    [InlineData(987654321)]
    public void Operations_SeededSequence_MatchesLinkedListAfterEveryStep(int seed)
    {
        // Arrange
        var random = new Random(seed);
        var deque = new ArrayDeque<int>();
        var expected = new LinkedList<int>();

        // Act / Assert
        for (int step = 0; step < 10000; step++)
        {
            int value = random.Next(-20, 21);
            switch (random.Next(100))
            {
                case < 30:
                    deque.PushFront(value);
                    expected.AddFirst(value);
                    break;
                case < 60:
                    deque.PushBack(value);
                    expected.AddLast(value);
                    break;
                case < 78:
                    if (expected.Count == 0)
                    {
                        Assert.Throws<InvalidOperationException>(() => deque.PopFront());
                    }
                    else
                    {
                        Assert.Equal(expected.First?.Value, deque.PopFront());
                        expected.RemoveFirst();
                    }
                    break;
                case < 96:
                    if (expected.Count == 0)
                    {
                        Assert.Throws<InvalidOperationException>(() => deque.PopBack());
                    }
                    else
                    {
                        Assert.Equal(expected.Last?.Value, deque.PopBack());
                        expected.RemoveLast();
                    }
                    break;
                case 99:
                    deque.Clear();
                    expected.Clear();
                    break;
            }
            Assert.Equal(expected.Count, deque.Count);
            Assert.Equal(expected.Count == 0, deque.IsEmpty);
            Assert.Equal(expected.ToArray(), deque.ToArray());
            if (expected.Count > 0)
            {
                Assert.Equal(expected.First?.Value, deque.PeekFront());
                Assert.Equal(expected.Last?.Value, deque.PeekBack());
            }
        }
    }

    [Fact]
    public void PushPop_NullsDuplicatesAndReferences_PreservesIdentity()
    {
        // Arrange
        var deque = new ArrayDeque<object?>();
        object first = new();
        object second = new();

        // Act
        deque.PushBack(first);
        deque.PushFront(null);
        deque.PushBack(second);
        deque.PushBack(first);

        // Assert
        Assert.Null(deque.PopFront());
        Assert.Same(first, deque.PopBack());
        Assert.Same(second, deque.PopBack());
        Assert.Same(first, deque.PopFront());
        Assert.True(deque.IsEmpty);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(65)]
    public void Clear_AnySize_RemovesItemsAndAllowsReuse(int count)
    {
        // Arrange
        var deque = new ArrayDeque<int>();
        for (int i = 0; i < count; i++)
        {
            deque.PushFront(i);
        }

        // Act
        deque.Clear();
        deque.Clear();

        // Assert
        Assert.True(deque.IsEmpty);
        Assert.Equal(0, deque.Count);
        Assert.Empty(deque);
        deque.PushFront(int.MinValue);
        deque.PushBack(int.MaxValue);
        Assert.Equal(int.MinValue, deque.PopFront());
        Assert.Equal(int.MaxValue, deque.PopBack());
        Assert.True(deque.IsEmpty);
    }
}
