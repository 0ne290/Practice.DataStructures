using System.Diagnostics.CodeAnalysis;

namespace Practice.DataStructures.Core.Collections;

public sealed partial class SinglyLinkedList<T>
{
    /// <summary>Прежнее API списка: операции над значениями и поиск по условию.</summary>
    public sealed class ValueApi
    {
        private readonly SinglyLinkedList<T> _owner;

        internal ValueApi(SinglyLinkedList<T> owner)
        {
            _owner = owner;
        }

        /// <summary>Добавляет элемент в начало за O(1) и инвалидирует перечислители.</summary>
        /// <param name="item">Добавляемое значение.</param>
        public void AddFirst(T item)
        {
            _owner.InsertAfterNode(null, item);
        }

        /// <summary>Добавляет элемент в конец за O(1) и инвалидирует перечислители.</summary>
        /// <param name="item">Добавляемое значение.</param>
        public void AddLast(T item)
        {
            _owner.InsertAfterNode(_owner._tail, item);
        }

        /// <summary>Вставляет значение после первого совпадения за O(n).</summary>
        /// <param name="indexedPredicate">Ненулевое условие выбора элемента и позиции.</param>
        /// <param name="item">Добавляемое значение.</param>
        /// <returns>True при вставке; false при отсутствии совпадения.</returns>
        /// <remarks>Успешная вставка инвалидирует перечислители; отсутствие совпадения сохраняет их.</remarks>
        /// <exception cref="InvalidOperationException">Условие изменило список и вернуло результат.</exception>
        public bool AddAfter(IndexedPredicate<T> indexedPredicate, T item)
        {
            var (_, targetNode, _) = _owner._find(indexedPredicate);
            if (targetNode is null)
                return false;

            _owner.InsertAfterNode(targetNode, item);
            return true;
        }

        /// <summary>Вставляет значение перед первым совпадением за O(n).</summary>
        /// <param name="indexedPredicate">Ненулевое условие выбора элемента и позиции.</param>
        /// <param name="item">Добавляемое значение.</param>
        /// <returns>True при вставке; false при отсутствии совпадения.</returns>
        /// <remarks>Успешная вставка инвалидирует перечислители; отсутствие совпадения сохраняет их.</remarks>
        /// <exception cref="InvalidOperationException">Условие изменило список и вернуло результат.</exception>
        public bool AddBefore(IndexedPredicate<T> indexedPredicate, T item)
        {
            var (previousNode, targetNode, _) = _owner._find(indexedPredicate);
            if (targetNode is null)
                return false;

            _owner.InsertAfterNode(previousNode, item);
            return true;
        }

        /// <summary>Пытается прочитать голову за O(1), сохраняя состояние списка и перечислителей.</summary>
        /// <param name="value">Значение головы при успехе; default при пустом списке.</param>
        /// <returns>True, если элемент существует, включая хранимый null или default.</returns>
        public bool TryFirst([MaybeNullWhen(false)] out T value)
        {
            value = _owner._head is null ? default : _owner._head.Value;
            return _owner._head is not null;
        }

        /// <summary>Пытается прочитать хвост за O(1), сохраняя состояние списка и перечислителей.</summary>
        /// <param name="value">Значение хвоста при успехе; default при пустом списке.</param>
        /// <returns>True, если элемент существует, включая хранимый null или default.</returns>
        public bool TryLast([MaybeNullWhen(false)] out T value)
        {
            value = _owner._tail is null ? default : _owner._tail.Value;
            return _owner._tail is not null;
        }

        /// <summary>Пытается прочитать первое совпадение за O(n).</summary>
        /// <param name="indexedPredicate">Ненулевое условие выбора элемента и позиции.</param>
        /// <param name="value">Найденное значение, включая null или default; default при отсутствии.</param>
        /// <returns>True, если совпадение найдено; иначе false.</returns>
        /// <exception cref="InvalidOperationException">Условие изменило список и вернуло результат.</exception>
        public bool TryFindFirst(IndexedPredicate<T> indexedPredicate, [MaybeNullWhen(false)] out T value)
        {
            var (_, targetNode, _) = _owner._find(indexedPredicate);

            value = targetNode is null ? default : targetNode.Value;
            return targetNode is not null;
        }

