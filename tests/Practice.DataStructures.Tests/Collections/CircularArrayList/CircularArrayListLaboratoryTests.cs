using System.Globalization;
using Practice.DataStructures.Core.Collections;
using Xunit;

namespace Practice.DataStructures.Tests.Collections.CircularArrayList;

public sealed class CircularArrayListLaboratoryTests(ITestOutputHelper output)
{
    [Fact]
    [Trait("Category", "Laboratory")]
    public void CircularArrayList_RandomData_DemonstratesAcceptedContract()
    {
        // Arrange
        const string seedVariable = "DATASTRUCTURES_LAB_SEED";
        const int capacity = 4;
        const int rotations = 3;
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

        output.WriteLine($"Seed={seed}; Capacity={capacity}; смещений начала={rotations}; значения=[-9, 9].");
        output.WriteLine($"Повтор: --environment {seedVariable}={seed}");
        var random = new Random(seed);
        var list = new CircularArrayList<int>(capacity);
        var expected = new List<int>();

        void Show(string operation)
        {
            Assert.Equal(expected.Count, list.Count);
            Assert.Equal(expected, list);
            if (expected.Count > 0)
            {
                Assert.Equal(expected[0], list.First);
                Assert.Equal(expected[^1], list.Last);
            }

            output.WriteLine($"{operation}: [{string.Join(", ", list)}]; Count={list.Count}, Capacity={list.Capacity}");
        }

        // Act / Assert: заполнение, продвижение начала и переход через границу массива.
        for (int step = 0; step < capacity; step++)
        {
            int value = random.Next(-9, 10);
            list.Add(value);
            expected.Add(value);
            Show($"Add({value})");
        }

        for (int step = 0; step < rotations; step++)
        {
            list.RemoveAt(0);
            expected.RemoveAt(0);
            Show("RemoveAt(0)");
            int value = random.Next(-9, 10);
            list.Add(value);
            expected.Add(value);
            Assert.Equal(capacity, list.Capacity);
            Show($"Add({value}) без роста");
        }

        // Act / Assert: рост при вставке, затем вставки в середину и конец.
        foreach (int position in new[] { 0, 2, 6 })
        {
            int value = random.Next(-9, 10);
            list.Insert(position, value);
            expected.Insert(position, value);
            Show($"Insert({position}, {value})");
        }

        list[1] = 100;
        expected[1] = 100;
        Show("Замена [1] = 100");
        Assert.Equal(1, list.IndexOf((value, position) => value == 100 && position == 1));
        Assert.True(list.Contains((_, position) => position == 2));
        output.WriteLine($"First={list.First}, Last={list.Last}; поиск по значению и позиции проверен.");
        var traversed = new List<int>();
        foreach (int value in list)
        {
            traversed.Add(value);
        }

        Assert.Equal(expected, traversed);
        output.WriteLine($"foreach: [{string.Join(", ", traversed)}]");
        using (IEnumerator<int> iterator = list.GetEnumerator())
        {
            list.RemoveAt(list.Count - 1);
            expected.RemoveAt(expected.Count - 1);
            Assert.Throws<InvalidOperationException>(() => iterator.MoveNext());
            Show("Удаление с конца инвалидировало прежний обход");
        }

        using (IEnumerator<int> iterator = list.GetEnumerator())
        {
            Assert.Throws<ArgumentOutOfRangeException>("index", () => list.Insert(list.Count + 1, 0));
            Assert.True(iterator.MoveNext());
            Assert.Equal(expected[0], iterator.Current);
            output.WriteLine("Insert(Count + 1, 0) отклонён; прежний обход сохранился.");
        }

        int retainedCapacity = list.Capacity;
        list.Clear();
        expected.Clear();
        Assert.Equal(retainedCapacity, list.Capacity);
        Assert.Throws<InvalidOperationException>(() => list.First);
        Assert.Throws<InvalidOperationException>(() => list.Last);
        Show("Clear; чтение концов пустого списка отклонено");
        list.Insert(0, 42);
        expected.Add(42);
        Show("Повторное использование: Insert(0, 42)");
    }
}
