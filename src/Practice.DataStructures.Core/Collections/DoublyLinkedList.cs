using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Practice.DataStructures.Core.Collections;

/// <summary>Двусвязный список с обходом в обе стороны и локальными операциями через курсор.</summary>
/// <typeparam name="T">Тип элементов; допускаются повторы, null и значения по умолчанию.</typeparam>
/// <remarks>
/// Узлы и связи скрыты. Курсор обозначает элемент, а не индекс: изменения других
/// элементов сохраняют его. Удаление выбранного элемента и Clear делают курсор
/// недействительным. Перечислители обнаруживают любое изменение списка, включая
/// замену значения и очистку пустого списка. Изменение объекта через внешнюю ссылку
/// версию не меняет. Коллекция не обеспечивает синхронизацию потоков.
/// </remarks>
public sealed class DoublyLinkedList<T> : IEnumerable<T>
{
    private Node? _head;
    private Node? _tail;
    private int _version;
    private object _generation = new();

    /// <summary>Число элементов, O(1).</summary>
    public int Count { get; private set; }

    /// <summary>Новый независимый курсор на первый элемент либо null, O(1).</summary>
    public Cursor? First => _head is null ? null : new Cursor(this, _head);

    /// <summary>Новый независимый курсор на последний элемент либо null, O(1).</summary>
    public Cursor? Last => _tail is null ? null : new Cursor(this, _tail);

    /// <summary>Добавляет значение в начало за O(1).</summary>
    public void AddFirst(T item)
    {
        InsertBetween(null, _head, item);
    }

    /// <summary>Добавляет значение в конец за O(1).</summary>
    public void AddLast(T item)
    {
        InsertBetween(_tail, null, item);
    }

    /// <summary>Удаляет первый элемент за O(1); возвращает false и default для пустого списка.</summary>
    public bool TryRemoveFirst([MaybeNullWhen(false)] out T value)
    {
        return TryRemove(_head, out value);
    }

    /// <summary>Удаляет последний элемент за O(1); возвращает false и default для пустого списка.</summary>
    public bool TryRemoveLast([MaybeNullWhen(false)] out T value)
    {
        return TryRemove(_tail, out value);
    }

    /// <summary>Находит первое совпадение от головы к хвосту, O(n).</summary>
    /// <param name="predicate">Ненулевое условие; получает значение и позицию от головы, начиная с нуля.</param>
    /// <returns>Новый курсор на совпадение либо null.</returns>
    /// <remarks>
    /// Каждая посещённая позиция проверяется один раз. После нормального завершения
    /// условия проверяется версия. Изменения из условия не откатываются; исключения
    /// передаются вызывающему коду. Чтение и вложенный поиск разрешены.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Условие изменило список и вернуло результат.</exception>
    public Cursor? FindFirst(IndexedPredicate<T> predicate)
    {
        return Find(predicate, backwards: false);
    }

    /// <summary>Находит последнее совпадение от хвоста к голове, O(n).</summary>
    /// <param name="predicate">Ненулевое условие; позиции отсчитываются от головы: Count − 1, …, 0.</param>
    /// <returns>Новый курсор на совпадение либо null.</returns>
    /// <remarks>Правила вызова условия и обнаружения изменений совпадают с <see cref="FindFirst"/>.</remarks>
    public Cursor? FindLast(IndexedPredicate<T> predicate)
    {
        return Find(predicate, backwards: true);
    }

