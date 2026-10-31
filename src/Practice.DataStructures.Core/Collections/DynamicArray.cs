using System.Collections;

namespace Practice.DataStructures.Core.Collections;

/// <summary>
/// Динамический массив с добавлением в конец, доступом и удалением по индексу и перебором элементов.
/// </summary>
/// <typeparam name="T">Тип хранимых элементов.</typeparam>
/// <remarks>
/// <para>
/// Элементы занимают индексы от нуля до <see cref="Count"/> минус один.
/// Оставшаяся часть <see cref="Capacity"/> является резервом и не участвует в переборе.
/// Допускаются повторяющиеся элементы и значения по умолчанию для <typeparamref name="T"/>.
/// </para>
/// <para>
/// Коллекция не обеспечивает синхронизацию доступа из нескольких потоков.
/// Правила обнаружения изменений при обходе описаны у <see cref="GetEnumerator"/>.
/// </para>
/// </remarks>
public sealed class DynamicArray<T> : IEnumerable<T>
{
    private T[] _array;
    private int _version;

    /// <summary>Создаёт пустой массив с заданной начальной вместимостью.</summary>
    /// <param name="capacity">Начальная вместимость, не меньше единицы.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="capacity"/> меньше единицы.
    /// </exception>
    /// <exception cref="OutOfMemoryException">Не удалось выделить внутренний массив.</exception>
    public DynamicArray(int capacity)
    {
        if (capacity < 1)
            throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "Capacity must be greater than zero.");

