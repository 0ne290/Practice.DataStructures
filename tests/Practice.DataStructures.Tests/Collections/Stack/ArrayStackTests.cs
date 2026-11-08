using Practice.DataStructures.Core.Collections;

namespace Practice.DataStructures.Tests.Collections.Stack;

public sealed class ArrayStackTests : StackContractTests
{
    protected override StackSubject<T> CreateStack<T>()
    {
        var stack = new ArrayStack<T>();
        return new StackSubject<T>(
            () => stack.Count, () => stack.IsEmpty,
            stack.Push, stack.Pop, stack.Peek, stack.Clear, stack);
    }
}
