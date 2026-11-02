using System.Diagnostics.CodeAnalysis;

namespace Practice.DataStructures.Core.Collections;

public sealed partial class SinglyLinkedList<T>
{
    /// <summary>Сохранённая позиция: движение вперёд и редактирование после неё за O(1).</summary>
    /// <remarks>
    /// Курсоры независимы и сохраняются при изменении других элементов через любое API.
    /// Удаление выбранного элемента или Clear делают курсор недействительным.
    /// Позиция BeforeFirst сохраняется при удалении головы, но не при очистке.
    /// Узлы, предшественник и числовая позиция не предоставляются.
    /// </remarks>
    public sealed class Cursor
    {
        private readonly SinglyLinkedList<T> _owner;
        private readonly object _generation;
        private Node? _node;

        internal Cursor(SinglyLinkedList<T> owner, Node? node)
        {
            _owner = owner;
            _generation = owner._generation;
            _node = node;
        }

        /// <summary>Читает или заменяет значение за O(1). В BeforeFirst выбрасывает InvalidOperationException.</summary>
        /// <remarks>Запись инвалидирует перечислители, но сохраняет курсоры на этом элементе.</remarks>
        public T Value
        {
            get
            {
                Validate();
                return GetNode().Value;
            }
            set
            {
                Validate();
                _owner.SetNodeValue(GetNode(), value);
            }
        }

        /// <summary>Переходит вперёд за O(1); если следующего элемента нет, сохраняет позицию и возвращает false.</summary>
        public bool MoveNext()
        {
            Validate();
            var next = _node is null ? _owner._head : _node.Next;
            if (next is null)
                return false;

            _node = next;
            return true;
        }

        /// <summary>Вставляет после позиции за O(1); в BeforeFirst вставляет голову. Позиция сохраняется.</summary>
        public void InsertAfter(T item)
        {
            Validate();
            _owner.InsertAfterNode(_node, item);
        }

        /// <summary>Удаляет следующий элемент за O(1), сохраняя курсор; в BeforeFirst удаляет голову.</summary>
        /// <returns>True при удалении; false, если следующего элемента нет, с default в value.</returns>
        public bool TryRemoveAfter([MaybeNullWhen(false)] out T value)
        {
            Validate();
            var target = _node is null ? _owner._head : _node.Next;
            if (target is null)
            {
                value = default;
                return false;
            }

            value = target.Value;
            _owner.RemoveAfterNode(_node, target);
            return true;
        }

        private Node GetNode()
        {
            return _node ?? throw new InvalidOperationException("The cursor is positioned before the first element.");
        }

        private void Validate()
        {
            if (_generation != _owner._generation || _node is { IsAttached: false })
                throw new InvalidOperationException("The cursor no longer refers to a position in this list.");
        }
    }
}
