using System.Collections;
using Practice.DataStructures.Core.Collections;

namespace Practice.DataStructures.Tests.Collections.DequeContract;

// Прямое отображение публичных операций; без собственного состояния дека.
internal sealed class DequeSubject<T>(
    Func<int> count,
    Func<bool> isEmpty,
    Action<T> pushFront,
    Action<T> pushBack,
    Func<T> popFront,
    Func<T> popBack,
    Func<T> peekFront,
    Func<T> peekBack,
    Action clear,
    IEnumerable<T> items) : IEnumerable<T>
{
    public int Count => count();
    public bool IsEmpty => isEmpty();
    public void PushFront(T item) => pushFront(item);
    public void PushBack(T item) => pushBack(item);
    public T PopFront() => popFront();
    public T PopBack() => popBack();
    public T PeekFront() => peekFront();
    public T PeekBack() => peekBack();
    public void Clear() => clear();
    public IEnumerator<T> GetEnumerator() => items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)items).GetEnumerator();

    internal static DequeSubject<T> CreateArray()
    {
        var deque = new ArrayDeque<T>();
        return new DequeSubject<T>(() => deque.Count, () => deque.IsEmpty,
            deque.PushFront, deque.PushBack, deque.PopFront, deque.PopBack,
            deque.PeekFront, deque.PeekBack, deque.Clear, deque);
    }

    internal static DequeSubject<T> CreateLinked()
    {
        var deque = new LinkedDeque<T>();
        return new DequeSubject<T>(() => deque.Count, () => deque.IsEmpty,
            deque.PushFront, deque.PushBack, deque.PopFront, deque.PopBack,
            deque.PeekFront, deque.PeekBack, deque.Clear, deque);
    }
}