        _array = new T[capacity];
        Capacity = capacity;
        Count = 0;
        _version = 0;
    }

    /// <summary>Создаёт пустой массив с начальной вместимостью 8 элементов.</summary>
    public DynamicArray() : this(8) { }

    /// <summary>Получает количество сохранённых элементов за <c>O(1)</c>.</summary>
    /// <value>Число от нуля до <see cref="Capacity"/> включительно.</value>
    public int Count { get; private set; }

    /// <summary>Получает количество элементов, помещающихся без увеличения массива.</summary>
    /// <value>Длина внутреннего массива, не меньше <see cref="Count"/>.</value>
    /// <remarks>Чтение свойства выполняется за <c>O(1)</c>.</remarks>
    public int Capacity { get; private set; }

    /// <summary>Получает или заменяет элемент по индексу за <c>O(1)</c>.</summary>
    /// <param name="index">Индекс от нуля до <see cref="Count"/> минус один.</param>
    /// <value>Элемент на указанной позиции.</value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> отрицателен или не меньше <see cref="Count"/>.
    /// </exception>
    /// <remarks>
    /// Замена сохраняет <see cref="Count"/> и <see cref="Capacity"/>, но инвалидирует
    /// ранее полученные перечислители, даже если записано то же значение.
    /// Чтение и отклонённая запись не инвалидируют перечислители.
    /// </remarks>
    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index), index, "Index must refer to an existing element.");

            return _array[index];
        }

        set
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index), index, "Index must refer to an existing element.");

            _array[index] = value;
            _version = unchecked(_version + 1);
        }
    }

    /// <summary>Добавляет элемент в конец массива, сохраняя порядок предыдущих элементов.</summary>
    /// <param name="item">Добавляемый элемент.</param>
    /// <exception cref="OutOfMemoryException">Не удалось выделить массив большей вместимости.</exception>
    /// <remarks>
    /// <para>
    /// При заполнении хранилища вместимость автоматически увеличивается.
    /// Амортизированная сложность добавления — <c>O(1)</c>; при увеличении массива —
    /// <c>O(n)</c>, где <c>n</c> — текущее количество элементов.
    /// </para>
    /// <para>
    /// Успешное добавление увеличивает <see cref="Count"/> на один и инвалидирует
    /// ранее полученные перечислители. Если подготовить новое хранилище не удалось,
    /// содержимое, количество элементов и вместимость сохраняются.
    /// </para>
    /// </remarks>
    public void Add(T item)
    {
        if (Count >= Capacity)
        {
            var newCapacity = Capacity * 2;
            var newArray = new T[newCapacity];
            _array.CopyTo(newArray, 0);
            _array = newArray;
            Capacity = newCapacity;
        }

        _array[Count] = item;
        Count++;
        _version = unchecked(_version + 1);
    }

    /// <summary>Удаляет элемент по индексу, сохраняя порядок остальных элементов.</summary>
    /// <param name="index">Индекс удаляемого элемента от нуля до <see cref="Count"/> минус один.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> отрицателен или не меньше <see cref="Count"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Успешное удаление уменьшает <see cref="Count"/> на один и сохраняет
    /// <see cref="Capacity"/>, в том числе при удалении единственного элемента.
    /// Освободившаяся ячейка хранилища получает значение по умолчанию для
    /// <typeparamref name="T"/>, чтобы не удерживать лишние ссылки.
    /// </para>
    /// <para>
    /// Удаление инвалидирует ранее полученные перечислители по правилам
    /// <see cref="GetEnumerator"/>. Недопустимый индекс не меняет содержимое,
    /// количество элементов, вместимость и состояние перечислителей.
    /// </para>
    /// <para>
    /// Сложность в худшем случае — <c>O(n)</c>, где <c>n</c> — текущее количество
    /// элементов; удаление последнего элемента — <c>O(1)</c>.
    /// Дополнительная память — <c>O(1)</c>.
    /// </para>
    /// </remarks>
    public void RemoveAt(int index)
    {
        if (index < 0 || index >= Count)
            throw new ArgumentOutOfRangeException(nameof(index), index, "Index must refer to an existing element.");

        for (var i = index; i < Count - 1; i++)
            _array[i] = _array[i + 1];
        Array.Clear(_array, Count - 1, 1);

        Count--;
        _version = unchecked(_version + 1);
    }

    /// <summary>Создаёт новый перечислитель элементов в порядке индексов.</summary>
    /// <returns>Перечислитель, расположенный перед первым элементом.</returns>
    /// <remarks>
    /// <para>
    /// Получение перечислителя выполняется за <c>O(1)</c>, полный обход — за <c>O(n)</c>,
    /// где <c>n</c> — <see cref="Count"/>. Каждый обход имеет независимое состояние;
    /// копия содержимого массива не создаётся.
    /// </para>
    /// <para>
    /// Версия списка фиксируется при получении перечислителя.
    /// Успешный вызов <see cref="Add"/>, <see cref="RemoveAt"/> или запись через индексатор, включая запись того же значения,
    /// инвалидирует ранее полученные перечислители. Их <see cref="IEnumerator.MoveNext"/>
    /// и <see cref="IEnumerator{T}.Current"/> после этого выбрасывают
    /// <see cref="InvalidOperationException"/>, в том числе до первого шага и после завершения обхода.
    /// Чтение, отклонённая запись и удаление по недопустимому индексу сохраняют работоспособность перечислителя.
    /// </para>
    /// <para>
    /// <see cref="IEnumerator{T}.Current"/> до первого успешного шага, после завершения
    /// или освобождения перечислителя выбрасывает <see cref="InvalidOperationException"/>.
    /// <see cref="IEnumerator.Reset"/> не поддерживается и выбрасывает
    /// <see cref="NotSupportedException"/>. Для нового обхода нужно получить новый перечислитель.
    /// </para>
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

    /// <summary>Хранит позицию обхода и версию списка на момент получения перечислителя.</summary>
    private sealed class Enumerator : IEnumerator<T>
    {
        private readonly DynamicArray<T> _owner;
        private readonly int _expectedVersion;
        private int _index;

        internal Enumerator(DynamicArray<T> owner)
        {
            _owner = owner;
            _expectedVersion = owner._version;
            _index = -1;
        }

        /// <summary>Возвращает текущий элемент после проверки состояния и версии списка.</summary>
        /// <exception cref="InvalidOperationException">
        /// Список изменился либо перечислитель не расположен на элементе.
        /// </exception>
        public T Current
        {
            get
            {
                ValidateVersion();
                if (_index < 0 || _index >= _owner.Count)
                    throw new InvalidOperationException("Enumeration has not started or has already finished.");

                return _owner._array[_index];
            }
        }

        /// <inheritdoc cref="Current"/>
        object? IEnumerator.Current => Current;

        /// <summary>Перемещает перечислитель на следующий элемент.</summary>
        /// <returns><see langword="true"/>, если следующий элемент существует; иначе <see langword="false"/>.</returns>
        /// <exception cref="InvalidOperationException">Список изменился после получения перечислителя.</exception>
        public bool MoveNext()
        {
            ValidateVersion();
            if (_index < _owner.Count)
                _index++;

            return _index < _owner.Count;
        }

        /// <summary>Завершает обход; повторное освобождение допустимо.</summary>
        public void Dispose()
        {
            _index = _owner.Count;
        }

        /// <summary>Возврат перечислителя в начальное состояние не поддерживается.</summary>
        /// <exception cref="NotSupportedException">Выбрасывается при каждом вызове.</exception>
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
