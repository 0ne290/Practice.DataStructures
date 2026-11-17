using Practice.DataStructures.Core.Collections;

namespace Practice.DataStructures.Tests.Collections.Queue;

public sealed class ArrayQueueTests : QueueContractTests
{
    protected override QueueSubject<T> CreateQueue<T>()
    {
        var queue = new ArrayQueue<T>();
        return new QueueSubject<T>(
            () => queue.Count, () => queue.IsEmpty,
            queue.Enqueue, queue.Dequeue, queue.Peek, queue.Clear, queue);
    }
}
