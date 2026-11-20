namespace Practice.DataStructures.Core.Trees;

public sealed class BinarySearchTree<TKey, TValue>
{
    private readonly IComparer<TKey> _comparer;

    private Node? _root;

    public int Count { get; private set; }

    public int Height { get; private set; }

    public BinarySearchTree(IComparer<TKey>? comparer = null)
    {
        _comparer = comparer ?? Comparer<TKey>.Default;
        _root = null;
        Count = 0;
        Height = 0;
    }

    public void Add(TKey key, TValue value)
    {
        ref var target = ref _root;
        var depth = 1;

        while (target != null)
        {
            var comparisonResult = _comparer.Compare(key, target.Key);
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            // P. S. ебаный Rider советует использовать ref switch, который не поддерживается компилятором, лол
            if (comparisonResult == 0)
                throw new ArgumentException("An element with the same key already exists in the tree.", nameof(key));

            if (comparisonResult < 0)
                target = ref target.Left;
            else
                target = ref target.Right;

            depth++;
        }

        target = new Node { Key = key, Value = value };

        Count++;
        if (depth > Height)
            Height = depth;
    }

    internal sealed class Node
    {
        internal required TKey Key;
        internal required TValue Value;
        internal Node? Left;
        internal Node? Right;
    }
}
