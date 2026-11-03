using System.Globalization;
using Practice.DataStructures.Core.Collections;
using Xunit;

namespace Practice.DataStructures.Tests.Collections.Stack;

public sealed class StackLaboratoryTests(ITestOutputHelper output)
{
    [Fact]
    [Trait("Category", "Laboratory")]
    public void Stack_RandomData_DemonstratesAcceptedContract()
    {
        // Arrange
        const string seedVariable = "DATASTRUCTURES_LAB_SEED";
        const int count = 20;
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

        output.WriteLine($"Seed={seed}; элементов={count}; диапазон=[{minimum}, {maximum}]; повторное значение=42.");
        output.WriteLine($"Повтор: --environment {seedVariable}={seed}");
        var random = new Random(seed);
        int[] input = [.. Enumerable.Range(0, count).Select(_ => random.Next(minimum, maximum + 1))];
        output.WriteLine($"Вход: [{string.Join(", ", input)}]");
        var stack = new ArrayStack<int>();

        // Act / Assert: заполнение и чтение вершины.
        Assert.True(stack.IsEmpty);
        for (int i = 0; i < input.Length; i++)
        {
            stack.Push(input[i]);
            Assert.Equal(i + 1, stack.Count);
            Assert.Equal(input[i], stack.Peek());
            output.WriteLine($"Push({input[i]}): Count={stack.Count}, вершина={stack.Peek()}");
        }

        Assert.Equal(count, stack.Count);
        int position = input.Length - 1;
        output.WriteLine("foreach: от вершины к основанию без удаления.");
        foreach (int value in stack)
        {
            Assert.Equal(input[position--], value);
            output.WriteLine($"  {value}");
        }
        Assert.Equal(-1, position);
        Assert.Equal(count, stack.Count);
        using (IEnumerator<int> enumerator = stack.GetEnumerator())
        {
            Assert.True(enumerator.MoveNext());
            stack.Push(42);
            Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
            Assert.Equal(42, stack.Pop());
            output.WriteLine("Push во время обхода: перечислитель обнаружил изменение.");
        }
        output.WriteLine($"Peek()={stack.Peek()}; Count={stack.Count}: чтение сохраняет стек.");
        for (int i = input.Length - 1; i >= 0; i--)
        {
            int value = stack.Pop();
            Assert.Equal(input[i], value);
            Assert.Equal(i, stack.Count);
            output.WriteLine($"Pop()={value}: Count={stack.Count}");
        }

        Assert.True(stack.IsEmpty);
        InvalidOperationException popError = Assert.Throws<InvalidOperationException>(() => stack.Pop());
        InvalidOperationException peekError = Assert.Throws<InvalidOperationException>(() => stack.Peek());
        output.WriteLine($"Пустой стек: Pop — {popError.Message}; Peek — {peekError.Message}");
        Assert.Equal(0, stack.Count);

        foreach (int value in input)
        {
            stack.Push(value);
        }
        stack.Clear();
        Assert.True(stack.IsEmpty);
        Assert.Equal(0, stack.Count);
        stack.Clear();
        stack.Push(42);
        Assert.Equal(42, stack.Peek());
        Assert.Equal(42, stack.Pop());
        Assert.True(stack.IsEmpty);
        output.WriteLine("Повторное заполнение, очистка и повторное использование: Push(42), Peek()=42, Pop()=42; стек пуст.");
    }
}
