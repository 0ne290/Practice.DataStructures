using System.Globalization;
using Practice.DataStructures.Core.Collections;
using Xunit;

namespace Practice.DataStructures.Tests.Collections.DoublyLinkedList;

public sealed class DoublyLinkedListLaboratoryTests(ITestOutputHelper output)
{
    [Fact]
    [Trait("Category", "Laboratory")]
    public void DoublyLinkedList_RandomData_DemonstratesBidirectionalLocalOperations()
    {
        // Arrange
        const string seedVariable = "DATASTRUCTURES_LAB_SEED";
        const int count = 8;
        const int minimum = -9;
        const int maximum = 9;
        string? suppliedSeed = Environment.GetEnvironmentVariable(seedVariable);
        int seed;
        if (suppliedSeed is null)
        {
            seed = Random.Shared.Next();
        }
        else if (!int.TryParse(suppliedSeed, NumberStyles.Integer, CultureInfo.InvariantCulture, out seed))
        {
            throw new InvalidOperationException($"{seedVariable} must contain a 32-bit integer seed.");
        }

        output.WriteLine($"Seed={seed}; элементов={count}; диапазон=[{minimum}, {maximum}]; вставки=100,200; замена=300; дубликат=42.");
        output.WriteLine($"Повтор: --environment {seedVariable}={seed}");
        var random = new Random(seed);
        int[] input = [.. Enumerable.Range(0, count).Select(_ => random.Next(minimum, maximum + 1))];
        output.WriteLine($"Вход: [{string.Join(", ", input)}]");
        var list = new DoublyLinkedList<int>();
        var expected = new List<int>(input);

        // Act / Assert: поиск с разных концов и позиции от головы.
        foreach (int value in input)
        {
            list.AddLast(value);
        }
        list.AddFirst(42);
        list.AddLast(42);
        expected.Insert(0, 42);
        expected.Add(42);
        int forwardCalls = 0;
        int backwardCalls = 0;
        DoublyLinkedList<int>.Cursor first = Assert.IsType<DoublyLinkedList<int>.Cursor>(list.FindFirst((value, position) =>
        {
            forwardCalls++;
            output.WriteLine($"Поиск от головы: позиция={position}, значение={value}");
            return value == 42;
        }));
        DoublyLinkedList<int>.Cursor last = Assert.IsType<DoublyLinkedList<int>.Cursor>(list.FindLast((value, position) =>
        {
            backwardCalls++;
            output.WriteLine($"Поиск от хвоста: позиция={position}, значение={value}");
            return value == 42;
        }));
        Assert.Equal(1, forwardCalls);
        Assert.Equal(1, backwardCalls);
        Assert.Equal(42, first.Value);
        Assert.Equal(42, last.Value);

        // Один выбор позиции; дальнейшие локальные операции не требуют поиска.
        int selectedIndex = list.Count - 2;
        output.WriteLine($"Выбираем позицию {selectedIndex} с ближайшего конца — один переход от хвоста; затем работаем курсором за O(1).");
        DoublyLinkedList<int>.Cursor cursor = list.GetCursorAt(selectedIndex);
        int selectedValue = cursor.Value;
        cursor.InsertBefore(100);
        cursor.InsertAfter(200);
        expected.Insert(selectedIndex, 100);
        expected.Insert(selectedIndex + 2, 200);
        Assert.Equal(selectedValue, cursor.Value);
        Assert.True(cursor.MovePrevious());
        Assert.Equal(100, cursor.Value);
        cursor.Value = 300;
        expected[selectedIndex] = 300;
        Assert.True(cursor.MoveNext());
        Assert.True(cursor.MoveNext());
        Assert.Equal(200, cursor.Value);
        cursor.Remove();
        expected.RemoveAt(selectedIndex + 2);
        Assert.Throws<InvalidOperationException>(() => cursor.Value);
        Assert.Equal(expected, list);
        Assert.Equal(expected.AsEnumerable().Reverse(), list.Reverse());
        output.WriteLine($"После локальных вставок, движения назад/вперёд, замены и удаления: [{string.Join(", ", list)}]");
        output.WriteLine($"Обратный обход без копии: [{string.Join(", ", list.Reverse())}]");
        output.WriteLine("Курсоры на оба значения 42 сохранились после изменений в середине.");
        Assert.Equal(42, first.Value);
        Assert.Equal(42, last.Value);

        // Удаление хвоста за O(1), устойчивость другого курсора и защита перечислителя.
        using IEnumerator<int> enumerator = list.Reverse().GetEnumerator();
        Assert.True(enumerator.MoveNext());
        Assert.True(list.TryRemoveLast(out int removed));
        Assert.Equal(42, removed);
        expected.RemoveAt(expected.Count - 1);
        Assert.Throws<InvalidOperationException>(() => last.Value);
        Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
        Assert.Equal(42, first.Value);
        Assert.Equal(expected, list);
        output.WriteLine($"Удалён хвост={removed}; курсор на голову действителен; старый обратный перечислитель обнаружил изменение.");

        list.Clear();
        Assert.Throws<InvalidOperationException>(() => first.Value);
        Assert.Empty(list);
        Assert.Empty(list.Reverse());
        Assert.Equal(0, list.Count);
        Assert.Throws<ArgumentOutOfRangeException>(() => list.GetCursorAt(0));
        list.AddLast(7);
        Assert.True(list.TryRemoveFirst(out int reused));
        Assert.Equal(7, reused);
        Assert.False(list.TryRemoveLast(out _));
        output.WriteLine("Очистка инвалидировала оставшиеся курсоры; повторное использование и удаление единственного элемента прошли.");
    }
}
