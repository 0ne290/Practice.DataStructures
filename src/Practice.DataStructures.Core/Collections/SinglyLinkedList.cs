using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Practice.DataStructures.Core.Collections;

/// <summary>Односвязный список на собственных узлах с выбором элементов по условию.</summary>
/// <typeparam name="T">Тип элементов. Допускаются повторы, значения по умолчанию и null.</typeparam>
/// <remarks>
/// Условие получает элемент и его позицию от нуля. Поиск идёт от головы к хвосту
/// и прекращается на первом совпадении; каждая посещённая позиция проверяется один раз.
/// После нормально завершившегося вызова условия проверяется версия списка.
/// Изменения из условия обнаруживаются, но не откатываются; исключения делегата
/// передаются вызывающему коду. Чтение и вложенный поиск разрешены.
/// Делегаты должны быть ненулевыми; проверка этого предусловия не выполняется.
/// Изменение самого объекта через внешнюю ссылку версию списка не меняет.
/// Коллекция не обеспечивает синхронизацию доступа из нескольких потоков.
/// </remarks>
public sealed class SinglyLinkedList<T> : IEnumerable<T>
{
    /// <summary>Получает число элементов за O(1).</summary>
    public int Count { get; private set; }

    private int _version;

    private Node? _head;
    private Node? _tail;

    /// <summary>Создаёт пустой список.</summary>
    public SinglyLinkedList()
    {
        _head = null;
        _tail = null;
        Count = 0;
        _version = 0;
    }

