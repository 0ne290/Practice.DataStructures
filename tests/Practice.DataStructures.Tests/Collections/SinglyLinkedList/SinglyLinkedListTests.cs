using Practice.DataStructures.Core.Collections;
using Xunit;

namespace Practice.DataStructures.Tests.Collections.SinglyLinkedList;

public sealed class SinglyLinkedListTests
{
    [Fact]
    public void Constructor_Empty_ReturnsMissingValues()
    {
        // Arrange / Act
        var list = new SinglyLinkedList<int>();

        // Assert
        Assert.Equal(0, list.Count);
        Assert.Empty(list);
        Assert.False(list.TryFirst(out int first));
        Assert.False(list.TryLast(out int last));
        Assert.False(list.TryFindFirst((_, _) => true, out int found));
        Assert.Equal(0, first);
        Assert.Equal(0, last);
        Assert.Equal(0, found);
        Assert.Equal(-1, list.PositionOf((_, _) => true));
        Assert.False(list.Contains((_, _) => true));
    }

    [Fact]
    public void AddFirstAndLast_MixedOrder_PreserveEndpoints()
    {
        // Arrange
        var list = new SinglyLinkedList<int>();

        // Act
        list.AddFirst(2);
        list.AddFirst(1);
        list.AddLast(3);

        // Assert
        Assert.Equal(new[] { 1, 2, 3 }, list);
        Assert.Equal(3, list.Count);
        Assert.True(list.TryFirst(out int first));
        Assert.True(list.TryLast(out int last));
        Assert.Equal(1, first);
        Assert.Equal(3, last);
    }

    [Theory]
    [InlineData(true, 0)]
    [InlineData(true, 1)]
    [InlineData(true, 2)]
    [InlineData(false, 0)]
    [InlineData(false, 1)]
    [InlineData(false, 2)]
    public void Insert_AtSelectedPosition_PreservesOrderAndTail(bool before, int position)
    {
        // Arrange
        var list = new SinglyLinkedList<int>();
        var expected = new List<int> { 1, 2, 3 };
        foreach (int value in expected)
        {
            list.AddLast(value);
        }

        // Act
        bool inserted = before
            ? list.AddBefore((_, index) => index == position, 9)
            : list.AddAfter((_, index) => index == position, 9);
        expected.Insert(before ? position : position + 1, 9);
        list.AddLast(10);
        expected.Add(10);

        // Assert
        Assert.True(inserted);
        Assert.Equal(expected, list);
        Assert.Equal(expected.Count, list.Count);
        Assert.True(list.TryFirst(out int first));
        Assert.True(list.TryLast(out int last));
        Assert.Equal(expected[0], first);
        Assert.Equal(10, last);
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(3, 0)]
    [InlineData(3, 1)]
    [InlineData(3, 2)]
    public void RemoveFirst_SelectedPosition_PreservesRemainingChain(int count, int position)
    {
        // Arrange
        var list = new SinglyLinkedList<int>();
        var expected = Enumerable.Range(1, count).ToList();
        foreach (int value in expected)
        {
            list.AddLast(value);
        }

        // Act
        bool removed = list.RemoveFirst((_, index) => index == position);
        expected.RemoveAt(position);

        // Assert
        Assert.True(removed);
        Assert.Equal(expected, list);
        Assert.Equal(expected.Count, list.Count);
        Assert.Equal(expected.Count != 0, list.TryFirst(out int first));
        Assert.Equal(expected.Count != 0, list.TryLast(out int last));
        Assert.Equal(expected.FirstOrDefault(), first);
        Assert.Equal(expected.LastOrDefault(), last);
        list.AddLast(9);
        expected.Add(9);
        Assert.Equal(expected, list);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    public void Clear_AnyCount_AllowsReuse(int count)
    {
        // Arrange
        var list = new SinglyLinkedList<int>();
        for (int index = 0; index < count; index++)
        {
            list.AddLast(index);
        }

        // Act
        list.Clear();

        // Assert
        Assert.Empty(list);
        Assert.Equal(0, list.Count);
        Assert.False(list.TryFirst(out _));
        Assert.False(list.TryLast(out _));
        list.AddFirst(7);
        list.AddLast(8);
        Assert.Equal(new[] { 7, 8 }, list);
    }

    [Fact]
    public void Mutations_FixedSeed_MatchReferenceSequence()
    {
        // Arrange
        var random = new Random(7319);
        var list = new SinglyLinkedList<int>();
        var expected = new List<int>();

        // Act / Assert: each operation is checked against an independent BCL sequence.
        for (int step = 0; step < 500; step++)
        {
            int value = random.Next(-5, 6);
            int position = random.Next(expected.Count + 1);
            bool exists = position < expected.Count;
            switch (random.Next(7))
            {
                case 0:
                    list.AddFirst(value);
                    expected.Insert(0, value);
                    break;
                case 1:
                    list.AddLast(value);
                    expected.Add(value);
                    break;
                case 2:
                    Assert.Equal(exists, list.AddBefore((_, index) => index == position, value));
                    if (exists)
                    {
                        expected.Insert(position, value);
                    }
                    break;
                case 3:
                    Assert.Equal(exists, list.AddAfter((_, index) => index == position, value));
                    if (exists)
                    {
                        expected.Insert(position + 1, value);
                    }
                    break;
                case 4:
                    Assert.Equal(exists, list.RemoveFirst((_, index) => index == position));
                    if (exists)
                    {
                        expected.RemoveAt(position);
                    }
                    break;
                case 5:
                    Assert.Equal(exists, list.ReplaceFirst((_, index) => index == position, value));
                    if (exists)
                    {
                        expected[position] = value;
                    }
                    break;
                default:
                    list.Clear();
                    expected.Clear();
                    break;
            }

            Assert.Equal(expected, list);
            Assert.Equal(expected.Count, list.Count);
            Assert.Equal(expected.Count > 0, list.TryFirst(out int first));
            Assert.Equal(expected.Count > 0, list.TryLast(out int last));
            Assert.Equal(expected.FirstOrDefault(), first);
            Assert.Equal(expected.LastOrDefault(), last);
        }
    }
}
