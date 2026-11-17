using Practice.DataStructures.Core.Collections;

namespace Practice.DataStructures.Tests.Collections.ArrayListContract;

public sealed class ContiguousArrayListContractTests : ArrayListContractTests
{
    protected override ListSubject<T> CreateList<T>(int capacity)
    {
        var list = new ArrayList<T>(capacity);
        return new ListSubject<T>(() => list.Count, () => list.Capacity,
            index => list[index], (index, value) => list[index] = value,
            list.Add, list.Insert, list.RemoveAt, list.Clear, list.IndexOf, list.Contains, list);
    }
}
