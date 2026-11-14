using Xunit;

namespace Practice.DataStructures.Tests.Collections.CircularArray;

public sealed class CircularArrayLaboratoryTests(ITestOutputHelper output)
{
    [Fact]
    [Trait("Category", "Laboratory")]
    public void CircularArray_AuthorExperiments_RunsLaboratory()
    {
        // Полигон автора: здесь можно вызывать свою реализацию и проверять результаты.
        output.WriteLine("Полигон расширяемого кольцевого массива готов к экспериментам автора.");
    }
}
