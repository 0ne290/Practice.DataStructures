namespace Practice.DataStructures.Core.Collections;

public sealed class CircularArray<T>
{
    private T[] _array;
    private int _version;

    private int _headIndex;
    private int _tailIndex;

    public int Count { get; private set; }
    public int Capacity { get; private set; }

    public CircularArray(int capacity)
    {
        if (capacity < 1)
            throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "Capacity must be greater than zero.");

        _array = new T[capacity];
        _version = 0;

        _headIndex = 0;
        _tailIndex = 0;

        Count = 0;
        Capacity = capacity;
    }
    public CircularArray() : this(8) { }

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

    public void AddLast(T item)
    {
        if (Count >= Capacity)
        {
            var newCapacity = Capacity * 2;
            var newArray = new T[newCapacity];
            _array.CopyTo(newArray, 0);
            _array = newArray;
            Capacity = newCapacity;
        }

        _array[_tailIndex + 1] = item;
        _tailIndex++;
        Count++;
        _version = unchecked(_version + 1);
    }
}
