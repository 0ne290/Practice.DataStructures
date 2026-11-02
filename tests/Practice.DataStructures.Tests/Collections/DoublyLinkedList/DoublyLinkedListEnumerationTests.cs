using System.Collections;
using Practice.DataStructures.Core.Collections;
using Xunit;

namespace Practice.DataStructures.Tests.Collections.DoublyLinkedList;

public sealed class DoublyLinkedListEnumerationTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Enumerator_BeforeStartAndAfterEnd_EnforcesCurrentState(bool backwards)
    {
        // Arrange
        var list = new DoublyLinkedList<int>();
        list.AddLast(1);
        list.AddLast(2);
        IEnumerable<int> sequence = backwards ? list.Reverse() : list;
        using IEnumerator<int> enumerator = sequence.GetEnumerator();

        // Act / Assert
        Assert.Throws<InvalidOperationException>(() => enumerator.Current);
        Assert.True(enumerator.MoveNext());
        Assert.Equal(backwards ? 2 : 1, enumerator.Current);
        Assert.True(enumerator.MoveNext());
        Assert.Equal(backwards ? 1 : 2, enumerator.Current);
        Assert.False(enumerator.MoveNext());
        Assert.False(enumerator.MoveNext());
        Assert.Throws<InvalidOperationException>(() => enumerator.Current);
        Assert.Throws<NotSupportedException>(enumerator.Reset);
    }

    [Theory]
    [InlineData(false, "addFirst")]
    [InlineData(true, "addLast")]
    [InlineData(false, "removeFirst")]
    [InlineData(true, "removeLast")]
    [InlineData(false, "insertBefore")]
    [InlineData(true, "insertAfter")]
    [InlineData(false, "removeCursor")]
    [InlineData(true, "value")]
    [InlineData(false, "clear")]
    [InlineData(true, "clear")]
    public void Enumerator_Mutation_InvalidatesBeforeStartDuringAndAfterEnd(bool backwards, string mutation)
    {
        // Arrange
        var list = new DoublyLinkedList<int>();
        list.AddLast(1);
        list.AddLast(2);
        IEnumerable<int> sequence = backwards ? list.Reverse() : list;
        using IEnumerator<int> before = sequence.GetEnumerator();
        using IEnumerator<int> during = sequence.GetEnumerator();
        using IEnumerator<int> after = sequence.GetEnumerator();
        Assert.True(during.MoveNext());
        while (after.MoveNext())
        {
        }
        DoublyLinkedList<int>.Cursor cursor = list.GetCursorAt(0);

        // Act
        switch (mutation)
        {
            case "addFirst": list.AddFirst(3); break;
            case "addLast": list.AddLast(3); break;
            case "removeFirst": Assert.True(list.TryRemoveFirst(out _)); break;
            case "removeLast": Assert.True(list.TryRemoveLast(out _)); break;
            case "insertBefore": cursor.InsertBefore(3); break;
            case "insertAfter": cursor.InsertAfter(3); break;
            case "removeCursor": cursor.Remove(); break;
            case "value": cursor.Value = cursor.Value; break;
            case "clear": list.Clear(); break;
        }

        // Assert
        foreach (IEnumerator<int> enumerator in new[] { before, during, after })
        {
            Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
            Assert.Throws<InvalidOperationException>(() => enumerator.Current);
        }
    }

    [Fact]
    public void Enumerator_EmptyClear_InvalidatesButFailedRemovalDoesNot()
    {
        // Arrange
        var list = new DoublyLinkedList<int>();
        using IEnumerator<int> forward = list.GetEnumerator();
        using IEnumerator<int> backward = list.Reverse().GetEnumerator();

        // Act / Assert
        Assert.False(list.TryRemoveFirst(out _));
        Assert.False(list.TryRemoveLast(out _));
        Assert.False(forward.MoveNext());
        Assert.False(backward.MoveNext());
        list.Clear();
        Assert.Throws<InvalidOperationException>(() => forward.MoveNext());
        Assert.Throws<InvalidOperationException>(() => backward.MoveNext());
    }

    [Fact]
    public void Reverse_DeferredViewAndIndependentEnumerators_UseCurrentList()
    {
        // Arrange
        var list = new DoublyLinkedList<int>();
        IEnumerable<int> reverse = list.Reverse();
        list.AddLast(1);
        list.AddLast(2);

        // Act / Assert
        Assert.Equal(new[] { 2, 1 }, reverse);
        Assert.Equal(new[] { 2, 1 }, reverse);
        using IEnumerator<int> first = reverse.GetEnumerator();
        using IEnumerator<int> second = reverse.GetEnumerator();
        Assert.True(first.MoveNext());
        Assert.True(first.MoveNext());
        Assert.True(second.MoveNext());
        Assert.Equal(1, first.Current);
        Assert.Equal(2, second.Current);
        AssertNonGenericOrder(reverse, [2, 1]);
        AssertNonGenericOrder(list, [1, 2]);
        first.Dispose();
        Assert.Equal(1, first.Current);
    }

    [Fact]
    public void CursorMovementAndReads_DoNotInvalidateEnumeration()
    {
        // Arrange
        var list = new DoublyLinkedList<int>();
        list.AddLast(1);
        list.AddLast(2);
        using IEnumerator<int> enumerator = list.GetEnumerator();
        DoublyLinkedList<int>.Cursor cursor = list.GetCursorAt(0);

        // Act
        Assert.True(cursor.MoveNext());
        Assert.True(cursor.MovePrevious());
        Assert.Equal(1, cursor.Value);
        Assert.Null(list.FindLast((_, _) => false));
        Assert.Throws<ArgumentOutOfRangeException>(() => list.GetCursorAt(-1));

        // Assert
        Assert.True(enumerator.MoveNext());
        Assert.Equal(1, enumerator.Current);
    }

    private static void AssertNonGenericOrder(IEnumerable sequence, int[] expected)
    {
        IEnumerator enumerator = sequence.GetEnumerator();
        try
        {
            Assert.Throws<InvalidOperationException>(() => enumerator.Current);
            foreach (int value in expected)
            {
                Assert.True(enumerator.MoveNext());
                Assert.Equal(value, Assert.IsType<int>(enumerator.Current));
            }
            Assert.False(enumerator.MoveNext());
            Assert.Throws<InvalidOperationException>(() => enumerator.Current);
        }
        finally
        {
            (enumerator as IDisposable)?.Dispose();
        }
    }
}
