using System.Collections;
using Practice.DataStructures.Core.Collections;
using Xunit;

namespace Practice.DataStructures.Tests.Collections.ArrayListContract;

public abstract class ArrayListContractTests
{
    protected abstract ListSubject<T> CreateList<T>(int capacity);

    protected sealed class ListSubject<T>(
        Func<int> count, Func<int> capacity, Func<int, T> get, Action<int, T> set,
        Action<T> add, Action<int, T> insert, Action<int> removeAt, Action clear,
        Func<IndexedPredicate<T>, int> indexOf, Func<IndexedPredicate<T>, bool> contains,
        IEnumerable<T> items) : IEnumerable<T>
    {
        public int Count => count();
        public int Capacity => capacity();
        public T this[int index] { get => get(index); set => set(index, value); }
        public void Add(T item) => add(item);
        public void Insert(int index, T item) => insert(index, item);
        public void RemoveAt(int index) => removeAt(index);
        public void Clear() => clear();
        public int IndexOf(IndexedPredicate<T> predicate) => indexOf(predicate);
        public bool Contains(IndexedPredicate<T> predicate) => contains(predicate);
        public IEnumerator<T> GetEnumerator() => items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)items).GetEnumerator();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void Constructor_InvalidCapacity_Throws(int capacity)
    {
        // Arrange / Act / Assert
        Assert.Throws<ArgumentOutOfRangeException>(nameof(capacity), () => CreateList<int>(capacity));
    }

    [Theory]
    [InlineData(1, 12345)]
    [InlineData(3, 781)]
    [InlineData(8, 193)]
    public void Operations_DeterministicSequence_MatchesReferenceAfterEveryStep(int capacity, int seed)
    {
        // Arrange
        ListSubject<int> list = CreateList<int>(capacity);
        var expected = new List<int>();
        var random = new Random(seed);
        Assert.Equal(capacity, list.Capacity);
        Assert.Equal(0, list.Count);

        // Act / Assert
        for (int step = 0; step < 3000; step++)
        {
            int previousCapacity = list.Capacity;
            int value = random.Next(-9, 10);
            int operation = random.Next(10);
            if (operation < 3 || expected.Count == 0)
            {
                int position = operation switch
                {
                    0 => 0,
                    1 => expected.Count,
                    _ => random.Next(expected.Count + 1)
                };
                list.Insert(position, value);
                expected.Insert(position, value);
            }
            else if (operation == 3)
            {
                list.Add(value);
                expected.Add(value);
            }
            else if (operation < 7)
            {
                int position = operation switch
                {
                    4 => 0,
                    5 => expected.Count - 1,
                    _ => random.Next(expected.Count)
                };
                list.RemoveAt(position);
                expected.RemoveAt(position);
                Assert.Equal(previousCapacity, list.Capacity);
            }
            else if (operation < 9)
            {
                int position = random.Next(expected.Count);
                list[position] = value;
                expected[position] = value;
                Assert.Equal(previousCapacity, list.Capacity);
            }
            else if (step % 71 == 0)
            {
                list.Clear();
                expected.Clear();
                Assert.Equal(previousCapacity, list.Capacity);
            }

            Assert.Equal(expected.Count, list.Count);
            Assert.True(list.Capacity >= list.Count);
            Assert.Equal(expected, list);
            for (int position = 0; position < expected.Count; position++)
            {
                Assert.Equal(expected[position], list[position]);
            }

            Assert.Equal(expected.IndexOf(value), list.IndexOf((item, _) => item == value));
            Assert.Equal(expected.Contains(value), list.Contains((item, _) => item == value));
            Assert.Equal(expected.Count > 0 ? expected.Count - 1 : -1,
                list.IndexOf((_, position) => position == expected.Count - 1));
        }
    }

    [Theory]
    [InlineData(0, -1)]
    [InlineData(0, 1)]
    [InlineData(3, -1)]
    [InlineData(3, 4)]
    [InlineData(3, int.MinValue)]
    [InlineData(3, int.MaxValue)]
    public void Operations_InvalidIndices_PreserveStateAndTraversal(int count, int index)
    {
        // Arrange
        ListSubject<int> list = CreateList<int>(Math.Max(1, count));
        int[] expected = [.. Enumerable.Range(10, count)];
        foreach (int item in expected)
        {
            list.Add(item);
        }

        int capacity = list.Capacity;
        using IEnumerator<int> iterator = list.GetEnumerator();

        // Act / Assert
        Assert.Throws<ArgumentOutOfRangeException>(nameof(index), () => list.Insert(index, 99));
        Assert.Throws<ArgumentOutOfRangeException>(nameof(index), () => list.RemoveAt(index));
        Assert.Throws<ArgumentOutOfRangeException>(nameof(index), () => list[index] = 99);
        Assert.Throws<ArgumentOutOfRangeException>(nameof(index), () => list[index]);
        Assert.Throws<ArgumentOutOfRangeException>(nameof(index), () => list.RemoveAt(count));
        Assert.Throws<ArgumentOutOfRangeException>(nameof(index), () => list[count] = 99);
        Assert.Throws<ArgumentOutOfRangeException>(nameof(index), () => list[count]);
        Assert.Equal(count, list.Count);
        Assert.Equal(capacity, list.Capacity);
        Assert.Equal(expected, list);
        foreach (int item in expected)
        {
            Assert.True(iterator.MoveNext());
            Assert.Equal(item, iterator.Current);
        }

        Assert.False(iterator.MoveNext());
    }

    [Fact]
    public void Operations_NullAndDuplicates_PreserveReferenceIdentityAndAllowReuse()
    {
        // Arrange
        ListSubject<object?> list = CreateList<object?>(1);
        object value = new();

        // Act
        list.Add(value);
        list.Insert(0, null);
        list.Insert(1, value);
        list.RemoveAt(0);
        list.Insert(list.Count, null);

        // Assert
        Assert.Equal(3, list.Count);
        Assert.Same(value, list[0]);
        Assert.Same(value, list[1]);
        Assert.Null(list[2]);
        Assert.Equal(0, list.IndexOf((item, _) => ReferenceEquals(item, value)));
        Assert.True(list.Contains((item, position) => item is null && position == 2));
        int capacity = list.Capacity;
        list.Clear();
        Assert.Empty(list);
        Assert.Equal(capacity, list.Capacity);
        list.Insert(0, value);
        Assert.Same(value, Assert.Single(list));
    }

    [Theory]
    [InlineData("Add")]
    [InlineData("Insert")]
    [InlineData("RemoveAt")]
    [InlineData("Replace")]
    [InlineData("Clear")]
    public void Mutation_ExistingEnumerators_InvalidatesAllStates(string operation)
    {
        // Arrange
        ListSubject<int> list = CreateList<int>(2);
        list.Add(10);
        list.Add(20);
        using IEnumerator<int> beforeFirst = list.GetEnumerator();
        using IEnumerator<int> active = list.GetEnumerator();
        using IEnumerator<int> completed = list.GetEnumerator();
        using IEnumerator<int> disposed = list.GetEnumerator();
        Assert.True(active.MoveNext());
        while (completed.MoveNext())
        {
        }

        disposed.Dispose();

        // Act
        switch (operation)
        {
            case "Add": list.Add(30); break;
            case "Insert": list.Insert(0, 30); break;
            case "RemoveAt": list.RemoveAt(0); break;
            case "Replace": list[0] = list[0]; break;
            case "Clear": list.Clear(); break;
        }

        // Assert
        foreach (IEnumerator<int> iterator in new[] { beforeFirst, active, completed, disposed })
        {
            Assert.Throws<InvalidOperationException>(() => iterator.MoveNext());
            Assert.Throws<InvalidOperationException>(() => iterator.Current);
            Assert.Throws<InvalidOperationException>(() => ((IEnumerator)iterator).Current);
        }
    }

    [Fact]
    public void Enumerator_Lifecycle_SupportsIndependentAndNonGenericTraversal()
    {
        // Arrange
        ListSubject<int> list = CreateList<int>(3);
        list.Add(10);
        list.Add(20);
        using IEnumerator<int> iterator = list.GetEnumerator();
        var nonGenericItems = new List<int>();

        // Act / Assert
        Assert.Throws<InvalidOperationException>(() => iterator.Current);
        foreach (object? item in (IEnumerable)list)
        {
            nonGenericItems.Add(Assert.IsType<int>(item));
        }

        Assert.Equal(new[] { 10, 20 }, nonGenericItems);
        foreach (int outer in list)
        {
            foreach (int inner in list)
            {
                Assert.Contains(outer + inner, new[] { 20, 30, 40 });
            }
        }

        Assert.True(iterator.MoveNext());
        Assert.Equal(10, iterator.Current);
        Assert.Equal(0, list.IndexOf((value, _) => value == 10));
        Assert.True(list.Contains((_, position) => position == 1));
        Assert.True(iterator.MoveNext());
        Assert.Equal(20, ((IEnumerator)iterator).Current);
        Assert.False(iterator.MoveNext());
        Assert.False(iterator.MoveNext());
        Assert.Throws<InvalidOperationException>(() => iterator.Current);
        Assert.Throws<NotSupportedException>(iterator.Reset);
        iterator.Dispose();
        iterator.Dispose();
        Assert.False(iterator.MoveNext());
        Assert.Throws<InvalidOperationException>(() => iterator.Current);
    }

    [Fact]
    public void Clear_EmptyList_InvalidatesEnumeratorAndKeepsCapacity()
    {
        // Arrange
        ListSubject<int> list = CreateList<int>(3);
        using IEnumerator<int> iterator = list.GetEnumerator();
        Assert.Throws<InvalidOperationException>(() => iterator.Current);
        Assert.False(iterator.MoveNext());
        Assert.Throws<InvalidOperationException>(() => iterator.Current);

        // Act
        list.Clear();

        // Assert
        Assert.Equal(0, list.Count);
        Assert.Equal(3, list.Capacity);
        Assert.Throws<InvalidOperationException>(() => iterator.MoveNext());
        Assert.Throws<InvalidOperationException>(() => iterator.Current);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Search_PredicateMutation_IsDetectedWithoutRollback(bool contains)
    {
        // Arrange
        ListSubject<int> list = CreateList<int>(1);
        list.Add(10);
        int calls = 0;
        bool Mutate(int _, int position)
        {
            calls++;
            list.Insert(position, 20);
            return true;
        }

        // Act / Assert
        if (contains)
        {
            Assert.Throws<InvalidOperationException>(() => list.Contains(Mutate));
        }
        else
        {
            Assert.Throws<InvalidOperationException>(() => list.IndexOf(Mutate));
        }

        Assert.Equal(1, calls);
        Assert.Equal(new[] { 20, 10 }, list);
    }

    [Fact]
    public void Search_PredicateVisitsAndExceptions_RespectLogicalOrderAndStopAtFirstMatch()
    {
        // Arrange
        ListSubject<int> list = CreateList<int>(4);
        list.Add(10);
        list.Add(20);
        list.Add(20);
        var visited = new List<(int Value, int Position)>();
        var failure = new InvalidOperationException("Predicate failure.");

        // Act
        int found = list.IndexOf((value, position) =>
        {
            visited.Add((value, position));
            Assert.True(list.Contains((item, _) => item == value));
            return value == 20;
        });

        // Assert
        Assert.Equal(1, found);
        Assert.Equal(new[] { (10, 0), (20, 1) }, visited);
        Assert.Same(failure, Record.Exception(() => list.Contains((_, _) => throw failure)));
        Assert.Equal(new[] { 10, 20, 20 }, list);
    }
}
