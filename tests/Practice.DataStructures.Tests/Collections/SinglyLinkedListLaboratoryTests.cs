using Xunit;

namespace Practice.DataStructures.Tests.Collections;

public sealed class SinglyLinkedListLaboratoryTests(ITestOutputHelper output)
{
    [Fact]
    [Trait("Category", "Laboratory")]
    public void SinglyLinkedList_Playground_RunsAuthorExperiments()
    {
        // Полигон автора: здесь можно вызывать свою реализацию и ставить точки останова.
        output.WriteLine("SinglyLinkedList<T>: полигон для самостоятельных экспериментов.");
    }
}
