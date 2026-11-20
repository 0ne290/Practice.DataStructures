namespace Practice.DataStructures.Core.Trees;

public sealed class BinarySearchTree<TKey, TValue>
{
    private readonly IComparer<TKey> _comparer;

    private Node? _root;

    public int Count { get; private set; }

    public int Depth { get; private set; }

    public BinarySearchTree(IComparer<TKey>? comparer = null)
    {
        _comparer = comparer ?? Comparer<TKey>.Default;
        _root = null;
        Count = 0;
        Depth = 0;
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
                throw new NotImplementedException(); // TODO Codex: идея в том, что я хочу запретить дубликаты в дереве, но я не уверен, что я выбрал самый эффективный способ для этого

            if (comparisonResult < 0)
                target = ref target.Left;
            else
                target = ref target.Right;

            depth++;
        }

        target = new Node { Key = key, Value = value };

        Count++;
        if (depth > Depth)
            Depth = depth;
    }

    internal sealed class Node
    {
        internal required TKey Key;
        internal required TValue Value;
        internal Node? Left;
        internal Node? Right;
    }
}
