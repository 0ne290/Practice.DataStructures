using System.Reflection;
using Practice.DataStructures.Core.Collections;
using Xunit;

namespace Practice.DataStructures.Tests.Collections.CircularArrayList;

public sealed class CircularArrayListStorageTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void RemoveAt_WrappedReferences_ClearsEveryUnusedSlot(int position)
    {
        // Arrange
        var list = new CircularArrayList<object?>(5) { new(), new(), new(), new() };
        list.RemoveAt(0);
        list.RemoveAt(0);
        list.Add(new object());
        list.Add(new object());
        var expected = new List<object?>(list);
        expected.RemoveAt(position);

        // Act
        list.RemoveAt(position);

        // Assert
        Assert.Equal(expected, list);
        object?[] storage = GetStorage(list);
        foreach (object? item in storage)
        {
            if (item is not null)
            {
                Assert.Contains(item, expected);
            }
        }

        Assert.Equal(list.Count, storage.Count(item => item is not null));
    }

    [Fact]
    public void Clear_WrappedStructsContainingReferences_ClearsEntireOccupiedRange()
    {
        // Arrange
        var list = new CircularArrayList<(object? Reference, int Value)>(5);
        for (int value = 1; value <= 4; value++)
        {
            list.Add((new object(), value));
        }

        list.RemoveAt(0);
        list.RemoveAt(0);
        list.Add((new object(), 5));
        list.Add((new object(), 6));

        // Act
        list.Clear();

        // Assert
        Assert.Equal(0, list.Count);
        Assert.Equal(5, list.Capacity);
        Assert.All(GetStorage(list), item =>
        {
            Assert.Null(item.Reference);
            Assert.Equal(0, item.Value);
        });
        list.Insert(0, (new object(), 7));
        Assert.Equal(7, Assert.Single(list).Value);
    }

    private static T[] GetStorage<T>(CircularArrayList<T> list)
    {
        FieldInfo? field = typeof(CircularArrayList<T>).GetField("_array", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        return Assert.IsType<T[]>(field.GetValue(list));
    }
}
