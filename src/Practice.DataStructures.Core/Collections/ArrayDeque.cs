using System.Collections;

namespace Practice.DataStructures.Core.Collections;

/// <summary>Дек с автоматически расширяемым хранилищем на массиве через собственный CircularArrayList.</summary>
/// <typeparam name="T">Тип элементов.</typeparam>
/// <remarks>
/// Добавление, чтение и извлечение доступны на обоих концах; перебор идёт от начала к концу.
/// Конструктор без параметров создаёт пустой дек. Вместимость растёт автоматически
/// и сохраняется при удалении и очистке. Позиционный API внутреннего списка скрыт.
/// Допускаются дубликаты и значения по умолчанию, включая null для ссылочных типов.
/// Коллекция не обеспечивает синхронизацию доступа из нескольких потоков.
/// </remarks>
public sealed class ArrayDeque<T> : IEnumerable<T>
{
    private readonly CircularArrayList<T> _list = [];

    private int _version;

    /// <summary>Получает количество элементов за O(1).</summary>
    public int Count => _list.Count;

    /// <summary>Проверяет отсутствие элементов за O(1).</summary>
    public bool IsEmpty => Count == 0;

    /// <summary>Добавляет элемент в конец и инвалидирует перечислители.</summary>
    /// <param name="item">Добавляемое значение.</param>
    /// <remarks>Амортизированная сложность — O(1), при росте — O(n) времени и дополнительной памяти.</remarks>
    /// <exception cref="OutOfMemoryException">Хранилище не удалось расширить; состояние дека сохраняется.</exception>
    public void PushBack(T item)
    {
        _list.Add(item);

        _version = unchecked(_version + 1);
    }

    /// <summary>Добавляет элемент в начало и инвалидирует перечислители.</summary>
    /// <param name="item">Добавляемое значение.</param>
    /// <remarks>Амортизированная сложность — O(1), при росте — O(n) времени и дополнительной памяти.</remarks>
    /// <exception cref="OutOfMemoryException">Хранилище не удалось расширить; состояние дека сохраняется.</exception>
    public void PushFront(T item)
    {
        _list.Insert(0, item);

        _version = unchecked(_version + 1);
    }

    /// <summary>Удаляет и возвращает последний элемент за O(1), инвалидируя перечислители.</summary>
    /// <returns>Удалённый элемент с конца дека.</returns>
    /// <exception cref="InvalidOperationException">Дек пуст; его состояние и перечислители сохраняются.</exception>
    public T PopBack()
    {
        if (IsEmpty)
            throw new InvalidOperationException("Deque is empty.");

        var lastIndex = _list.Count - 1;
        var item = _list[lastIndex];
        _list.RemoveAt(lastIndex);

        _version = unchecked(_version + 1);

        return item;
    }

    /// <summary>Удаляет и возвращает первый элемент за O(1), инвалидируя перечислители.</summary>
    /// <returns>Удалённый элемент из начала дека.</returns>
    /// <exception cref="InvalidOperationException">Дек пуст; его состояние и перечислители сохраняются.</exception>
    public T PopFront()
    {
        if (IsEmpty)
            throw new InvalidOperationException("Deque is empty.");

        var item = _list[0];
        _list.RemoveAt(0);

        _version = unchecked(_version + 1);

        return item;
    }

    /// <summary>Читает последний элемент за O(1), сохраняя состояние и перечислители.</summary>
    /// <returns>Элемент с конца дека без удаления.</returns>
    /// <exception cref="InvalidOperationException">Дек пуст.</exception>
    public T PeekBack()
    {
        return IsEmpty ? throw new InvalidOperationException("Deque is empty.")
            : _list[^1];
    }

    /// <summary>Читает первый элемент за O(1), сохраняя состояние и перечислители.</summary>
    /// <returns>Элемент из начала дека без удаления.</returns>
    /// <exception cref="InvalidOperationException">Дек пуст.</exception>
    public T PeekFront()
    {
        return IsEmpty ? throw new InvalidOperationException("Deque is empty.")
            : _list[0];
    }

    /// <summary>Удаляет все элементы за O(n), сохраняя вместимость.</summary>
    /// <remarks>Освобождённые ячейки очищаются. Каждый вызов инвалидирует перечислители,
    /// даже у пустого дека. Дек можно использовать повторно.</remarks>
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

    private sealed class Enumerator(ArrayDeque<T> owner) : IEnumerator<T>
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