        /// <summary>Заменяет первое совпадение за O(n).</summary>
        /// <param name="indexedPredicate">Ненулевое условие выбора элемента и позиции.</param>
        /// <param name="newItem">Новое значение.</param>
        /// <returns>True при замене; false при отсутствии совпадения.</returns>
        /// <remarks>Успех инвалидирует перечислители даже при записи того же значения.</remarks>
        /// <exception cref="InvalidOperationException">Условие изменило список и вернуло результат.</exception>
        public bool ReplaceFirst(IndexedPredicate<T> indexedPredicate, T newItem)
        {
            var (_, targetNode, _) = _owner._find(indexedPredicate);
            if (targetNode is null)
                return false;

            _owner.SetNodeValue(targetNode, newItem);
            return true;
        }

        /// <summary>Передаёт объект первого совпадения действию для изменения его состояния за O(n).</summary>
        /// <param name="indexedPredicate">Ненулевое условие выбора элемента и позиции.</param>
        /// <param name="update">Ненулевое действие; получает хранимую ссылку, включая null.</param>
        /// <returns>True после успешного действия; false при отсутствии совпадения.</returns>
        /// <remarks>
        /// Ссылку в узле не заменяет. Успех инвалидирует перечислители даже при пустом действии.
        /// Изменения списка из действия обнаруживаются после его нормального завершения.
        /// При исключении действия версия этим методом не увеличивается; уже выполненные
        /// изменения объекта не откатываются. Сложность указана без стоимости делегатов.
        /// </remarks>
        /// <exception cref="NotSupportedException">T — значимый тип, включая Nullable, даже если список пуст.</exception>
        /// <exception cref="InvalidOperationException">Условие или действие изменило список и нормально завершилось.</exception>
        public bool UpdateFirst(IndexedPredicate<T> indexedPredicate, Action<T> update)
        {
            if (typeof(T).IsValueType)
                throw new NotSupportedException(
                    "UpdateFirst only supports reference element types.");

            var (_, targetNode, _) = _owner._find(indexedPredicate);
            if (targetNode is null)
                return false;

            var versionSnapshot = _owner._version;
            update(targetNode.Value);
            _owner.ValidateVersion(versionSnapshot);

            _owner.IncrementVersion();
            return true;
        }

        /// <summary>Удаляет первое совпадение, сохраняя порядок остальных элементов, за O(n).</summary>
        /// <param name="indexedPredicate">Ненулевое условие выбора элемента и позиции.</param>
        /// <returns>True при удалении; false при отсутствии совпадения.</returns>
        /// <remarks>Успех инвалидирует перечислители; отсутствие совпадения сохраняет их.</remarks>
        /// <exception cref="InvalidOperationException">Условие изменило список и вернуло результат.</exception>
        public bool RemoveFirst(IndexedPredicate<T> indexedPredicate)
        {
            var (previousNode, targetNode, _) = _owner._find(indexedPredicate);
            if (targetNode is null)
                return false;

            _owner.RemoveAfterNode(previousNode, targetNode);
            return true;
        }

        /// <summary>Находит позицию первого совпадения за O(n).</summary>
        /// <param name="indexedPredicate">Ненулевое условие выбора элемента и позиции.</param>
        /// <returns>Позиция от нуля либо -1 при отсутствии совпадения.</returns>
        /// <exception cref="InvalidOperationException">Условие изменило список и вернуло результат.</exception>
        public int PositionOf(IndexedPredicate<T> indexedPredicate)
        {
            var (_, _, depth) = _owner._find(indexedPredicate);

            return depth;
        }

        /// <summary>Проверяет наличие совпадения за O(n).</summary>
        /// <param name="indexedPredicate">Ненулевое условие выбора элемента и позиции.</param>
        /// <returns>True при наличии совпадения; иначе false.</returns>
        /// <exception cref="InvalidOperationException">Условие изменило список и вернуло результат.</exception>
        public bool Contains(IndexedPredicate<T> indexedPredicate)
        {
            var (_, targetNode, _) = _owner._find(indexedPredicate);

            return targetNode is not null;
        }

        /// <summary>Отсоединяет все узлы за O(1), обнуляет Count и инвалидирует перечислители даже у пустого списка.</summary>
        /// <remarks>Старый перечислитель может удерживать узлы до освобождения ссылки на него.</remarks>
        public void Clear()
        {
            var generation = new object();
            _owner._generation = generation;
            _owner._head = null;
            _owner._tail = null;
            _owner.Count = 0;
            _owner.IncrementVersion();
        }
    }
}
