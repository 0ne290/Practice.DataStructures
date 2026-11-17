using System.Collections;

namespace Practice.DataStructures.Core.Collections;

/// <summary>
/// Расширяемый индексируемый список с кольцевым хранением на массиве.
/// </summary>
/// <typeparam name="T">Тип хранимых элементов.</typeparam>
/// <remarks>
/// <para>
/// Поддерживает добавление в конец, вставку, чтение, замену и удаление по индексу,
/// поиск по условию, очистку и перебор от первого логического элемента к последнему.
/// Индексы отсчитываются от нуля и не оборачиваются. Кольцевое размещение внутри
/// массива не изменяет логический порядок. Допускаются дубликаты и значения
/// по умолчанию для <typeparamref name="T"/>, включая допустимые для типа null.
/// </para>
/// <para>
/// Вместимость автоматически увеличивается при заполнении; удаление и очистка
/// сохраняют её. Доступ по индексу и чтение концов выполняются за O(1), добавление
/// на любом конце — за амортизированное O(1), удаление с любого конца — за O(1).
/// Стоимость остальных операций указана у соответствующих членов.
/// </para>
/// <para>
/// Коллекция не обеспечивает синхронизацию доступа из нескольких потоков.
/// Правила обнаружения изменений при обходе описаны у <see cref="GetEnumerator"/>.
/// </para>
/// </remarks>
public sealed class CircularArrayList<T> : IEnumerable<T>
{
    private T[] _array;
    private int _version;

    private int _headIndex;
    private int _tailIndex;

    /// <summary>Создаёт пустой список с заданной начальной вместимостью.</summary>
    /// <param name="capacity">Начальная вместимость, не меньше единицы.</param>
    /// <exception cref="ArgumentOutOfRangeException">Вместимость меньше единицы.</exception>
    /// <exception cref="OutOfMemoryException">Не удалось выделить внутренний массив.</exception>
    public CircularArrayList(int capacity)
    {
        if (capacity < 1)
            throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "Capacity must be greater than zero.");

        _array = new T[capacity];
        Capacity = capacity;
        Count = 0;
        _version = 0;

