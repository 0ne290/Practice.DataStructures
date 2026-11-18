using System.Collections;
using System.Globalization;
using Practice.DataStructures.Core.Collections;
using Xunit;

namespace Practice.DataStructures.Tests.Collections.Deque;

public sealed class DequeLaboratoryTests(ITestOutputHelper output)
{
    [Fact]
    [Trait("Category", "Laboratory")]
    public void ArrayDeque_RandomData_DemonstratesAcceptedContract()
    {
        // Arrange
        const string seedVariable = "DATASTRUCTURES_LAB_SEED";
        const int count = 24;
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
        output.WriteLine($"Seed={seed}; элементов={count}; диапазон=[{minimum}, {maximum}]; заполнение=8; извлечение=6; маркер=42.");
        output.WriteLine($"Повтор: --environment {seedVariable}={seed}");
        var random = new Random(seed);
        int[] input = [.. Enumerable.Range(0, count).Select(_ => random.Next(minimum, maximum + 1))];
        output.WriteLine($"Вход: [{string.Join(", ", input)}]");
        var deque = new ArrayDeque<int>();
        var expected = new LinkedList<int>();

        void Show(string operation)
        {
            Assert.Equal(expected.Count, deque.Count);
            Assert.Equal(expected.Count == 0, deque.IsEmpty);
            Assert.Equal(expected.ToArray(), deque.ToArray());
            output.WriteLine($"{operation}: Count={deque.Count}; [{string.Join(", ", deque)}]");
        }

        // Act / Assert
        Show("Пустой дек");
        output.WriteLine("Сдвигаем начало последовательности; затем пересекаем границу массива и расширяем хранилище.");
        for (int i = 0; i < 8; i++)
        {
            deque.PushBack(input[i]);
            expected.AddLast(input[i]);
            Show($"PushBack({input[i]})");
        }
        for (int i = 0; i < 6; i++)
        {
            int value = deque.PopFront();
            Assert.Equal(expected.First?.Value, value);
            expected.RemoveFirst();
            Show($"PopFront() = {value}");
        }
        for (int i = 8; i < count; i++)
        {
            deque.PushBack(input[i]);
            expected.AddLast(input[i]);
            Show($"PushBack({input[i]})");
        }
        deque.PushFront(42);
        expected.AddFirst(42);
        Show("PushFront(42)");
        int last = deque.PopBack();
        Assert.Equal(expected.Last?.Value, last);
        expected.RemoveLast();
        Show($"PopBack() = {last}");

        using (IEnumerator<int> enumerator = deque.GetEnumerator())
        {
            Assert.Equal(expected.First?.Value, deque.PeekFront());
            Assert.Equal(expected.Last?.Value, deque.PeekBack());
            Show($"Чтение без удаления: начало={deque.PeekFront()}, конец={deque.PeekBack()}");
            Assert.True(enumerator.MoveNext());
            Assert.Equal(42, enumerator.Current);
            int index = 0;
            int[] snapshot = expected.ToArray();
            foreach (object? item in (IEnumerable)deque)
            {
                Assert.Equal(snapshot[index++], Assert.IsType<int>(item));
            }
            Assert.Equal(snapshot.Length, index);
            index = 0;
            foreach (int item in deque)
            {
                Assert.Equal(snapshot[index], item);
                output.WriteLine($"foreach [{index++}] = {item}");
            }
            Assert.Equal(snapshot.Length, index);
            deque.PushBack(42);
            expected.AddLast(42);
            Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
            Assert.Throws<InvalidOperationException>(() => enumerator.Current);
            output.WriteLine("Изменение дека инвалидировало начатый обход: получено InvalidOperationException.");
        }
        while (!deque.IsEmpty)
        {
            if (deque.Count % 2 == 0)
            {
                Assert.Equal(expected.First?.Value, deque.PopFront());
                expected.RemoveFirst();
                Show("Извлечение из начала");
            }
            else
            {
                Assert.Equal(expected.Last?.Value, deque.PopBack());
                expected.RemoveLast();
                Show("Извлечение с конца");
            }
        }
        Assert.Throws<InvalidOperationException>(() => deque.PopFront());
        Assert.Throws<InvalidOperationException>(() => deque.PopBack());
        Assert.Throws<InvalidOperationException>(() => deque.PeekFront());
        Assert.Throws<InvalidOperationException>(() => deque.PeekBack());
        output.WriteLine("Пустые PopFront, PopBack, PeekFront и PeekBack: получено InvalidOperationException.");

        deque.PushFront(42);
        expected.AddFirst(42);
        using (IEnumerator<int> enumerator = deque.GetEnumerator())
        {
            Assert.True(enumerator.MoveNext());
            enumerator.Dispose();
            enumerator.Dispose();
            Assert.False(enumerator.MoveNext());
            Assert.Throws<InvalidOperationException>(() => enumerator.Current);
            Assert.Throws<NotSupportedException>(() => enumerator.Reset());
            output.WriteLine("Dispose завершил обход, повторный Dispose безопасен; Reset не поддерживается.");
        }
        deque.Clear();
        expected.Clear();
        Show("Clear непустого дека");
        using (IEnumerator<int> enumerator = deque.GetEnumerator())
        {
            deque.Clear();
            Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
            output.WriteLine("Clear пустого дека тоже инвалидировал перечислитель.");
        }
        deque.PushBack(input[0]);
        expected.AddLast(input[0]);
        deque.PushFront(42);
        expected.AddFirst(42);
        Show("Повторное использование после очистки");
    }
}
