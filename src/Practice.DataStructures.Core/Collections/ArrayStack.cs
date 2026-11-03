using System.Collections;

namespace Practice.DataStructures.Core.Collections;

/// <summary>Стек LIFO на собственном динамическом массиве.</summary>
/// <typeparam name="T">Тип элементов.</typeparam>
/// <remarks>
/// Допускаются дубликаты и значения по умолчанию, включая null для ссылочных типов.
/// Коллекция не обеспечивает синхронизацию доступа из нескольких потоков.
/// </remarks>
public sealed class ArrayStack<T> : IEnumerable<T>
{
    private readonly DynamicArray<T> _array = [];
    private int _version;

    /// <summary>Получает количество элементов за O(1).</summary>
    public int Count => _array.Count;

    /// <summary>Проверяет отсутствие элементов за O(1).</summary>
    public bool IsEmpty => Count == 0;

    /// <summary>Добавляет элемент на вершину.</summary>
    /// <param name="item">Добавляемый элемент.</param>
    /// <remarks>Амортизированная сложность — O(1), при росте массива — O(n).</remarks>
    /// <exception cref="OutOfMemoryException">Не удалось выделить хранилище.</exception>
    public void Push(T item)
    {
        _array.Add(item);
        _version = unchecked(_version + 1);
    }

    /// <summary>Удаляет и возвращает вершину за O(1).</summary>
    /// <returns>Последний добавленный из оставшихся элементов.</returns>
    /// <remarks>Освободившаяся ячейка очищается; вместимость сохраняется.</remarks>
    /// <exception cref="InvalidOperationException">Стек пуст. Его состояние сохраняется.</exception>
    public T Pop()
    {
        if (IsEmpty)
            throw new InvalidOperationException("Stack is empty.");

        var index = Count - 1;
        var item = _array[index];
        _array.RemoveAt(index);
        _version = unchecked(_version + 1);
        return item;
    }

    /// <summary>Возвращает вершину за O(1), сохраняя состояние стека.</summary>
    /// <returns>Последний добавленный из оставшихся элементов.</returns>
    /// <exception cref="InvalidOperationException">Стек пуст.</exception>
    public T Peek()
    {
        return IsEmpty ? throw new InvalidOperationException("Stack is empty.") :
            _array[Count - 1];
    }

    /// <summary>Удаляет все элементы за O(n), сохраняя вместимость.</summary>
    /// <remarks>Занятые ячейки очищаются, стек можно использовать повторно.</remarks>
    public void Clear()
    {
        _array.Clear();
        _version = unchecked(_version + 1);
    }

    /// <summary>Создаёт независимый перечислитель от вершины к основанию за O(1).</summary>
    /// <returns>Перечислитель перед первым элементом; полный обход занимает O(n) без копирования элементов.</returns>
    /// <remarks>
    /// Успешные Push, Pop и каждый Clear, включая очистку пустого стека, инвалидируют
    /// ранее полученные перечислители. Peek и отклонённый Pop сохраняют их состояние.
    /// После изменения MoveNext и Current выбрасывают InvalidOperationException,
    /// в том числе до первого шага и после завершения обхода.
    /// Current до первого шага, после завершения или Dispose также выбрасывает
    /// InvalidOperationException. Reset не поддерживается.
    /// </remarks>
    public IEnumerator<T> GetEnumerator() => new Enumerator(this);

    /// <inheritdoc cref="GetEnumerator"/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private sealed class Enumerator(ArrayStack<T> owner) : IEnumerator<T>
    {
        private readonly int _expectedVersion = owner._version;
        private int _index = owner.Count;
        private bool _hasCurrent;
        private bool _finished;

        public T Current
        {
            get
            {
                ValidateVersion();
                if (!_hasCurrent)
                    throw new InvalidOperationException("Enumeration has not started or has already finished.");
                return owner._array[_index];
            }
        }

        object? IEnumerator.Current => Current;

        public bool MoveNext()
        {
            ValidateVersion();
            if (_finished)
                return false;
            if (_index > 0)
            {
                _index--;
                _hasCurrent = true;
                return true;
            }

            _hasCurrent = false;
            _finished = true;
            return false;
        }

        public void Reset() => throw new NotSupportedException();

        public void Dispose()
        {
            _hasCurrent = false;
            _finished = true;
        }

        private void ValidateVersion()
        {
            if (_expectedVersion != owner._version)
                throw new InvalidOperationException("Collection was modified; enumeration cannot continue.");
        }
    }
}