    /// <summary>Создаёт курсор на позицию от нуля, двигаясь с ближайшего конца.</summary>
    /// <remarks>Время O(min(index, Count − 1 − index)), память O(1).</remarks>
    /// <exception cref="ArgumentOutOfRangeException">Позиция вне списка.</exception>
    public Cursor GetCursorAt(int index)
    {
        if ((uint)index >= (uint)Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        Node node;
        if (index <= (Count - 1) / 2)
        {
            node = _head!;
            for (var position = 0; position < index; position++)
                node = node.Next!;
        }
        else
        {
            node = _tail!;
            for (var position = Count - 1; position > index; position--)
                node = node.Previous!;
        }

        return new Cursor(this, node);
    }

    /// <summary>Очищает список за O(1), делая прежние курсоры и перечислители недействительными.</summary>
    /// <remarks>Старые курсоры и перечислители могут удерживать отсоединённые узлы в памяти.</remarks>
    public void Clear()
    {
        // Маркер поколения отличает старую цепочку без прохода по её узлам.
        var generation = new object();
        _head = null;
        _tail = null;
        Count = 0;
        _generation = generation;
        IncrementVersion();
    }

    /// <summary>Предоставляет живой обратный обход без копирования элементов, O(1).</summary>
    /// <remarks>Версия фиксируется при создании перечислителя, а не представления.</remarks>
    public IEnumerable<T> Reverse()
    {
        return new ReverseEnumerable(this);
    }

    /// <summary>Создаёт прямой перечислитель за O(1); полный обход занимает O(n).</summary>
    /// <remarks>
    /// Current до первого шага и после конца недоступен. Current и MoveNext проверяют
    /// версию даже после конца. Повторный MoveNext после конца возвращает false.
    /// Reset не поддерживается; Dispose ничего не делает.
    /// </remarks>
    public IEnumerator<T> GetEnumerator()
    {
        return new Enumerator(this, backwards: false);
    }

    /// <inheritdoc cref="GetEnumerator"/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private Cursor? Find(IndexedPredicate<T> predicate, bool backwards)
    {
        var version = _version;
        var position = backwards ? Count - 1 : 0;
        var node = backwards ? _tail : _head;
        while (node is not null)
        {
            var matches = predicate(node.Value, position);
            ValidateVersion(version);
            if (matches)
                return new Cursor(this, node);

            node = backwards ? node.Previous : node.Next;
            position += backwards ? -1 : 1;
        }

        return null;
    }

    private void InsertBetween(Node? previous, Node? next, T item)
    {
        var node = new Node { Value = item, Previous = previous, Next = next };
        if (previous is null)
            _head = node;
        else
            previous.Next = node;

        if (next is null)
            _tail = node;
        else
            next.Previous = node;

        Count++;
        IncrementVersion();
    }

    private bool TryRemove(Node? node, [MaybeNullWhen(false)] out T value)
    {
        if (node is null)
        {
            value = default;
            return false;
        }

        value = node.Value;
        Remove(node);
        return true;
    }

    private void Remove(Node node)
    {
        if (node.Previous is null)
            _head = node.Next;
        else
            node.Previous.Next = node.Next;

        if (node.Next is null)
            _tail = node.Previous;
        else
            node.Next.Previous = node.Previous;

        node.Previous = null;
        node.Next = null;
        node.IsAttached = false;
        // Значение больше недоступно через курсор; освобождаем удерживаемую ссылку.
        node.Value = default!;
        Count--;
        IncrementVersion();
    }

    private void IncrementVersion()
    {
        _version = unchecked(_version + 1);
    }

    private void ValidateVersion(int expectedVersion)
    {
        if (expectedVersion != _version)
            throw new InvalidOperationException("Collection was modified; operation cannot be continued.");
    }

    internal sealed class Node
    {
        internal required T Value { get; set; }
        internal Node? Previous { get; set; }
        internal Node? Next { get; set; }
        internal bool IsAttached { get; set; } = true;
    }

    /// <summary>Позиция на элементе с локальными операциями за O(1), без доступа к узлам.</summary>
    /// <remarks>
    /// Каждый курсор независим. Движение меняет только его позицию. Изменения других
    /// элементов сохраняют курсор. После удаления его элемента или Clear любые операции
    /// курсора выбрасывают InvalidOperationException. Курсор нельзя создать вне списка.
    /// </remarks>
    public sealed class Cursor
    {
        private readonly DoublyLinkedList<T> _owner;
        private readonly object _generation;
        private Node _node;

        internal Cursor(DoublyLinkedList<T> owner, Node node)
        {
            _owner = owner;
            _node = node;
            _generation = owner._generation;
        }

        /// <summary>Читает или заменяет значение за O(1); запись инвалидирует перечислители.</summary>
        public T Value
        {
            get
            {
                Validate();
                return _node.Value;
            }
            set
            {
                Validate();
                _node.Value = value;
                _owner.IncrementVersion();
            }
        }

        /// <summary>Переходит вперёд; на хвосте возвращает false, сохраняя позицию.</summary>
        public bool MoveNext()
        {
            Validate();
            if (_node.Next is null)
                return false;

            _node = _node.Next;
            return true;
        }

        /// <summary>Переходит назад; на голове возвращает false, сохраняя позицию.</summary>
        public bool MovePrevious()
        {
            Validate();
            if (_node.Previous is null)
                return false;

            _node = _node.Previous;
            return true;
        }

        /// <summary>Вставляет перед текущим элементом; курсор остаётся на прежнем элементе.</summary>
        public void InsertBefore(T item)
        {
            Validate();
            _owner.InsertBetween(_node.Previous, _node, item);
        }

        /// <summary>Вставляет после текущего элемента; курсор остаётся на прежнем элементе.</summary>
        public void InsertAfter(T item)
        {
            Validate();
            _owner.InsertBetween(_node, _node.Next, item);
        }

        /// <summary>Удаляет текущий элемент, делая все курсоры на нём недействительными.</summary>
        public void Remove()
        {
            Validate();
            _owner.Remove(_node);
        }

        private void Validate()
        {
            if (_generation != _owner._generation || !_node.IsAttached)
                throw new InvalidOperationException("The cursor no longer refers to an element of this list.");
        }
    }

    private sealed class ReverseEnumerable(DoublyLinkedList<T> owner) : IEnumerable<T>
    {
        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(owner, backwards: true);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    private sealed class Enumerator(DoublyLinkedList<T> owner, bool backwards) : IEnumerator<T>
    {
        private readonly int _expectedVersion = owner._version;
        private Node? _current;
        private bool _started;

        public T Current
        {
            get
            {
                owner.ValidateVersion(_expectedVersion);
                return _current is null
                    ? throw new InvalidOperationException("Enumeration has not started or has already finished.")
                    : _current.Value;
            }
        }

        object? IEnumerator.Current => Current;

        public bool MoveNext()
        {
            owner.ValidateVersion(_expectedVersion);
            if (!_started)
                _current = backwards ? owner._tail : owner._head;
            else
                _current = backwards ? _current?.Previous : _current?.Next;
            _started = true;
            return _current is not null;
        }

        public void Dispose()
        {
            // Перечислитель не владеет ресурсами, требующими освобождения.
        }

        void IEnumerator.Reset()
        {
            throw new NotSupportedException("Reset is not supported. Obtain a new enumerator instead.");
        }
    }
}