    /// <summary>Добавляет элемент в начало за O(1) и инвалидирует перечислители.</summary>
    /// <param name="item">Добавляемое значение.</param>
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
        _version = unchecked(_version + 1);
    }

    /// <summary>Добавляет элемент в конец за O(1) и инвалидирует перечислители.</summary>
    /// <param name="item">Добавляемое значение.</param>
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
        _version = unchecked(_version + 1);
    }

    /// <summary>Вставляет значение после первого совпадения за O(n).</summary>
    /// <param name="indexedPredicate">Ненулевое условие выбора элемента и позиции.</param>
    /// <param name="item">Добавляемое значение.</param>
    /// <returns>True при вставке; false при отсутствии совпадения.</returns>
    /// <remarks>Успешная вставка инвалидирует перечислители; отсутствие совпадения сохраняет их.</remarks>
    /// <exception cref="InvalidOperationException">Условие изменило список и вернуло результат.</exception>
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
        _version = unchecked(_version + 1);

        return true;
    }

    /// <summary>Вставляет значение перед первым совпадением за O(n).</summary>
    /// <param name="indexedPredicate">Ненулевое условие выбора элемента и позиции.</param>
    /// <param name="item">Добавляемое значение.</param>
    /// <returns>True при вставке; false при отсутствии совпадения.</returns>
    /// <remarks>Успешная вставка инвалидирует перечислители; отсутствие совпадения сохраняет их.</remarks>
    /// <exception cref="InvalidOperationException">Условие изменило список и вернуло результат.</exception>
    public bool AddBefore(IndexedPredicate<T> indexedPredicate, T item)
    {
        var (previousNode, targetNode, _) = _find(indexedPredicate);
        if (targetNode is null)
            return false;

        if (targetNode == _head)
        {
            var newHead = new Node { Value = item, Next = _head };
            _head = newHead;
        }
        else
        {
            var newNode = new Node { Value = item, Next = targetNode };
            previousNode!.Next = newNode;
        }

        Count++;
        _version = unchecked(_version + 1);

        return true;
    }

    private (Node? previousNode, Node? targetNode, int depth) _find(IndexedPredicate<T> indexedPredicate)
    {
        if (Count == 0)
            return (null, null, -1);

        var versionSnapshot = _version;

        Node? previousNode = null;
        var currentNode = _head;
        var depth = 0;
        while (currentNode is not null)
        {
            var matches = indexedPredicate(currentNode.Value, depth);
            if (versionSnapshot != _version)
                throw new InvalidOperationException("Collection was modified; operation cannot be continued.");

            if (matches)
                return (previousNode, currentNode, depth);

            previousNode = currentNode;
            currentNode = currentNode.Next;
            depth++;
        }

        return (null, null, -1);
    }

    /// <summary>Пытается прочитать голову за O(1), сохраняя состояние списка и перечислителей.</summary>
    /// <param name="value">Значение головы при успехе; default при пустом списке.</param>
    /// <returns>True, если элемент существует, включая хранимый null или default.</returns>
    public bool TryFirst([MaybeNullWhen(false)] out T value)
    {
        value = _head is null ? default : _head.Value;
        return _head is not null;
    }

    /// <summary>Пытается прочитать хвост за O(1), сохраняя состояние списка и перечислителей.</summary>
    /// <param name="value">Значение хвоста при успехе; default при пустом списке.</param>
    /// <returns>True, если элемент существует, включая хранимый null или default.</returns>
    public bool TryLast([MaybeNullWhen(false)] out T value)
    {
        value = _tail is null ? default : _tail.Value;
        return _tail is not null;
    }

    /// <summary>Пытается прочитать первое совпадение за O(n).</summary>
    /// <param name="indexedPredicate">Ненулевое условие выбора элемента и позиции.</param>
    /// <param name="value">Найденное значение, включая null или default; default при отсутствии.</param>
    /// <returns>True, если совпадение найдено; иначе false.</returns>
    /// <exception cref="InvalidOperationException">Условие изменило список и вернуло результат.</exception>
    public bool TryFindFirst(IndexedPredicate<T> indexedPredicate, [MaybeNullWhen(false)] out T value)
    {
        var (_, targetNode, _) = _find(indexedPredicate);

        value = targetNode is null ? default : targetNode.Value;
        return targetNode is not null;
    }

    /// <summary>Заменяет первое совпадение за O(n).</summary>
    /// <param name="indexedPredicate">Ненулевое условие выбора элемента и позиции.</param>
    /// <param name="newItem">Новое значение.</param>
    /// <returns>True при замене; false при отсутствии совпадения.</returns>
    /// <remarks>Успех инвалидирует перечислители даже при записи того же значения.</remarks>
    /// <exception cref="InvalidOperationException">Условие изменило список и вернуло результат.</exception>
    public bool ReplaceFirst(IndexedPredicate<T> indexedPredicate, T newItem)
    {
        var (_, targetNode, _) = _find(indexedPredicate);
        if (targetNode is null)
            return false;

        targetNode.Value = newItem;
        _version = unchecked(_version + 1);
        return true;
    }

    /// <summary>Передаёт объект первого совпадения действию для изменения его состояния за O(n).</summary>
    /// <param name="indexedPredicate">Ненулевое условие выбора элемента и позиции.</param>
    /// <param name="update">Ненулевое действие; получает хранимую ссылку, включая null.</param>
    /// <returns>True после успешного действия; false при отсутствии совпадения.</returns>
    /// <remarks>
    /// Ссылку в узле не заменяет. Успех инвалидирует перечислители даже при пустом действии.
    /// Изменения списка из действия обнаруживаются после его нормального завершения.
    /// При исключении действия версия этим методом не увеличивается; уже выполненные
    /// изменения объекта не откатываются. Сложность указана без стоимости делегатов.
    /// </remarks>
    /// <exception cref="NotSupportedException">T — значимый тип, включая Nullable, даже если список пуст.</exception>
    /// <exception cref="InvalidOperationException">Условие или действие изменило список и нормально завершилось.</exception>
    public bool UpdateFirst(IndexedPredicate<T> indexedPredicate, Action<T> update)
    {
        if (typeof(T).IsValueType)
            throw new NotSupportedException(
                "UpdateFirst only supports reference element types.");

        var (_, targetNode, _) = _find(indexedPredicate);
        if (targetNode is null)
            return false;

        var versionSnapshot = _version;
        update(targetNode.Value);
        if (versionSnapshot != _version)
            throw new InvalidOperationException("Collection was modified; operation cannot be continued.");

        _version = unchecked(_version + 1);
        return true;
    }

    /// <summary>Удаляет первое совпадение, сохраняя порядок остальных элементов, за O(n).</summary>
    /// <param name="indexedPredicate">Ненулевое условие выбора элемента и позиции.</param>
    /// <returns>True при удалении; false при отсутствии совпадения.</returns>
    /// <remarks>Успех инвалидирует перечислители; отсутствие совпадения сохраняет их.</remarks>
    /// <exception cref="InvalidOperationException">Условие изменило список и вернуло результат.</exception>
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
        _version = unchecked(_version + 1);

        return true;
    }

    /// <summary>Находит позицию первого совпадения за O(n).</summary>
    /// <param name="indexedPredicate">Ненулевое условие выбора элемента и позиции.</param>
    /// <returns>Позиция от нуля либо -1 при отсутствии совпадения.</returns>
    /// <exception cref="InvalidOperationException">Условие изменило список и вернуло результат.</exception>
    public int PositionOf(IndexedPredicate<T> indexedPredicate)
    {
        var (_, _, depth) = _find(indexedPredicate);

        return depth;
    }

    /// <summary>Проверяет наличие совпадения за O(n).</summary>
    /// <param name="indexedPredicate">Ненулевое условие выбора элемента и позиции.</param>
    /// <returns>True при наличии совпадения; иначе false.</returns>
    /// <exception cref="InvalidOperationException">Условие изменило список и вернуло результат.</exception>
    public bool Contains(IndexedPredicate<T> indexedPredicate)
    {
        var (_, targetNode, _) = _find(indexedPredicate);

        return targetNode is not null;
    }

    /// <summary>Отсоединяет все узлы за O(1), обнуляет Count и инвалидирует перечислители даже у пустого списка.</summary>
    /// <remarks>Старый перечислитель может удерживать узлы до освобождения ссылки на него.</remarks>
    public void Clear()
    {
        _head = null;
        _tail = null;
        Count = 0;
        _version = unchecked(_version + 1);
    }

    private sealed class Node
    {
        public required T Value { get; set; }

        public required Node? Next { get; set; }
    }

    /// <summary>Создаёт независимый перечислитель от головы к хвосту за O(1).</summary>
    /// <returns>Перечислитель перед первым элементом; полный обход занимает O(n).</returns>
    /// <remarks>
    /// Версия фиксируется при создании. Успешные вставка, удаление, замена и обновление,
    /// а также каждый Clear инвалидируют старые перечислители: Current и MoveNext
    /// выбрасывают InvalidOperationException до первого шага и после завершения тоже.
    /// Current до первого шага и после конца выбрасывает InvalidOperationException.
    /// После конца MoveNext продолжает возвращать false, если версия не менялась.
    /// Reset не поддерживается. Dispose ничего не делает, продолжение обхода допустимо.
    /// </remarks>
    public IEnumerator<T> GetEnumerator()
    {
        return new Enumerator(this);
    }

    /// <inheritdoc cref="GetEnumerator"/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private sealed class Enumerator : IEnumerator<T>
    {
        private readonly SinglyLinkedList<T> _owner;
        private readonly int _expectedVersion;
        private Node? _currentNode;
        private bool _started;

        internal Enumerator(SinglyLinkedList<T> owner)
        {
            _owner = owner;
            _expectedVersion = owner._version;
            _currentNode = null;
        }

        public T Current
        {
            get
            {
                ValidateVersion();
                return _currentNode is null
                    ? throw new InvalidOperationException("Enumeration has not started or has already finished.")
                    : _currentNode.Value;
            }
        }

        object? IEnumerator.Current => Current;

        public bool MoveNext()
        {
            ValidateVersion();

            _currentNode = _started ? _currentNode?.Next : _owner._head;
            _started = true;

            return _currentNode != null;
        }

        public void Dispose()
        {
            // Перечислитель не владеет ресурсами, требующими освобождения.
        }

        void IEnumerator.Reset()
        {
            throw new NotSupportedException("Reset is not supported. Obtain a new enumerator instead.");
        }

        private void ValidateVersion()
        {
            if (_expectedVersion != _owner._version)
                throw new InvalidOperationException("Collection was modified; enumeration cannot continue.");
        }
    }
}
