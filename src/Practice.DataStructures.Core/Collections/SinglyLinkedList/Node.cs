namespace Practice.DataStructures.Core.Collections;

public sealed partial class SinglyLinkedList<T>
{
    internal sealed class Node
    {
        public required T Value { get; set; }

        public required Node? Next { get; set; }

        public bool IsAttached { get; set; } = true;
    }
}
