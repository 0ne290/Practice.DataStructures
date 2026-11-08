using System.Collections;

namespace Practice.DataStructures.Core.Collections;

/// <summary>Стек LIFO на собственном односвязном списке.</summary>
/// <typeparam name="T">Тип элементов.</typeparam>
/// <remarks>
/// Допускаются дубликаты и значения по умолчанию, включая null для ссылочных типов.
/// Коллекция не обеспечивает синхронизацию доступа из нескольких потоков.
/// </remarks>
public sealed class LinkedStack<T> : IEnumerable<T>
{
    private readonly SinglyLinkedList<T> _list = [];
    private int _version;

    /// <summary>Получает количество элементов за O(1).</summary>
    public int Count => _list.Count;

    /// <summary>Проверяет отсутствие элементов за O(1).</summary>
    public bool IsEmpty => Count == 0;

    /// <summary>Добавляет элемент на вершину за O(1), без переноса остальных элементов.</summary>
    /// <param name="item">Добавляемое значение.</param>
    public void Push(T item)
    {
        _list.Values.AddFirst(item);
        _version = unchecked(_version + 1);
    }

    /// <summary>Удаляет и возвращает вершину за O(1).</summary>
    /// <returns>Последний добавленный из оставшихся элементов.</returns>
    /// <exception cref="InvalidOperationException">Стек пуст. Его состояние сохраняется.</exception>
    public T Pop()
    {
        if (!_list.Cursors.BeforeFirst.TryRemoveAfter(out var item))
            throw new InvalidOperationException("Stack is empty.");

        _version = unchecked(_version + 1);
        return item;
    }

    /// <summary>Читает вершину за O(1), сохраняя состояние стека.</summary>
    /// <returns>Последний добавленный из оставшихся элементов.</returns>
    /// <exception cref="InvalidOperationException">Стек пуст.</exception>
    public T Peek()
    {
        return _list.Values.TryFirst(out var item)
            ? item
            : throw new InvalidOperationException("Stack is empty.");
    }

    /// <summary>Удаляет все элементы за O(1); стек можно использовать повторно.</summary>
    /// <remarks>Старый перечислитель может удерживать прежние узлы до освобождения ссылки на него.</remarks>
    public void Clear()
    {
        _list.Values.Clear();
        _version = unchecked(_version + 1);
    }

    /// <summary>Создаёт независимый перечислитель от вершины к основанию за O(1).</summary>
    /// <returns>Перечислитель перед первым элементом; полный обход — O(n), без копирования элементов.</returns>
    /// <remarks>
    /// Успешные Push, Pop и каждый Clear инвалидируют старые перечислители.
    /// Чтение и отклонённые операции сохраняют их состояние.
    /// После изменения MoveNext и Current выбрасывают InvalidOperationException,
    /// включая состояния до начала, после конца и после Dispose.
    /// Current до начала, после конца и после Dispose выбрасывает InvalidOperationException.
    /// После конца и Dispose MoveNext возвращает false, если стек не изменялся.
    /// Reset не поддерживается; повторный Dispose безопасен.
    /// </remarks>
    public IEnumerator<T> GetEnumerator() => new Enumerator(this);

    /// <inheritdoc cref="GetEnumerator"/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private sealed class Enumerator(LinkedStack<T> owner) : IEnumerator<T>
    {
        private readonly int _expectedVersion = owner._version;
        private readonly IEnumerator<T> _inner = owner._list.GetEnumerator();
        private bool _disposed;

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

        public bool MoveNext()
        {
            ValidateVersion();
            return !_disposed && _inner.MoveNext();
        }

        public void Reset() => throw new NotSupportedException();

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
