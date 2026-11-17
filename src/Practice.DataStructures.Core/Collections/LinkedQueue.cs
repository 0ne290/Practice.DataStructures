using System.Collections;

namespace Practice.DataStructures.Core.Collections;

/// <summary>Очередь FIFO на собственном односвязном списке.</summary>
/// <typeparam name="T">Тип элементов.</typeparam>
/// <remarks>
/// Добавление выполняется в конец, чтение и извлечение — из начала.
/// Пустая очередь создаётся конструктором без параметров. Заранее заданного
/// ограничения количества элементов нет; рост не требует переноса всех элементов.
/// Узлы и курсорный API списка скрыты. Допускаются дубликаты и значения
/// по умолчанию, включая null для ссылочных типов.
/// Коллекция не обеспечивает синхронизацию доступа из нескольких потоков.
/// </remarks>
public sealed class LinkedQueue<T> : IEnumerable<T>
{
    private readonly SinglyLinkedList<T> _list = [];

    private int _version;

    /// <summary>Получает количество элементов за O(1).</summary>
    public int Count => _list.Count;

    /// <summary>Проверяет отсутствие элементов за O(1).</summary>
    public bool IsEmpty => Count == 0;

    /// <summary>Добавляет элемент в конец очереди и инвалидирует перечислители.</summary>
    /// <param name="item">Добавляемое значение.</param>
    /// <remarks>Сложность — O(1), без переноса остальных элементов.</remarks>
    public void Enqueue(T item)
    {
        _list.Values.AddLast(item);
        _version = unchecked(_version + 1);
    }

    /// <summary>Удаляет и возвращает начало очереди за O(1), инвалидируя перечислители.</summary>
    /// <returns>Первый добавленный из оставшихся элементов.</returns>
    /// <exception cref="InvalidOperationException">Очередь пуста; её состояние и перечислители сохраняются.</exception>
    public T Dequeue()
    {
        if (!_list.Cursors.BeforeFirst.TryRemoveAfter(out var item))
            throw new InvalidOperationException("Queue is empty.");

        _version = unchecked(_version + 1);
        return item;
    }

    /// <summary>Читает начало очереди за O(1), сохраняя состояние и перечислители.</summary>
    /// <returns>Первый добавленный из оставшихся элементов, без удаления.</returns>
    /// <exception cref="InvalidOperationException">Очередь пуста.</exception>
    public T Peek()
    {
        return _list.Values.TryFirst(out var item) ? item
            : throw new InvalidOperationException("Queue is empty.");
    }

    /// <summary>Удаляет все элементы за O(1); очередь можно использовать повторно.</summary>
    /// <remarks>Каждый вызов инвалидирует перечислители, даже у пустой очереди.
    /// Старый перечислитель может удерживать прежние узлы до освобождения ссылки на него.</remarks>
    public void Clear()
    {
        _list.Values.Clear();
        _version = unchecked(_version + 1);
    }

    /// <summary>Создаёт независимый перечислитель от начала очереди к концу за O(1).</summary>
    /// <returns>Перечислитель перед первым элементом; полный обход — O(n), без удаления и копирования элементов.</returns>
    /// <remarks>
    /// Успешные Enqueue, Dequeue и каждый Clear, включая очистку пустой очереди,
    /// инвалидируют ранее созданные перечислители. Peek, Count, IsEmpty и отклонённые
    /// операции сохраняют их состояние. После изменения MoveNext и оба Current
    /// выбрасывают InvalidOperationException до начала, во время, после конца обхода
    /// и после Dispose. Current до первого успешного шага, после конца и после Dispose
    /// также выбрасывает InvalidOperationException. Dispose завершает обход;
    /// повторный Dispose безопасен. При неизменной очереди MoveNext после конца
    /// и после Dispose возвращает false. Reset не поддерживается.
    /// </remarks>
    public IEnumerator<T> GetEnumerator() => new Enumerator(this);

    /// <inheritdoc cref="GetEnumerator"/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private sealed class Enumerator(LinkedQueue<T> owner) : IEnumerator<T>
    {
        private readonly int _expectedVersion = owner._version;
        private readonly IEnumerator<T> _inner = owner._list.GetEnumerator();
        private bool _disposed;

        public bool MoveNext()
        {
            ValidateVersion();
            return !_disposed && _inner.MoveNext();
        }

        public void Reset() => throw new NotSupportedException("Reset is not supported. Obtain a new enumerator instead.");

        public T Current
        {
            get
            {
                ValidateVersion();
                return _disposed
                    ? throw new InvalidOperationException("Enumeration has already been disposed.")
                    : _inner.Current;
            }
        }

        object? IEnumerator.Current => Current;

        public void Dispose()
        {
            _inner.Dispose();
            _disposed = true;
        }

        private void ValidateVersion()
        {
            if (_expectedVersion != owner._version)
                throw new InvalidOperationException("Collection was modified; enumeration cannot continue.");
        }
    }
}
