using System.Collections;
using Practice.DataStructures.Core.Collections;
using Xunit;

namespace Practice.DataStructures.Tests.Collections.SinglyLinkedList;

public sealed class SinglyLinkedListEnumerationTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    public void MoveNext_CompleteTraversal_RemainsFinished(int count)
    {
        // Arrange
        var list = new SinglyLinkedList<int>();
        int[] expected = [.. Enumerable.Range(1, count)];
        foreach (int value in expected)
        {
            list.Values.AddLast(value);
        }
        using IEnumerator<int> iterator = list.GetEnumerator();

        // Act / Assert
        Assert.Throws<InvalidOperationException>(() => iterator.Current);
        foreach (int value in expected)
        {
            Assert.True(iterator.MoveNext());
            Assert.Equal(value, iterator.Current);
            Assert.Equal(value, ((IEnumerator)iterator).Current);
        }
        Assert.False(iterator.MoveNext());
        Assert.False(iterator.MoveNext());
        Assert.Throws<InvalidOperationException>(() => iterator.Current);
        Assert.Throws<InvalidOperationException>(() => ((IEnumerator)iterator).Current);
        Assert.Throws<NotSupportedException>(() => iterator.Reset());
        Assert.Equal(expected, list);
    }

    [Fact]
    public void Enumeration_NestedAndNonGeneric_UsesIndependentState()
    {
        // Arrange
        var list = new SinglyLinkedList<int>();
        list.Values.AddLast(1);
        list.Values.AddLast(2);
        var pairs = new List<(int, int)>();
        var nonGenericValues = new List<int>();

        // Act
        foreach (int outer in list)
        {
            foreach (int inner in list)
            {
                pairs.Add((outer, inner));
            }
        }
        foreach (object value in (IEnumerable)list)
        {
            nonGenericValues.Add(Assert.IsType<int>(value));
        }

        // Assert
        Assert.Equal(new[] { (1, 1), (1, 2), (2, 1), (2, 2) }, pairs);
        Assert.Equal([1, 2], nonGenericValues);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    public void Mutation_AnyEnumerationState_InvalidatesOldIterators(int mutation)
    {
        // Arrange / Act / Assert: before start, on element, and after completion.
        for (int state = 0; state < 3; state++)
        {
            var list = new SinglyLinkedList<int>();
            list.Values.AddLast(1);
            using IEnumerator<int> iterator = list.GetEnumerator();
            if (state > 0)
            {
                Assert.True(iterator.MoveNext());
            }
            if (state == 2)
            {
                Assert.False(iterator.MoveNext());
            }

            switch (mutation)
            {
                case 0: list.Values.AddFirst(2); break;
                case 1: list.Values.AddLast(2); break;
                case 2: Assert.True(list.Values.AddBefore((_, _) => true, 2)); break;
                case 3: Assert.True(list.Values.AddAfter((_, _) => true, 2)); break;
                case 4: Assert.True(list.Values.ReplaceFirst((_, _) => true, 1)); break;
                case 5: Assert.True(list.Values.RemoveFirst((_, _) => true)); break;
                default: list.Values.Clear(); break;
            }

            Assert.Throws<InvalidOperationException>(() => iterator.MoveNext());
            Assert.Throws<InvalidOperationException>(() => iterator.Current);
            Assert.Throws<InvalidOperationException>(() => ((IEnumerator)iterator).Current);
        }
    }

    [Fact]
    public void ReadsAndMissingMutations_ExistingList_PreserveIterator()
    {
        // Arrange
        var list = new SinglyLinkedList<int>();
        list.Values.AddLast(1);
        using IEnumerator<int> iterator = list.GetEnumerator();

        // Act
        Assert.True(list.Values.TryFirst(out _));
        Assert.True(list.Values.TryLast(out _));
        Assert.True(list.Values.TryFindFirst((_, _) => true, out _));
        Assert.True(list.Values.Contains((_, _) => true));
        Assert.Equal(0, list.Values.PositionOf((_, _) => true));
        Assert.False(list.Values.AddBefore((_, _) => false, 2));
        Assert.False(list.Values.AddAfter((_, _) => false, 2));
        Assert.False(list.Values.ReplaceFirst((_, _) => false, 2));
        Assert.False(list.Values.RemoveFirst((_, _) => false));

        // Assert
        Assert.True(iterator.MoveNext());
        Assert.Equal(1, iterator.Current);
        iterator.Dispose();
        Assert.Equal(1, iterator.Current);
        Assert.False(iterator.MoveNext());
    }

    [Fact]
    public void Clear_EmptyList_InvalidatesIterator()
    {
        // Arrange
        var list = new SinglyLinkedList<int>();
        using IEnumerator<int> iterator = list.GetEnumerator();

        // Act
        list.Values.Clear();

        // Assert
        Assert.Throws<InvalidOperationException>(() => iterator.MoveNext());
    }
}
