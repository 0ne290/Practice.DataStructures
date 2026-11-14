using Practice.DataStructures.Core.Collections;
using Xunit;

namespace Practice.DataStructures.Tests.Collections.DoublyLinkedList;

public sealed class DoublyLinkedListCursorTests
{
    [Theory]
    [InlineData(1, 0)]
    [InlineData(3, 0)]
    [InlineData(3, 1)]
    [InlineData(3, 2)]
    public void InsertAndRemove_SelectedElement_PreserveBothDirections(int count, int index)
    {
        // Arrange
        var expected = Enumerable.Range(1, count).ToList();
        var list = new DoublyLinkedList<int>();
        foreach (int value in expected)
        {
            list.AddLast(value);
        }
        DoublyLinkedList<int>.Cursor cursor = list.GetCursorAt(index);

        // Act
        cursor.InsertBefore(10);
        cursor.InsertAfter(20);
        cursor.Remove();
        expected.Insert(index, 10);
        expected.Insert(index + 2, 20);
        expected.RemoveAt(index + 1);

        // Assert
        Assert.Equal(expected, list);
        Assert.Equal(expected.AsEnumerable().Reverse(), list.Reverse());
        Assert.Equal(expected.Count, list.Count);
        Assert.Throws<InvalidOperationException>(() => cursor.Value);
    }

    [Fact]
    public void Cursor_ChangesElsewhere_PreservesElementAndObservesNewNeighbors()
    {
        // Arrange
        var list = new DoublyLinkedList<int>();
        list.AddLast(2);
        DoublyLinkedList<int>.Cursor cursor = Assert.IsType<DoublyLinkedList<int>.Cursor>(list.First);
        DoublyLinkedList<int>.Cursor independent = Assert.IsType<DoublyLinkedList<int>.Cursor>(list.First);

        // Act
        list.AddFirst(1);
        list.AddLast(3);
        cursor.Value = 20;
        cursor.InsertBefore(10);
        cursor.InsertAfter(30);

        // Assert
        Assert.Equal(20, cursor.Value);
        Assert.Equal(20, independent.Value);
        Assert.True(cursor.MovePrevious());
        Assert.Equal(10, cursor.Value);
        Assert.Equal(20, independent.Value);
        cursor.Remove();
        Assert.Equal(20, independent.Value);
        Assert.True(independent.MoveNext());
        Assert.Equal(30, independent.Value);
        Assert.Equal([1, 20, 30, 3], list);
        Assert.Equal([3, 30, 20, 1], list.Reverse());
    }

    [Theory]
    [InlineData("cursor")]
    [InlineData("first")]
    [InlineData("last")]
    [InlineData("clear")]
    public void Cursor_ElementRemoved_AllOperationsRejectOldPosition(string removal)
    {
        // Arrange
        var list = new DoublyLinkedList<int>();
        list.AddLast(1);
        DoublyLinkedList<int>.Cursor cursor = list.GetCursorAt(0);
        DoublyLinkedList<int>.Cursor alias = list.GetCursorAt(0);

        // Act
        switch (removal)
        {
            case "cursor": cursor.Remove(); break;
            case "first": Assert.True(list.TryRemoveFirst(out _)); break;
            case "last": Assert.True(list.TryRemoveLast(out _)); break;
            case "clear": list.Clear(); break;
        }
        list.AddLast(2);

        // Assert
        Assert.Throws<InvalidOperationException>(() => cursor.Value);
        Assert.Throws<InvalidOperationException>(() => cursor.Value = 9);
        Assert.Throws<InvalidOperationException>(() => cursor.MoveNext());
        Assert.Throws<InvalidOperationException>(() => cursor.MovePrevious());
        Assert.Throws<InvalidOperationException>(() => cursor.InsertBefore(9));
        Assert.Throws<InvalidOperationException>(() => cursor.InsertAfter(9));
        Assert.Throws<InvalidOperationException>(cursor.Remove);
        Assert.Throws<InvalidOperationException>(() => alias.Value);
        Assert.Equal([2], list);
        Assert.Equal(1, list.Count);
    }

    [Fact]
    public void Navigation_AtEndpoints_FailedMoveKeepsCursorUsable()
    {
        // Arrange
        var list = new DoublyLinkedList<int>();
        list.AddLast(1);
        DoublyLinkedList<int>.Cursor cursor = list.GetCursorAt(0);

        // Act / Assert
        Assert.False(cursor.MovePrevious());
        Assert.False(cursor.MoveNext());
        Assert.Equal(1, cursor.Value);
        cursor.InsertBefore(0);
        cursor.InsertAfter(2);
        Assert.True(cursor.MovePrevious());
        Assert.Equal(0, cursor.Value);
        Assert.False(cursor.MovePrevious());
        Assert.True(cursor.MoveNext());
        Assert.True(cursor.MoveNext());
        Assert.Equal(2, cursor.Value);
        Assert.False(cursor.MoveNext());
        Assert.True(cursor.MovePrevious());
        Assert.Equal(1, cursor.Value);
    }
}
