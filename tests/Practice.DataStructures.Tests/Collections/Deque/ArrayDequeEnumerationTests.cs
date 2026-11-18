using System.Collections;
using Practice.DataStructures.Core.Collections;
using Xunit;

namespace Practice.DataStructures.Tests.Collections.Deque;

public sealed class ArrayDequeEnumerationTests
{
    public static TheoryData<int, int> ModificationsAndStates
    {
        get
        {
            var data = new TheoryData<int, int>();
            for (int modification = 0; modification < 6; modification++)
            {
                for (int state = 0; state < 4; state++)
                {
                    data.Add(modification, state);
                }
            }
            return data;
        }
    }

    [Theory]
    [MemberData(nameof(ModificationsAndStates))]
    public void Modification_AnyEnumerationState_InvalidatesBothInterfaces(int modification, int state)
    {
        // Arrange: до начала, на элементе, после конца, после Dispose.
        var deque = new ArrayDeque<int>();
        deque.PushBack(1);
        deque.PushBack(2);
        using IEnumerator<int> enumerator = deque.GetEnumerator();
        switch (state)
        {
            case 1:
                Assert.True(enumerator.MoveNext());
                break;
            case 2:
                while (enumerator.MoveNext()) { }
                break;
            case 3:
                enumerator.Dispose();
                break;
        }

        // Act
        switch (modification)
        {
            case 0:
                deque.PushFront(3);
                break;
            case 1:
                deque.PushBack(3);
                break;
            case 2:
                deque.PopFront();
                break;
            case 3:
                deque.PopBack();
                break;
            case 4:
                deque.Clear();
                break;
            case 5:
                deque.PopFront();
                deque.PushBack(1);
                break;
        }

        // Assert
        Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
        Assert.Throws<InvalidOperationException>(() => enumerator.Current);
        Assert.Throws<InvalidOperationException>(() => ((IEnumerator)enumerator).Current);
        enumerator.Dispose();
        enumerator.Dispose();
        Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
        Assert.Throws<InvalidOperationException>(() => enumerator.Current);
        Assert.Throws<NotSupportedException>(() => enumerator.Reset());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Clear_EmptyDeque_InvalidatesEnumerator(bool disposed)
    {
        // Arrange
        var deque = new ArrayDeque<int>();
        using IEnumerator<int> enumerator = deque.GetEnumerator();
        if (disposed)
        {
            enumerator.Dispose();
        }

        // Act
        deque.Clear();

        // Assert
        Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
        Assert.Throws<InvalidOperationException>(() => enumerator.Current);
        Assert.Throws<InvalidOperationException>(() => ((IEnumerator)enumerator).Current);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(9)]
    public void Enumeration_AnySize_ValidatesCurrentEndDisposeAndReset(int count)
    {
        // Arrange
        var deque = new ArrayDeque<int>();
        for (int i = 0; i < count; i++)
        {
            deque.PushBack(i);
        }
        using IEnumerator<int> enumerator = deque.GetEnumerator();

        // Act / Assert
        Assert.Throws<InvalidOperationException>(() => enumerator.Current);
        Assert.Throws<InvalidOperationException>(() => ((IEnumerator)enumerator).Current);
        Assert.Throws<NotSupportedException>(() => enumerator.Reset());
        for (int i = 0; i < count; i++)
        {
            Assert.True(enumerator.MoveNext());
            Assert.Equal(i, enumerator.Current);
            Assert.Equal(i, ((IEnumerator)enumerator).Current);
        }
        Assert.False(enumerator.MoveNext());
        Assert.False(enumerator.MoveNext());
        Assert.Throws<InvalidOperationException>(() => enumerator.Current);
        Assert.Throws<InvalidOperationException>(() => ((IEnumerator)enumerator).Current);
        Assert.Throws<NotSupportedException>(() => enumerator.Reset());
        enumerator.Dispose();
        enumerator.Dispose();
        Assert.False(enumerator.MoveNext());
        Assert.Throws<InvalidOperationException>(() => enumerator.Current);
        Assert.Throws<InvalidOperationException>(() => ((IEnumerator)enumerator).Current);
    }

    [Fact]
    public void Dispose_DuringEnumeration_EndsOnlyThatEnumerator()
    {
        // Arrange
        var deque = new ArrayDeque<int>();
        deque.PushBack(1);
        deque.PushBack(2);
        using IEnumerator<int> first = deque.GetEnumerator();
        using IEnumerator<int> second = deque.GetEnumerator();
        Assert.True(first.MoveNext());

        // Act
        first.Dispose();
        first.Dispose();

        // Assert
        Assert.False(first.MoveNext());
        Assert.Throws<InvalidOperationException>(() => first.Current);
        Assert.Throws<InvalidOperationException>(() => ((IEnumerator)first).Current);
        Assert.True(second.MoveNext());
        Assert.Equal(1, second.Current);
        Assert.Equal([1, 2], deque.ToArray());
    }

    [Fact]
    public void ReadsAndNestedForeach_NonEmptyDeque_PreserveEnumerationAndOrder()
    {
        // Arrange
        var deque = new ArrayDeque<int>();
        deque.PushBack(2);
        deque.PushFront(1);
        deque.PushBack(3);
        using IEnumerator<int> enumerator = deque.GetEnumerator();

        // Act / Assert
        Assert.Equal(1, deque.PeekFront());
        Assert.Equal(3, deque.PeekBack());
        Assert.Equal(3, deque.Count);
        Assert.False(deque.IsEmpty);
        int outerIndex = 0;
        foreach (int item in deque)
        {
            Assert.Equal(++outerIndex, item);
            int innerIndex = 0;
            foreach (int inner in deque)
            {
                Assert.Equal(++innerIndex, inner);
            }
            Assert.Equal(3, innerIndex);
        }
        Assert.Equal(3, outerIndex);
        int untypedIndex = 0;
        foreach (object? item in (IEnumerable)deque)
        {
            Assert.Equal(++untypedIndex, Assert.IsType<int>(item));
        }
        Assert.Equal(3, untypedIndex);
        for (int i = 1; i <= 3; i++)
        {
            Assert.True(enumerator.MoveNext());
            Assert.Equal(i, enumerator.Current);
        }
        Assert.False(enumerator.MoveNext());
        Assert.Equal(3, deque.Count);
    }
}
