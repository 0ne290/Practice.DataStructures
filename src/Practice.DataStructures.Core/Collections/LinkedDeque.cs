using System.Collections;

namespace Practice.DataStructures.Core.Collections;

/// <summary>Дек на двусвязных узлах через собственный DoublyLinkedList.</summary>
/// <typeparam name="T">Тип элементов.</typeparam>
/// <remarks>
/// Добавление, чтение и извлечение доступны на обоих концах; перебор идёт от начала к концу.
/// Конструктор без параметров создаёт пустой дек. Заранее заданного ограничения
/// количества элементов нет; добавление не требует переноса остальных элементов.
/// Узлы и курсорный API внутреннего списка скрыты. Допускаются дубликаты
/// и значения по умолчанию, включая null для ссылочных типов.
/// Коллекция не обеспечивает синхронизацию доступа из нескольких потоков.
/// </remarks>
public sealed class LinkedDeque<T> : IEnumerable<T>
{
    private readonly DoublyLinkedList<T> _list = [];

    private int _version;

    /// <summary>Получает количество элементов за O(1).</summary>
    public int Count => _list.Count;

    /// <summary>Проверяет отсутствие элементов за O(1).</summary>
    public bool IsEmpty => Count == 0;

    /// <summary>Добавляет элемент в конец за O(1), инвалидируя перечислители.</summary>
    /// <param name="item">Добавляемое значение.</param>
    /// <exception cref="OutOfMemoryException">Не удалось создать узел; состояние дека сохраняется.</exception>
    public void PushBack(T item)
    {
        _list.AddLast(item);

        _version = unchecked(_version + 1);
    }

    /// <summary>Добавляет элемент в начало за O(1), инвалидируя перечислители.</summary>
    /// <param name="item">Добавляемое значение.</param>
    /// <exception cref="OutOfMemoryException">Не удалось создать узел; состояние дека сохраняется.</exception>
    public void PushFront(T item)
    {
        _list.AddFirst(item);

        _version = unchecked(_version + 1);
    }

    /// <summary>Удаляет и возвращает последний элемент за O(1), инвалидируя перечислители.</summary>
    /// <returns>Удалённый элемент с конца дека.</returns>
    /// <exception cref="InvalidOperationException">Дек пуст; его состояние и перечислители сохраняются.</exception>
    public T PopBack()
    {
        if (!_list.TryRemoveLast(out var item))
            throw new InvalidOperationException("Deque is empty.");

        _version = unchecked(_version + 1);

        return item;
    }

    /// <summary>Удаляет и возвращает первый элемент за O(1), инвалидируя перечислители.</summary>
    /// <returns>Удалённый элемент из начала дека.</returns>
    /// <exception cref="InvalidOperationException">Дек пуст; его состояние и перечислители сохраняются.</exception>
    public T PopFront()
    {
        if (!_list.TryRemoveFirst(out var item))
            throw new InvalidOperationException("Deque is empty.");

        _version = unchecked(_version + 1);

        return item;
    }

    /// <summary>Читает последний элемент за O(1), сохраняя состояние и перечислители.</summary>
    /// <returns>Элемент с конца дека без удаления.</returns>
    /// <remarks>Чтение создаёт временный курсор внутреннего списка.</remarks>
    /// <exception cref="InvalidOperationException">Дек пуст.</exception>
    public T PeekBack()
    {
        var item = _list.Last;
        return item is null ? throw new InvalidOperationException("Deque is empty.") : item.Value;
    }

    /// <summary>Читает первый элемент за O(1), сохраняя состояние и перечислители.</summary>
    /// <returns>Элемент из начала дека без удаления.</returns>
    /// <remarks>Чтение создаёт временный курсор внутреннего списка.</remarks>
    /// <exception cref="InvalidOperationException">Дек пуст.</exception>
    public T PeekFront()
    {
        var item = _list.First;
        return item is null ? throw new InvalidOperationException("Deque is empty.") : item.Value;
    }

    /// <summary>Удаляет все элементы за O(1); дек можно использовать повторно.</summary>
    /// <remarks>
    /// Каждый вызов инвалидирует перечислители, даже у пустого дека.
    /// Цепочка узлов отсоединяется без обхода. Старый перечислитель может удерживать
    /// прежние узлы, в том числе после Dispose, пока на него сохраняется ссылка.
    /// </remarks>
    /// <exception cref="OutOfMemoryException">Не удалось создать маркер поколения списка; состояние дека сохраняется.</exception>
    public void Clear()
    {
        _list.Clear();
        _version = unchecked(_version + 1);
    }

    /// <summary>Создаёт независимый перечислитель от начала к концу за O(1).</summary>
    /// <returns>Перечислитель перед первым элементом; полный обход — O(n), без копирования и удаления элементов.</returns>
    /// <remarks>
    /// Успешные PushFront, PushBack, PopFront, PopBack и каждый Clear, включая очистку
    /// пустого дека, инвалидируют ранее созданные перечислители. PeekFront, PeekBack,
    /// Count, IsEmpty и отклонённые операции сохраняют их состояние. Версия фиксируется
    /// при создании перечислителя. После изменения MoveNext и оба Current выбрасывают
    /// InvalidOperationException до начала, во время, после конца обхода и после Dispose.
    /// Current до первого успешного шага, после конца и после Dispose также выбрасывает
    /// InvalidOperationException. Dispose завершает обход; повторный вызов безопасен.
    /// При неизменном деке MoveNext после конца и после Dispose возвращает false.
    /// Reset не поддерживается и всегда выбрасывает NotSupportedException.
    /// </remarks>
    public IEnumerator<T> GetEnumerator() => new Enumerator(this);

    /// <inheritdoc cref="GetEnumerator"/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private sealed class Enumerator(LinkedDeque<T> owner) : IEnumerator<T>
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
