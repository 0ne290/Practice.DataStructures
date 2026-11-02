using System.Reflection;
using Practice.DataStructures.Core.Collections;
using Xunit;

namespace Practice.DataStructures.Tests.Collections.DoublyLinkedList;

public sealed class DoublyLinkedListLinkTests
{
    [Fact]
    public void Mutations_MixedSequence_PreserveReciprocalLinksAndDetachRemovedNodes()
    {
        // Arrange
        var list = new DoublyLinkedList<int>();
        var random = new Random(24680);
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        FieldInfo headField = Assert.IsType<FieldInfo>(typeof(DoublyLinkedList<int>).GetField("_head", flags), exactMatch: false);
        FieldInfo tailField = Assert.IsType<FieldInfo>(typeof(DoublyLinkedList<int>).GetField("_tail", flags), exactMatch: false);

        // Act / Assert
        for (int step = 0; step < 300; step++)
        {
            list.AddFirst(step);
            list.AddLast(-step);
            list.GetCursorAt(random.Next(list.Count)).InsertBefore(7);
            list.GetCursorAt(random.Next(list.Count)).InsertAfter(8);
            AssertLinks(list, headField, tailField);

            object? removed = headField.GetValue(list);
            Assert.NotNull(removed);
            list.GetCursorAt(0).Remove();
            Assert.Null(ReadProperty(removed, "Previous"));
            Assert.Null(ReadProperty(removed, "Next"));
            Assert.Equal(false, ReadProperty(removed, "IsAttached"));
            Assert.Equal(0, ReadProperty(removed, "Value"));
            Assert.True(list.TryRemoveLast(out _));
            AssertLinks(list, headField, tailField);

            if (step % 11 == 0)
            {
                list.Clear();
                AssertLinks(list, headField, tailField);
            }
        }
    }

    private static void AssertLinks(DoublyLinkedList<int> list, FieldInfo headField, FieldInfo tailField)
    {
        object? head = headField.GetValue(list);
        object? tail = tailField.GetValue(list);
        object? previous = null;
        object? current = head;
        int count = 0;
        while (current is not null)
        {
            Assert.True(count < list.Count, "Forward chain contains a cycle or more nodes than Count.");
            Assert.Same(previous, ReadProperty(current, "Previous"));
            Assert.Equal(true, ReadProperty(current, "IsAttached"));
            previous = current;
            current = ReadProperty(current, "Next");
            count++;
        }
        Assert.Equal(list.Count, count);
        Assert.Same(tail, previous);

        object? next = null;
        current = tail;
        count = 0;
        while (current is not null)
        {
            Assert.True(count < list.Count, "Backward chain contains a cycle or more nodes than Count.");
            Assert.Same(next, ReadProperty(current, "Next"));
            next = current;
            current = ReadProperty(current, "Previous");
            count++;
        }
        Assert.Equal(list.Count, count);
        Assert.Same(head, next);
    }

    private static object? ReadProperty(object node, string name)
    {
        PropertyInfo property = Assert.IsType<PropertyInfo>(
            node.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.NonPublic), exactMatch: false);
        return property.GetValue(node);
    }
}
