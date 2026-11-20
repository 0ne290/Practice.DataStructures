using Practice.DataStructures.Core.Trees;
using Xunit;

namespace Practice.DataStructures.Tests.Collections.BinarySearchTree;

public sealed class BinarySearchTreeLaboratoryTests
{
    [Fact]
    [Trait("Category", "Laboratory")]
    public void Explore_AuthorExperiments_Run()
    {
        // Полигон автора: здесь можно вызывать свою реализацию и исследовать результаты.

        var tree = new BinarySearchTree<int, int>();
        tree.Add(1, 1);

        tree.Add(3, 3);
        tree.Add(-1, -1);

        tree.Add(2, 2);
        tree.Add(4, 4);

        tree.Add(0, 0);
        tree.Add(-2, -2);

        tree.Add(2, 2);
    }
}
