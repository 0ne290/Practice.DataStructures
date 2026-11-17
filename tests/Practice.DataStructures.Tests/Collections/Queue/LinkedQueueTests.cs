using Practice.DataStructures.Core.Collections;

namespace Practice.DataStructures.Tests.Collections.Queue;

public sealed class LinkedQueueTests : QueueContractTests
{
    protected override QueueSubject<T> CreateQueue<T>()
    {
        var queue = new LinkedQueue<T>();
        return new QueueSubject<T>(
            () => queue.Count, () => queue.IsEmpty,
            queue.Enqueue, queue.Dequeue, queue.Peek, queue.Clear, queue);
    }
}
