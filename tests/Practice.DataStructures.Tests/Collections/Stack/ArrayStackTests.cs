using Practice.DataStructures.Core.Collections;
using Xunit;

namespace Practice.DataStructures.Tests.Collections.Stack;

public sealed class ArrayStackTests
{
    [Fact]
    public void Constructor_NewStack_IsEmpty()
    {
        // Arrange / Act
        var stack = new ArrayStack<int>();

        // Assert
        Assert.Equal(0, stack.Count);
        Assert.True(stack.IsEmpty);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(129)]
    public void PushPop_MultipleElements_PreservesLifoAcrossGrowth(int count)
    {
        // Arrange
        var stack = new ArrayStack<int>();

        // Act
        for (int i = 0; i < count; i++)
        {
            stack.Push(i);
        }

        // Assert
        Assert.Equal(count, stack.Count);
        Assert.False(stack.IsEmpty);
        for (int i = count - 1; i >= 0; i--)
        {
            Assert.Equal(i, stack.Pop());
            Assert.Equal(i, stack.Count);
            Assert.Equal(i == 0, stack.IsEmpty);
        }
    }

    [Fact]
    public void Peek_NonEmptyStack_PreservesTopAndCount()
    {
        // Arrange
        var stack = new ArrayStack<int>();
        stack.Push(10);
        stack.Push(20);

        // Act
        int first = stack.Peek();
        int second = stack.Peek();

        // Assert
        Assert.Equal(20, first);
        Assert.Equal(first, second);
        Assert.Equal(2, stack.Count);
        Assert.Equal(20, stack.Pop());
        Assert.Equal(10, stack.Pop());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PopPeek_EmptyStack_ThrowsAndRemainsReusable(bool drainFirst)
    {
        // Arrange
        var stack = new ArrayStack<int>();
        if (drainFirst)
        {
            stack.Push(42);
            stack.Pop();
        }

        // Act
        Assert.Throws<InvalidOperationException>(() => stack.Pop());
        Assert.Throws<InvalidOperationException>(() => stack.Peek());

        // Assert
        Assert.Equal(0, stack.Count);
        Assert.True(stack.IsEmpty);
        stack.Push(17);
        Assert.Equal(17, stack.Peek());
        Assert.Equal(17, stack.Pop());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(40)]
    public void Clear_AnyCount_EmptiesStackAndAllowsReuse(int count)
    {
        // Arrange
        var stack = new ArrayStack<int>();
        for (int i = 0; i < count; i++)
        {
            stack.Push(i);
        }

        // Act
        stack.Clear();
        stack.Clear();

        // Assert
        Assert.Equal(0, stack.Count);
        Assert.True(stack.IsEmpty);
        Assert.Throws<InvalidOperationException>(() => stack.Peek());
        Assert.Throws<InvalidOperationException>(() => stack.Pop());
        stack.Push(100);
        stack.Push(200);
        Assert.Equal(200, stack.Pop());
        Assert.Equal(100, stack.Pop());
        Assert.True(stack.IsEmpty);
    }

    [Fact]
    public void PushPop_NullDuplicatesAndReferences_PreservesValuesAndIdentity()
    {
        // Arrange
        var stack = new ArrayStack<object?>();
        object value = new();

        // Act
        stack.Push(null);
        stack.Push(value);
        stack.Push(value);

        // Assert
        Assert.Equal(3, stack.Count);
        Assert.Same(value, stack.Peek());
        Assert.Same(value, stack.Pop());
        Assert.Same(value, stack.Pop());
        Assert.Null(stack.Peek());
        Assert.Null(stack.Pop());
        Assert.True(stack.IsEmpty);
    }

    [Theory]
    [InlineData(12345)]
    [InlineData(9147)]
    public void MixedOperations_FixedSeed_MatchesReferenceStack(int seed)
    {
        // Arrange
        var stack = new ArrayStack<int>();
        var expected = new Stack<int>();
        var random = new Random(seed);

        // Act / Assert: compare after each operation.
        for (int i = 0; i < 1000; i++)
        {
            switch (random.Next(10))
            {
                case < 5:
                    int value = random.Next(-10, 11);
                    stack.Push(value);
                    expected.Push(value);
                    break;
                case < 8:
                    if (expected.Count == 0)
                    {
                        Assert.Throws<InvalidOperationException>(() => stack.Pop());
                    }
                    else
                    {
                        Assert.Equal(expected.Pop(), stack.Pop());
                    }
                    break;
                case 8:
                    stack.Clear();
                    expected.Clear();
                    break;
                default:
                    if (expected.Count == 0)
                    {
                        Assert.Throws<InvalidOperationException>(() => stack.Peek());
                    }
                    else
                    {
                        Assert.Equal(expected.Peek(), stack.Peek());
                    }
                    break;
            }

            Assert.Equal(expected.Count, stack.Count);
            Assert.Equal(expected.Count == 0, stack.IsEmpty);
            if (expected.Count > 0)
            {
                Assert.Equal(expected.Peek(), stack.Peek());
            }
        }

        while (expected.Count > 0)
        {
            Assert.Equal(expected.Pop(), stack.Pop());
        }
        Assert.True(stack.IsEmpty);
    }
}
