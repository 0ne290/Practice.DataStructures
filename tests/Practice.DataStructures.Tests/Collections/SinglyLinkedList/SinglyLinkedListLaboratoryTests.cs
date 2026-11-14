using System.Globalization;
using System.Text;
using Practice.DataStructures.Core.Collections;
using Xunit;

namespace Practice.DataStructures.Tests.Collections.SinglyLinkedList;

public sealed class SinglyLinkedListLaboratoryTests(ITestOutputHelper output)
{
    [Fact]
    [Trait("Category", "Laboratory")]
    public void SinglyLinkedList_RandomData_DemonstratesAcceptedContract()
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

        output.WriteLine($"Seed={seed}; элементов={count}; диапазон=[{minimum}, {maximum}]; вставки=100,200; замена=300; повторное значение=42; курсорные значения=0,1,2,3,10.");
        output.WriteLine($"Повтор: --environment {seedVariable}={seed}");
        var random = new Random(seed);
        int[] input = [.. Enumerable.Range(0, count).Select(_ => random.Next(minimum, maximum + 1))];
        output.WriteLine($"Вход: [{string.Join(", ", input)}]");
        var list = new SinglyLinkedList<int>();
        var expected = new List<int>();

        // Act / Assert: вставка с двух концов и рядом с выбранной позицией.
        foreach (int value in input)
        {
            list.Values.AddLast(value);
            expected.Add(value);
        }
        list.Values.AddFirst(100);
        expected.Insert(0, 100);
        Assert.True(list.Values.AddBefore((_, position) => position == 2, 200));
        expected.Insert(2, 200);
        Assert.True(list.Values.AddAfter((_, position) => position == expected.Count - 1, 200));
        expected.Add(200);
        Assert.Equal(expected, list);
        Assert.True(list.Values.TryFirst(out int first));
        Assert.True(list.Values.TryLast(out int last));
        Assert.Equal(100, first);
        Assert.Equal(200, last);
        Assert.True(list.Values.TryFindFirst((value, _) => value == 200, out int found));
        Assert.Equal(200, found);
        Assert.Equal(2, list.Values.PositionOf((value, _) => value == 200));
        Assert.True(list.Values.Contains((value, _) => value == 200));
        output.WriteLine($"После вставок: [{string.Join(", ", list)}]; First={first}, Last={last}, первая позиция 200=2.");

        Assert.True(list.Values.ReplaceFirst((value, _) => value == 200, 300));
        expected[2] = 300;
        Assert.True(list.Values.RemoveFirst((_, position) => position == 0));
        expected.RemoveAt(0);
        Assert.True(list.Values.RemoveFirst((_, position) => position == expected.Count - 1));
        expected.RemoveAt(expected.Count - 1);
        Assert.False(list.Values.RemoveFirst((value, _) => value == int.MaxValue));
        Assert.Equal(expected, list);
        Assert.Equal(expected.Count, list.Count);
        output.WriteLine($"Замена первого 200 и удаление головы/хвоста: [{string.Join(", ", list)}]; Count={list.Count}.");

        using (IEnumerator<int> iterator = list.GetEnumerator())
        {
            Assert.True(iterator.MoveNext());
            list.Values.AddLast(42);
            Assert.Throws<InvalidOperationException>(() => iterator.MoveNext());
            output.WriteLine("AddLast(42): прежний перечислитель выбросил InvalidOperationException.");
        }
        Assert.Throws<InvalidOperationException>(() => list.Values.Contains((value, _) =>
        {
            Assert.True(list.Values.ReplaceFirst((_, position) => position == 0, value));
            return true;
        }));
        Assert.Throws<NotSupportedException>(() => list.Values.UpdateFirst((_, _) => true, _ => { }));
        output.WriteLine("Запись из предиката обнаружена; UpdateFirst для int отклонён с NotSupportedException.");

        var objects = new SinglyLinkedList<StringBuilder>();
        var text = new StringBuilder("узел");
        objects.Values.AddLast(text);
        Assert.True(objects.Values.UpdateFirst((_, _) => true, item => item.Append(" обновлён")));
        Assert.Same(text, Assert.Single(objects));
        Assert.Equal("узел обновлён", text.ToString());
        output.WriteLine($"UpdateFirst для ссылочного объекта: {text}.");

        list.Values.Clear();
        Assert.Equal(0, list.Count);
        Assert.Empty(list);
        Assert.False(list.Values.TryFirst(out int missing));
        Assert.Equal(0, missing);
        list.Values.AddFirst(0);
        Assert.True(list.Values.TryFirst(out int stored));
        Assert.Equal(0, stored);
        Assert.True(list.Values.RemoveFirst((_, _) => true));
        list.Values.AddLast(42);
        Assert.Equal(42, Assert.Single(list));
        Assert.Equal(1, list.Count);
        output.WriteLine("Clear: TryFirst=false; хранимый 0: TryFirst=true; удаление единственного узла и повторное AddLast(42) прошли.");

        // Два API одной коллекции: сохранённая позиция и редактирование после неё.
        list.Values.Clear();
        SinglyLinkedList<int>.Cursor before = list.Cursors.BeforeFirst;
        before.InsertAfter(1);
        Assert.True(before.MoveNext());
        before.InsertAfter(2);
        list.Values.AddFirst(0);
        Assert.Equal(1, before.Value);
        Assert.True(list.Values.ReplaceFirst((value, _) => value == 1, 10));
        Assert.Equal(10, before.Value);
        Assert.True(before.TryRemoveAfter(out int removedAfter));
        Assert.Equal(2, removedAfter);
        before.InsertAfter(3);
        Assert.Equal([0, 10, 3], list);
        Assert.True(list.Values.Contains((value, position) => value == 3 && position == 2));
        output.WriteLine($"Values и Cursors используют одну цепочку: [{string.Join(", ", list)}]; курсор сохранился после вставки головы и замены значения через Values.");
        list.Values.Clear();
        Assert.Throws<InvalidOperationException>(() => before.MoveNext());
        output.WriteLine("Clear через Values инвалидировал курсор; обратных связей и поиска предшественника у курсора нет.");
    }
}
