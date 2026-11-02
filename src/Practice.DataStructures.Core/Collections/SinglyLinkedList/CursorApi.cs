namespace Practice.DataStructures.Core.Collections;

public sealed partial class SinglyLinkedList<T>
{
    /// <summary>Создаёт независимые курсоры; узлы и связи пользователю не доступны.</summary>
    public sealed class CursorApi
    {
        private readonly SinglyLinkedList<T> _owner;

        internal CursorApi(SinglyLinkedList<T> owner)
        {
            _owner = owner;
        }

        /// <summary>Курсор перед головой, в том числе пустого списка, O(1). Значения в этой позиции нет.</summary>
        public Cursor BeforeFirst => new(_owner, null);

        /// <summary>Новый курсор на голову либо null, O(1).</summary>
        public Cursor? First => _owner._head is null ? null : new Cursor(_owner, _owner._head);

        /// <summary>Новый курсор на хвост либо null, O(1).</summary>
        public Cursor? Last => _owner._tail is null ? null : new Cursor(_owner, _owner._tail);

        /// <summary>Находит первое совпадение от головы, O(n); возвращает курсор либо null.</summary>
        /// <remarks>Условие и обнаружение изменений подчиняются правилам Values.TryFindFirst.</remarks>
        public Cursor? FindFirst(IndexedPredicate<T> indexedPredicate)
        {
            var (_, node, _) = _owner._find(indexedPredicate);
            return node is null ? null : new Cursor(_owner, node);
        }
    }
}