        _headIndex = 0;
        _tailIndex = Count;
    }

    /// <summary>Создаёт пустой список с начальной вместимостью 8 элементов.</summary>
    public CircularArrayList() : this(8) { }

    /// <summary>Получает количество сохранённых элементов за O(1).</summary>
    /// <value>Число от нуля до <see cref="Capacity"/> включительно.</value>
    public int Count { get; private set; }

    /// <summary>Получает количество элементов, помещающихся без расширения хранилища.</summary>
    /// <value>Длина внутреннего массива, не меньше <see cref="Count"/>.</value>
    /// <remarks>Чтение свойства выполняется за O(1).</remarks>
    public int Capacity { get; private set; }

    /// <summary>Получает первый логический элемент без удаления за O(1).</summary>
    /// <value>Значение элемента с индексом 0.</value>
    /// <exception cref="InvalidOperationException">Список пуст.</exception>
    /// <remarks>Чтение сохраняет состояние списка и перечислителей.</remarks>
    public T First => Count > 0 ? _array[_headIndex] : throw new InvalidOperationException("List is empty.");

    /// <summary>Получает последний логический элемент без удаления за O(1).</summary>
    /// <value>Значение элемента с индексом <see cref="Count"/> минус один.</value>
    /// <exception cref="InvalidOperationException">Список пуст.</exception>
    /// <remarks>Чтение сохраняет состояние списка и перечислителей.</remarks>
    public T Last => Count > 0 ? _array[PrevIndex(_tailIndex)] : throw new InvalidOperationException("List is empty.");

    /// <summary>Получает или заменяет элемент по логическому индексу за O(1).</summary>
    /// <param name="index">Индекс от нуля до <see cref="Count"/> минус один.</param>
    /// <value>Элемент на указанной позиции.</value>
    /// <exception cref="ArgumentOutOfRangeException">Индекс отрицателен или не меньше Count.</exception>
    /// <remarks>
    /// Замена сохраняет Count и Capacity, но инвалидирует ранее полученные перечислители,
    /// даже если записано то же значение. Чтение и отклонённая запись сохраняют состояние.
    /// </remarks>
    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index), index, "Index must refer to an existing element.");

            return _array[PhysicalIndex(index)];
        }

        set
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index), index, "Index must refer to an existing element.");

            _array[PhysicalIndex(index)] = value;
            _version = unchecked(_version + 1);
        }
    }

    /// <summary>Добавляет элемент в конец, сохраняя порядок предыдущих элементов.</summary>
    /// <param name="item">Добавляемый элемент.</param>
    /// <exception cref="OutOfMemoryException">Хранилище не удалось расширить.</exception>
    /// <remarks>
    /// По результату соответствует Insert(Count, item). Увеличивает Count на один,
    /// при необходимости расширяет хранилище и инвалидирует перечислители.
    /// Если подготовить новое хранилище не удалось, состояние списка сохраняется.
    /// Амортизированная сложность — O(1); при расширении — O(n) времени и дополнительной
    /// памяти, где n — количество элементов до добавления; без расширения память — O(1).
    /// </remarks>
    public void Add(T item)
    {
        EnsureCapacity();

        _array[_tailIndex] = item;
        _tailIndex = NextIndex(_tailIndex);
        Count++;
        _version = unchecked(_version + 1);
    }

    /// <summary>Вставляет элемент перед указанной позицией с сохранением порядка остальных.</summary>
    /// <param name="index">Позиция от нуля до Count включительно; Count означает вставку в конец.</param>
    /// <param name="item">Вставляемый элемент.</param>
    /// <exception cref="ArgumentOutOfRangeException">Позиция отрицательна или больше Count.</exception>
    /// <exception cref="OutOfMemoryException">Хранилище не удалось расширить.</exception>
    /// <remarks>
    /// Увеличивает Count на один, при необходимости расширяет хранилище и инвалидирует
    /// перечислители. Недопустимая позиция и ошибка выделения нового массива сохраняют
    /// содержимое, количество, вместимость и состояние перечислителей.
    /// Без расширения сложность — O(1 + min(index, n − index)) времени и O(1)
    /// дополнительной памяти; с расширением — O(n) времени и памяти.
    /// Здесь n — количество элементов до вставки. Вставка на любом конце —
    /// амортизированное O(1).
    /// </remarks>
    public void Insert(int index, T item)
    {
        if (index < 0 || index > Count)
            throw new ArgumentOutOfRangeException(nameof(index), index, "Index must be between zero and Count, inclusive.");

        EnsureCapacity();

        if (index < Count - index)
        {
            _headIndex = PrevIndex(_headIndex);

            for (var i = 0; i < index; i++)
                _array[PhysicalIndex(i)] = _array[PhysicalIndex(i + 1)];
        }
        else
        {
            for (var i = Count; i > index; i--)
                _array[PhysicalIndex(i)] = _array[PhysicalIndex(i - 1)];
            _tailIndex = NextIndex(_tailIndex);
        }

        _array[PhysicalIndex(index)] = item;

        Count++;
        _version = unchecked(_version + 1);
    }

    private void EnsureCapacity()
    {
        if (Count < Capacity)
            return;

        var firstPart = Math.Min(Count, Capacity - _headIndex);

        var newCapacity = (int)Math.Min((long)Capacity * 2, Array.MaxLength);
        if (newCapacity <= Count)
            throw new OutOfMemoryException("The list cannot grow beyond the maximum array length.");
        var newArray = new T[newCapacity];
        Array.Copy(_array, _headIndex, newArray, 0, firstPart);
        Array.Copy(_array, 0, newArray, firstPart, Count - firstPart);

        _array = newArray;
        Capacity = newCapacity;
        _headIndex = 0;
        _tailIndex = Count;
    }

    /// <summary>Удаляет элемент по логическому индексу, сохраняя порядок и вместимость.</summary>
    /// <param name="index">Индекс от нуля до Count минус один.</param>
    /// <exception cref="ArgumentOutOfRangeException">Индекс отрицателен или не меньше Count.</exception>
    /// <remarks>
    /// Уменьшает Count на один, очищает освободившуюся ячейку значением по умолчанию
    /// для T и инвалидирует перечислители. Недопустимый индекс сохраняет содержимое,
    /// количество, вместимость и состояние перечислителей.
    /// Сложность — O(1 + min(index, n − 1 − index)), где n — количество элементов
    /// до удаления; на любом конце — O(1). Дополнительная память — O(1).
    /// </remarks>
    public void RemoveAt(int index)
    {
        if (index < 0 || index >= Count)
            throw new ArgumentOutOfRangeException(nameof(index), index, "Index must refer to an existing element.");

        if (index < Count / 2)
        {
            for (var i = index; i > 0; i--)
                _array[PhysicalIndex(i)] = _array[PhysicalIndex(i - 1)];

            Array.Clear(_array, _headIndex, 1);
            _headIndex = NextIndex(_headIndex);
        }
        else
        {
            for (var i = index; i < Count - 1; i++)
                _array[PhysicalIndex(i)] = _array[PhysicalIndex(i + 1)];

            Array.Clear(_array, PhysicalIndex(Count - 1), 1);
            _tailIndex = PrevIndex(_tailIndex);
        }

        Count--;
        _version = unchecked(_version + 1);
    }

    /// <summary>Удаляет все элементы с сохранением вместимости.</summary>
    /// <remarks>
    /// Очищает занятые ячейки значениями по умолчанию для T, освобождая хранимые ссылки,
    /// и устанавливает Count в ноль. Каждый вызов инвалидирует перечислители,
    /// включая очистку пустого списка. Сложность — O(n), дополнительная память — O(1).
    /// </remarks>
    public void Clear()
    {
        var firstPart = Math.Min(Count, Capacity - _headIndex);
        Array.Clear(_array, _headIndex, firstPart);
        Array.Clear(_array, 0, Count - firstPart);

        Count = 0;
        _version = unchecked(_version + 1);
        _headIndex = 0;
        _tailIndex = Count;
    }

    /// <summary>Возвращает логический индекс первого элемента, удовлетворяющего условию.</summary>
    /// <param name="indexedPredicate">Условие, получающее элемент и его текущую логическую
    /// позицию. Должно быть ненулевым; защитная проверка не выполняется.</param>
    /// <returns>Индекс первого совпадения либо -1, если совпадение отсутствует.</returns>
    /// <exception cref="InvalidOperationException">Условие изменило список и вернуло результат.</exception>
    /// <remarks>
    /// Проверяет элементы от начала к концу, исключая резерв. Условие вызывается один раз
    /// для каждой посещённой позиции; после первого совпадения поиск прекращается.
    /// Чтение и вложенный поиск разрешены и сохраняют состояние перечислителей.
    /// После каждого нормально завершившегося вызова условия проверяется версия списка;
    /// изменения обнаруживаются, но не откатываются. Исключения условия передаются
    /// вызывающему коду. Поведение при нарушении предусловия ненулевого условия
    /// не гарантируется. Сложность — O(n) при постоянной стоимости условия,
    /// дополнительная память — O(1).
    /// </remarks>
    public int IndexOf(IndexedPredicate<T> indexedPredicate)
    {
        var versionSnapshot = _version;

        for (var i = 0; i < Count; i++)
        {
            var indexedPredicateResult = indexedPredicate(_array[PhysicalIndex(i)], i);
            if (versionSnapshot != _version)
                throw new InvalidOperationException("Collection was modified; search cannot continue.");

            if (indexedPredicateResult)
                return i;
        }

        return -1;
    }

    /// <summary>Проверяет наличие элемента, удовлетворяющего условию.</summary>
    /// <param name="indexedPredicate">Ненулевое условие, получающее элемент и его логическую позицию.</param>
    /// <returns>True, если элемент найден; иначе false.</returns>
    /// <exception cref="InvalidOperationException">Условие изменило список и вернуло результат.</exception>
    /// <remarks>
    /// Соблюдает контракт <see cref="IndexOf"/>: порядок проверки, остановку на первом
    /// совпадении, обнаружение изменений и передачу исключений. Аргумент на null
    /// не проверяется; при нарушении предусловия поведение не гарантируется.
    /// Сложность — O(n) при постоянной стоимости условия, дополнительная память — O(1).
    /// </remarks>
    public bool Contains(IndexedPredicate<T> indexedPredicate) => IndexOf(indexedPredicate) != -1;

    private int NextIndex(int i) => i == Capacity - 1 ? 0 : i + 1;

    private int PrevIndex(int i) => i == 0 ? Capacity - 1 : i - 1;

    private int PhysicalIndex(int i) => i < Capacity - _headIndex ? _headIndex + i : i - (Capacity - _headIndex);

    /// <summary>Создаёт независимый перечислитель элементов в логическом порядке.</summary>
    /// <returns>Перечислитель, расположенный перед первым элементом.</returns>
    /// <remarks>
    /// <para>
    /// Получение перечислителя — O(1), полный обход — O(n); копия содержимого не создаётся.
    /// Версия фиксируется при получении перечислителя. Успешные Add, Insert, RemoveAt,
    /// запись через индексатор, даже тем же значением, и каждый Clear, включая очистку
    /// пустого списка, инвалидируют ранее полученные перечислители.
    /// Их Current и MoveNext выбрасывают InvalidOperationException, в том числе до
    /// начала, после завершения и после Dispose. Чтение, поиск и отклонённые операции
    /// с недопустимым индексом сохраняют работоспособность перечислителей.
    /// </para>
    /// <para>
    /// Current до первого успешного шага, после конца или Dispose выбрасывает
    /// InvalidOperationException. Dispose завершает обход; повторный вызов безопасен.
    /// Reset всегда выбрасывает NotSupportedException; для нового обхода нужно
    /// получить новый перечислитель.
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

    /// <summary>Хранит логическую позицию обхода и версию списка на момент создания.</summary>
    private sealed class Enumerator : IEnumerator<T>
    {
        private readonly CircularArrayList<T> _owner;
        private readonly int _expectedVersion;
        private int _index = -1;

        internal Enumerator(CircularArrayList<T> owner)
        {
            _owner = owner;
            _expectedVersion = owner._version;
        }

        /// <summary>Переходит к следующему логическому элементу.</summary>
        /// <returns>True, если следующий элемент существует; иначе false.</returns>
        /// <exception cref="InvalidOperationException">Версия списка изменилась.</exception>
        public bool MoveNext()
        {
            ValidateVersion();
            if (_index < _owner.Count)
                _index++;

            return _index < _owner.Count;
        }

        /// <summary>Возврат в начальное состояние не поддерживается.</summary>
        /// <exception cref="NotSupportedException">Выбрасывается при каждом вызове.</exception>
        public void Reset()
        {
            throw new NotSupportedException("Reset is not supported. Obtain a new enumerator instead.");
        }

        /// <summary>Получает текущий элемент после проверки версии и позиции.</summary>
        /// <exception cref="InvalidOperationException">Список изменился либо перечислитель
        /// не расположен на элементе.</exception>
        public T Current
        {
            get
            {
                ValidateVersion();
                if (_index < 0 || _index >= _owner.Count)
                    throw new InvalidOperationException("Enumeration has not started or has already finished.");

                return _owner._array[_owner.PhysicalIndex(_index)];
            }
        }

        /// <inheritdoc cref="Current"/>
        object? IEnumerator.Current => Current;

        /// <summary>Завершает обход; повторное освобождение безопасно.</summary>
        public void Dispose()
        {
            _index = _owner.Count;
        }

        private void ValidateVersion()
        {
            if (_expectedVersion != _owner._version)
                throw new InvalidOperationException("Collection was modified; enumeration cannot continue.");
        }
    }
}
