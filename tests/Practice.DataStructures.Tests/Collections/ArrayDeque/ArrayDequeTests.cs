using Practice.DataStructures.Core.Collections;
using Practice.DataStructures.Tests.Collections.DequeContract;
using Xunit;

namespace Practice.DataStructures.Tests.Collections.ArrayDeque;

public sealed class ArrayDequeTests : DequeEnumerationContractTests
{
    private protected override DequeSubject<T> CreateDeque<T>() => DequeSubject<T>.CreateArray();

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
}
