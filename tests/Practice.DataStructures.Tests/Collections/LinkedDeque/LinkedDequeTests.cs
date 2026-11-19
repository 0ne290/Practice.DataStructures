using Practice.DataStructures.Tests.Collections.DequeContract;

namespace Practice.DataStructures.Tests.Collections.LinkedDeque;

public sealed class LinkedDequeTests : DequeEnumerationContractTests
{
    private protected override DequeSubject<T> CreateDeque<T>() => DequeSubject<T>.CreateLinked();
}
