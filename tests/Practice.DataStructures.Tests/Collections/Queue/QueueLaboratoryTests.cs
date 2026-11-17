using System.Collections;
using System.Globalization;
using Practice.DataStructures.Core.Collections;
using Xunit;

namespace Practice.DataStructures.Tests.Collections.Queue;

public sealed class QueueLaboratoryTests(ITestOutputHelper output)
{
    [Fact]
    [Trait("Category", "Laboratory")]
    public void Queue_RandomData_DemonstratesAcceptedContract()
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

        output.WriteLine($"Seed={seed}; элементов={count}; диапазон=[{minimum}, {maximum}]; начальное заполнение=8; извлечение=6; маркер=42.");
        output.WriteLine($"Повтор: --environment {seedVariable}={seed}");
        var random = new Random(seed);
        int[] input = [.. Enumerable.Range(0, count).Select(_ => random.Next(minimum, maximum + 1))];
        output.WriteLine($"Вход: [{string.Join(", ", input)}]");

        // Act / Assert
        DemonstrateArrayQueue(input);
        DemonstrateLinkedQueue(input);
    }

    private void DemonstrateArrayQueue(int[] input)
    {
        output.WriteLine("ArrayQueue<T>: заполнение, частичное извлечение и дальнейшее заполнение.");
        var queue = new ArrayQueue<int>();
        Assert.True(queue.IsEmpty);
        Assert.Equal(0, queue.Count);
        for (int i = 0; i < 8; i++)
        {
            queue.Enqueue(input[i]);
            Assert.Equal(i + 1, queue.Count);
            Assert.Equal(input[0], queue.Peek());
            output.WriteLine($"Enqueue({input[i]}): Count={queue.Count}, начало={queue.Peek()}");
        }
        for (int i = 0; i < 6; i++)
        {
            int value = queue.Dequeue();
            Assert.Equal(input[i], value);
            Assert.Equal(7 - i, queue.Count);
            output.WriteLine($"Dequeue()={value}: Count={queue.Count}");
        }
        for (int i = 8; i < input.Length; i++)
        {
            queue.Enqueue(input[i]);
            Assert.Equal(i - 5, queue.Count);
            output.WriteLine($"Enqueue({input[i]}): Count={queue.Count}, начало={queue.Peek()}");
        }

        int[] remaining = [.. input.Skip(6)];
        int position = 0;
        output.WriteLine("foreach: FIFO от начала к концу без удаления.");
        foreach (int value in queue)
        {
            Assert.Equal(remaining[position++], value);
            output.WriteLine($"  {value}");
        }
        Assert.Equal(remaining.Length, position);
        Assert.Equal(remaining.Length, queue.Count);
        var untypedValues = new List<int>();
        foreach (int value in (IEnumerable)queue)
        {
            untypedValues.Add(value);
        }
        Assert.Equal(remaining, untypedValues);
        using (IEnumerator<int> enumerator = queue.GetEnumerator())
        {
            Assert.Equal(remaining[0], queue.Peek());
            Assert.True(enumerator.MoveNext());
            Assert.Equal(remaining[0], enumerator.Current);
            queue.Enqueue(42);
            Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
            Assert.Throws<InvalidOperationException>(() => enumerator.Current);
            output.WriteLine("Peek сохранил обход; Enqueue(42) инвалидировал перечислитель.");
        }
        foreach (int expected in remaining.Append(42))
        {
            int value = queue.Dequeue();
            Assert.Equal(expected, value);
            output.WriteLine($"Dequeue()={value}: Count={queue.Count}");
        }
        Assert.True(queue.IsEmpty);
        Assert.Equal(0, queue.Count);
        using (IEnumerator<int> empty = queue.GetEnumerator())
        {
            InvalidOperationException readError = Assert.Throws<InvalidOperationException>(() => queue.Peek());
            InvalidOperationException removeError = Assert.Throws<InvalidOperationException>(() => queue.Dequeue());
            Assert.False(empty.MoveNext());
            output.WriteLine($"Пустая очередь: Peek — {readError.Message}; Dequeue — {removeError.Message}; обход сохранён.");
            queue.Clear();
            Assert.Throws<InvalidOperationException>(() => empty.MoveNext());
            output.WriteLine("Clear пустой очереди инвалидировал обход.");
        }

        queue.Enqueue(42);
        using (IEnumerator<int> disposed = queue.GetEnumerator())
        {
            Assert.True(disposed.MoveNext());
            Assert.Equal(42, disposed.Current);
            disposed.Dispose();
            disposed.Dispose();
            Assert.False(disposed.MoveNext());
            Assert.Throws<InvalidOperationException>(() => disposed.Current);
            queue.Clear();
            Assert.Throws<InvalidOperationException>(() => disposed.MoveNext());
            output.WriteLine("Dispose завершил обход; Clear обнаружен даже после Dispose.");
        }
        Assert.True(queue.IsEmpty);
        foreach (int value in input)
        {
            queue.Enqueue(value);
        }
        queue.Clear();
        Assert.True(queue.IsEmpty);
        Assert.Equal(0, queue.Count);
        queue.Enqueue(42);
        Assert.Equal(42, queue.Peek());
        Assert.Equal(42, queue.Dequeue());
        Assert.True(queue.IsEmpty);
        output.WriteLine("Повторное заполнение, очистка и повторное использование: Enqueue(42), Peek()=42, Dequeue()=42; очередь пуста.");
    }

    private void DemonstrateLinkedQueue(int[] input)
    {
        output.WriteLine("LinkedQueue<T>: заполнение, частичное извлечение и дальнейшее заполнение.");
        var queue = new LinkedQueue<int>();
        Assert.True(queue.IsEmpty);
        Assert.Equal(0, queue.Count);
        for (int i = 0; i < 8; i++)
        {
            queue.Enqueue(input[i]);
            Assert.Equal(i + 1, queue.Count);
            Assert.Equal(input[0], queue.Peek());
            output.WriteLine($"Enqueue({input[i]}): Count={queue.Count}, начало={queue.Peek()}");
        }
        for (int i = 0; i < 6; i++)
        {
            int value = queue.Dequeue();
            Assert.Equal(input[i], value);
            Assert.Equal(7 - i, queue.Count);
            output.WriteLine($"Dequeue()={value}: Count={queue.Count}");
        }
        for (int i = 8; i < input.Length; i++)
        {
            queue.Enqueue(input[i]);
            Assert.Equal(i - 5, queue.Count);
            output.WriteLine($"Enqueue({input[i]}): Count={queue.Count}, начало={queue.Peek()}");
        }

        int[] remaining = [.. input.Skip(6)];
        int position = 0;
        output.WriteLine("foreach: FIFO от начала к концу без удаления.");
        foreach (int value in queue)
        {
            Assert.Equal(remaining[position++], value);
            output.WriteLine($"  {value}");
        }
        Assert.Equal(remaining.Length, position);
        Assert.Equal(remaining.Length, queue.Count);
        var untypedValues = new List<int>();
        foreach (int value in (IEnumerable)queue)
        {
            untypedValues.Add(value);
        }
        Assert.Equal(remaining, untypedValues);
        using (IEnumerator<int> enumerator = queue.GetEnumerator())
        {
            Assert.Equal(remaining[0], queue.Peek());
            Assert.True(enumerator.MoveNext());
            Assert.Equal(remaining[0], enumerator.Current);
            queue.Enqueue(42);
            Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
            Assert.Throws<InvalidOperationException>(() => enumerator.Current);
            output.WriteLine("Peek сохранил обход; Enqueue(42) инвалидировал перечислитель.");
        }
        foreach (int expected in remaining.Append(42))
        {
            int value = queue.Dequeue();
            Assert.Equal(expected, value);
            output.WriteLine($"Dequeue()={value}: Count={queue.Count}");
        }
        Assert.True(queue.IsEmpty);
        Assert.Equal(0, queue.Count);
        using (IEnumerator<int> empty = queue.GetEnumerator())
        {
            InvalidOperationException readError = Assert.Throws<InvalidOperationException>(() => queue.Peek());
            InvalidOperationException removeError = Assert.Throws<InvalidOperationException>(() => queue.Dequeue());
            Assert.False(empty.MoveNext());
            output.WriteLine($"Пустая очередь: Peek — {readError.Message}; Dequeue — {removeError.Message}; обход сохранён.");
            queue.Clear();
            Assert.Throws<InvalidOperationException>(() => empty.MoveNext());
            output.WriteLine("Clear пустой очереди инвалидировал обход.");
        }

        queue.Enqueue(42);
        using (IEnumerator<int> disposed = queue.GetEnumerator())
        {
            Assert.True(disposed.MoveNext());
            Assert.Equal(42, disposed.Current);
            disposed.Dispose();
            disposed.Dispose();
            Assert.False(disposed.MoveNext());
            Assert.Throws<InvalidOperationException>(() => disposed.Current);
            queue.Clear();
            Assert.Throws<InvalidOperationException>(() => disposed.MoveNext());
            output.WriteLine("Dispose завершил обход; Clear обнаружен даже после Dispose.");
        }
        Assert.True(queue.IsEmpty);
        foreach (int value in input)
        {
            queue.Enqueue(value);
        }
        queue.Clear();
        Assert.True(queue.IsEmpty);
        Assert.Equal(0, queue.Count);
        queue.Enqueue(42);
        Assert.Equal(42, queue.Peek());
        Assert.Equal(42, queue.Dequeue());
        Assert.True(queue.IsEmpty);
        output.WriteLine("Повторное заполнение, очистка и повторное использование: Enqueue(42), Peek()=42, Dequeue()=42; очередь пуста.");
    }
}
