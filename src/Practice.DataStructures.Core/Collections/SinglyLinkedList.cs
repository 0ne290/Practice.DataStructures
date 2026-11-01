namespace Practice.DataStructures.Core.Collections;

/*
 * Реализовать обобщённый односвязный список SinglyLinkedList<T> на собственных узлах. Узлы должны оставаться внутренней реализацией.
 * Список должен предоставлять:
 * - добавление в начало и конец;
 * - чтение, изменение и удаление элемента по условию;
 * - поиск позиции и проверку наличия по условию;
 * - очистку, последовательный перебор и количество элементов.
 * Условию доступны элемент и его текущая позиция. Конкретные сигнатуры и поведение операций выбираешь самостоятельно.
 */
public sealed class SinglyLinkedList<T>
{
    public int Count { get; private set; }

    private Node? _head;
    public T? First => _head is null ? default : _head.Value;
    private Node? _tail;
    public T? Last => _tail is null ? default : _tail.Value;

    public SinglyLinkedList()
    {
        _head = null;
        _tail = null;
        Count = 0;
    }

    public void AddFirst(T item)
    {
        if (Count == 0)
        {
            var firstNode = new Node { Value = item, Next = null };
            _tail = firstNode;
            _head = firstNode;
        }
        else
        {
            var newHead = new Node { Value = item, Next = _head };
            _head = newHead;
        }

        Count++;
    }

    public void AddLast(T item)
    {
        if (Count == 0)
        {
            var firstNode = new Node { Value = item, Next = null };
            _tail = firstNode;
            _head = firstNode;
        }
        else
        {
            var newTail = new Node { Value = item, Next = null };
            _tail!.Next = newTail;
            _tail = newTail;
        }

        Count++;
    }

    public bool AddAfter(IndexedPredicate<T> indexedPredicate, T item)
    {
        var (_, targetNode, _) = _find(indexedPredicate);
        if (targetNode is null)
            return false;

        if (targetNode == _tail)
        {
            var newTail = new Node { Value = item, Next = null };
            _tail.Next = newTail;
            _tail = newTail;
        }
        else
        {
            var newNode = new Node { Value = item, Next = targetNode.Next };
            targetNode.Next = newNode;
        }

        Count++;

        return true;
    }

    public bool AddBefore(IndexedPredicate<T> indexedPredicate, T item)
    {
        var (_, targetNode, _) = _find(indexedPredicate);
        if (targetNode is null)
            return false;

        if (targetNode == _head)
        {
            var newHead = new Node { Value = item, Next = _head };
            _head = newHead;
        }
        else
        {
            var newNode = new Node { Value = item, Next = targetNode.Next };
            targetNode.Next = newNode;
        }

        Count++;

        return true;
    }

    private (Node? previousNode, Node? targetNode, int depth) _find(IndexedPredicate<T> indexedPredicate)
    {
        if (Count == 0)
            return (null,  null, -1);

        if (indexedPredicate(_head!.Value, 0))
            return (null, _head, 0);
        if (indexedPredicate(_tail!.Value, Count - 1))
            return (null, _tail, Count - 1);
        if (Count == 1)
            return (null,  null, -1);

        var previousNode = _head;
        var currentNode = _head.Next;
        var depth = 1;
        while (currentNode != _tail)
        {
            if (indexedPredicate(currentNode!.Value, depth))
                return (previousNode, currentNode, depth);

            previousNode = currentNode;
            currentNode = currentNode.Next;
            depth++;
        }

        return (null,  null, -1);
    }

    public T? FindFirst(IndexedPredicate<T> indexedPredicate)
    {
        var (_, targetNode, _) = _find(indexedPredicate);

        return targetNode is null ? default : targetNode.Value;
    }

    public bool ReplaceFirst(IndexedPredicate<T> indexedPredicate, T newItem)
    {
        var (_, targetNode, _) = _find(indexedPredicate);
        if (targetNode is null)
            return false;

        targetNode.Value = newItem;
        return true;
    }

    public bool UpdateFirst(IndexedPredicate<T> indexedPredicate, Action<T> update)
    {
        var (_, targetNode, _) = _find(indexedPredicate);
        if (targetNode is null)
            return false;

        update(targetNode.Value);
        return true;
    }

    public bool RemoveFirst(IndexedPredicate<T> indexedPredicate)
    {
        var (previousNode, targetNode, _) = _find(indexedPredicate);
        if (targetNode is null)
            return false;

        if (Count == 1)
        {
            _head = null;
            _tail = null;
        }
        else if (targetNode == _head)
            _head = targetNode.Next;
        else if (targetNode == _tail)
        {
            _tail = previousNode!;
            _tail.Next = null;
        }
        else
            previousNode!.Next = targetNode.Next;

        Count--;

        return true;
    }

    public int PositionOf(IndexedPredicate<T> indexedPredicate)
    {
        var (_, _, depth) = _find(indexedPredicate);

        return depth;
    }

    public bool Contains(IndexedPredicate<T> indexedPredicate)
    {
        var (_, targetNode, _) = _find(indexedPredicate);

        return targetNode is not null;
    }

    private sealed class Node
    {
        public required T Value { get; set; }

        public required Node? Next { get; set; }
    }
}
