using System.Reflection;
using Practice.DataStructures.Core.Collections;
using Xunit;

namespace Practice.DataStructures.Tests.Collections.ArrayList;

public sealed class ArrayListStorageTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void RemoveAt_ReferenceType_ClearsVacatedSlot(int index)
    {
        // Arrange
        var array = new ArrayList<object>(3) { new(), new(), new() };
        int vacatedIndex = array.Count - 1;

        // Act
        array.RemoveAt(index);

        // Assert
        Assert.Null(GetStorage(array)[vacatedIndex]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void RemoveAt_ValueType_ClearsVacatedSlot(int index)
    {
        // Arrange
        var array = new ArrayList<int>(3) { 10, 20, 30 };
        int vacatedIndex = array.Count - 1;

        // Act
        array.RemoveAt(index);

        // Assert
        Assert.Equal(0, GetStorage(array)[vacatedIndex]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void RemoveAt_StructContainingReference_ClearsVacatedSlot(int index)
    {
        // Arrange
        var array = new ArrayList<(object? Reference, int Value)>(3)
        {
            (new object(), 10),
            (new object(), 20),
            (new object(), 30)
        };
        int vacatedIndex = array.Count - 1;

        // Act
        array.RemoveAt(index);

        // Assert
        (object? reference, int value) = GetStorage(array)[vacatedIndex];
        Assert.Null(reference);
        Assert.Equal(0, value);
    }

    [Fact]
    public void Clear_ReferenceType_ClearsOccupiedSlots()
    {
        // Arrange
        var array = new ArrayList<object?>(4) { new(), null, new() };

        // Act
        array.Clear();

        // Assert
        Assert.All(GetStorage(array), Assert.Null);
    }

    [Fact]
    public void Clear_ValueType_ClearsOccupiedSlots()
    {
        // Arrange
        var array = new ArrayList<int>(4) { 10, 20, 30 };

        // Act
        array.Clear();

        // Assert
        Assert.All(GetStorage(array), item => Assert.Equal(0, item));
    }

    [Fact]
    public void Clear_StructContainingReference_ClearsOccupiedSlots()
    {
        // Arrange
        var array = new ArrayList<(object? Reference, int Value)>(4)
        {
            (new object(), 10),
            (new object(), 20),
            (new object(), 30)
        };

        // Act
        array.Clear();

        // Assert
        foreach ((object? reference, int value) in GetStorage(array))
        {
            Assert.Null(reference);
            Assert.Equal(0, value);
        }
    }

    private static T[] GetStorage<T>(ArrayList<T> array)
    {
        // Проверка внутреннего инварианта отделена от тестов публичного контракта.
        FieldInfo? storageField = typeof(ArrayList<T>).GetField("_array", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(storageField);
        return Assert.IsType<T[]>(storageField.GetValue(array));
    }
}
