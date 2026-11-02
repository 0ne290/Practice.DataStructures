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
        Assert.False(list.Values.TryFirst(out int first));
        Assert.False(list.Values.TryLast(out int last));
        Assert.False(list.Values.TryFindFirst((_, _) => true, out int found));
        Assert.Equal(0, first);
        Assert.Equal(0, last);
        Assert.Equal(0, found);
        Assert.Equal(-1, list.Values.PositionOf((_, _) => true));
        Assert.False(list.Values.Contains((_, _) => true));
    }

    [Fact]
    public void AddFirstAndLast_MixedOrder_PreserveEndpoints()
    {
        // Arrange
        var list = new SinglyLinkedList<int>();

        // Act
        list.Values.AddFirst(2);
        list.Values.AddFirst(1);
        list.Values.AddLast(3);

        // Assert
        Assert.Equal(new[] { 1, 2, 3 }, list);
        Assert.Equal(3, list.Count);
        Assert.True(list.Values.TryFirst(out int first));
        Assert.True(list.Values.TryLast(out int last));
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
            list.Values.AddLast(value);
        }

        // Act
        bool inserted = before
            ? list.Values.AddBefore((_, index) => index == position, 9)
            : list.Values.AddAfter((_, index) => index == position, 9);
        expected.Insert(before ? position : position + 1, 9);
        list.Values.AddLast(10);
        expected.Add(10);

        // Assert
        Assert.True(inserted);
        Assert.Equal(expected, list);
        Assert.Equal(expected.Count, list.Count);
        Assert.True(list.Values.TryFirst(out int first));
        Assert.True(list.Values.TryLast(out int last));
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
            list.Values.AddLast(value);
        }

        // Act
        bool removed = list.Values.RemoveFirst((_, index) => index == position);
        expected.RemoveAt(position);

        // Assert
        Assert.True(removed);
        Assert.Equal(expected, list);
        Assert.Equal(expected.Count, list.Count);
        Assert.Equal(expected.Count != 0, list.Values.TryFirst(out int first));
        Assert.Equal(expected.Count != 0, list.Values.TryLast(out int last));
        Assert.Equal(expected.FirstOrDefault(), first);
        Assert.Equal(expected.LastOrDefault(), last);
        list.Values.AddLast(9);
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
            list.Values.AddLast(index);
        }

        // Act
        list.Values.Clear();

        // Assert
        Assert.Empty(list);
        Assert.Equal(0, list.Count);
        Assert.False(list.Values.TryFirst(out _));
        Assert.False(list.Values.TryLast(out _));
        list.Values.AddFirst(7);
        list.Values.AddLast(8);
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
                    list.Values.AddFirst(value);
                    expected.Insert(0, value);
                    break;
                case 1:
                    list.Values.AddLast(value);
                    expected.Add(value);
                    break;
                case 2:
                    Assert.Equal(exists, list.Values.AddBefore((_, index) => index == position, value));
                    if (exists)
                    {
                        expected.Insert(position, value);
                    }
                    break;
                case 3:
                    Assert.Equal(exists, list.Values.AddAfter((_, index) => index == position, value));
                    if (exists)
                    {
                        expected.Insert(position + 1, value);
                    }
                    break;
                case 4:
                    Assert.Equal(exists, list.Values.RemoveFirst((_, index) => index == position));
                    if (exists)
                    {
                        expected.RemoveAt(position);
                    }
                    break;
                case 5:
                    Assert.Equal(exists, list.Values.ReplaceFirst((_, index) => index == position, value));
                    if (exists)
                    {
                        expected[position] = value;
                    }
                    break;
                default:
                    list.Values.Clear();
                    expected.Clear();
                    break;
            }

            Assert.Equal(expected, list);
            Assert.Equal(expected.Count, list.Count);
            Assert.Equal(expected.Count > 0, list.Values.TryFirst(out int first));
            Assert.Equal(expected.Count > 0, list.Values.TryLast(out int last));
            Assert.Equal(expected.FirstOrDefault(), first);
            Assert.Equal(expected.LastOrDefault(), last);
        }
    }
}
