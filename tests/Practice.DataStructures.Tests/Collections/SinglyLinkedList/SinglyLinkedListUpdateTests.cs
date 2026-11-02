using Practice.DataStructures.Core.Collections;
using Xunit;

namespace Practice.DataStructures.Tests.Collections.SinglyLinkedList;

public sealed class SinglyLinkedListUpdateTests
{
    [Fact]
    public void ReplaceFirst_DuplicateValues_ReplacesOnlyFirstMatch()
    {
        // Arrange
        var list = new SinglyLinkedList<int>();
        list.AddLast(1);
        list.AddLast(2);
        list.AddLast(2);

        // Act
        bool replaced = list.ReplaceFirst((value, _) => value == 2, 9);

        // Assert
        Assert.True(replaced);
        Assert.Equal(new[] { 1, 9, 2 }, list);
        Assert.Equal(3, list.Count);
    }

    [Fact]
    public void UpdateFirst_ReferenceObject_UpdatesFirstMatchAndInvalidatesIterator()
    {
        // Arrange
        var first = new MutableItem { Value = 2 };
        var second = new MutableItem { Value = 2 };
        var list = new SinglyLinkedList<MutableItem>();
        list.AddLast(first);
        list.AddLast(second);
        using IEnumerator<MutableItem> iterator = list.GetEnumerator();

        // Act
        bool updated = list.UpdateFirst((item, _) => item.Value == 2, item => item.Value = 9);

        // Assert
        Assert.True(updated);
        Assert.Equal(9, first.Value);
        Assert.Equal(2, second.Value);
        Assert.True(list.TryFirst(out MutableItem? result));
        Assert.Same(first, result);
        Assert.Equal(2, list.Count);
        Assert.Throws<InvalidOperationException>(() => iterator.MoveNext());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void UpdateFirst_ValueType_ThrowsBeforeSearching(bool populated)
    {
        // Arrange
        var list = new SinglyLinkedList<int>();
        if (populated)
        {
            list.AddLast(1);
        }
        int calls = 0;

        // Act
        Assert.Throws<NotSupportedException>(() => list.UpdateFirst((_, _) =>
        {
            calls++;
            return true;
        }, _ => { }));

        // Assert
        Assert.Equal(0, calls);
        Assert.Equal(populated ? 1 : 0, list.Count);
    }

    [Fact]
    public void UpdateFirst_NullableValueType_IsAlsoRejected()
    {
        // Arrange
        var list = new SinglyLinkedList<int?>();

        // Act / Assert
        Assert.Throws<NotSupportedException>(() => list.UpdateFirst((_, _) => true, _ => { }));
    }

    [Fact]
    public void UpdateFirst_NullElement_PassesNullToAction()
    {
        // Arrange
        var list = new SinglyLinkedList<string?>();
        list.AddLast(null);
        int calls = 0;
        bool receivedNull = false;

        // Act
        bool updated = list.UpdateFirst((item, _) => item is null, item =>
        {
            receivedNull = item is null;
            calls++;
        });

        // Assert
        Assert.True(updated);
        Assert.Equal(1, calls);
        Assert.True(receivedNull);
        Assert.Null(Assert.Single(list));
    }

    [Fact]
    public void UpdateFirst_NoMatch_DoesNotCallActionOrInvalidateIterator()
    {
        // Arrange
        var list = new SinglyLinkedList<string>();
        list.AddLast("one");
        using IEnumerator<string> iterator = list.GetEnumerator();
        int calls = 0;

        // Act
        bool updated = list.UpdateFirst((_, _) => false, _ => calls++);

        // Assert
        Assert.False(updated);
        Assert.Equal(0, calls);
        Assert.True(iterator.MoveNext());
        Assert.Equal("one", iterator.Current);
    }

    [Fact]
    public void UpdateFirst_EmptyAction_StillInvalidatesIterator()
    {
        // Arrange
        var list = new SinglyLinkedList<string>();
        list.AddLast("one");
        using IEnumerator<string> iterator = list.GetEnumerator();

        // Act
        Assert.True(list.UpdateFirst((_, _) => true, _ => { }));

        // Assert
        Assert.Throws<InvalidOperationException>(() => iterator.MoveNext());
    }

    [Fact]
    public void UpdateFirst_ActionMutatesList_ThrowsWithoutRollback()
    {
        // Arrange
        var list = new SinglyLinkedList<string>();
        list.AddLast("one");

        // Act
        Assert.Throws<InvalidOperationException>(() => list.UpdateFirst((_, _) => true, _ => list.Clear()));

        // Assert
        Assert.Empty(list);
        Assert.Equal(0, list.Count);
    }

    [Fact]
    public void UpdateFirst_ActionThrows_PreservesExceptionAndPartialObjectChange()
    {
        // Arrange
        var item = new MutableItem { Value = 1 };
        var list = new SinglyLinkedList<MutableItem>();
        list.AddLast(item);
        using IEnumerator<MutableItem> iterator = list.GetEnumerator();
        var expected = new ArithmeticException("Update failed.");

        // Act
        ArithmeticException actual = Assert.Throws<ArithmeticException>(() => list.UpdateFirst((_, _) => true, value =>
        {
            value.Value = 9;
            throw expected;
        }));

        // Assert
        Assert.Same(expected, actual);
        Assert.Equal(9, item.Value);
        Assert.True(iterator.MoveNext());
        Assert.Same(item, iterator.Current);
    }

    private sealed class MutableItem
    {
        internal int Value { get; set; }
    }
}
