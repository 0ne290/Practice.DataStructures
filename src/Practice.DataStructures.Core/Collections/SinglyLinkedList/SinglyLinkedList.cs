using System.Collections;

namespace Practice.DataStructures.Core.Collections;

/// <summary>Односвязный список с отдельными API значений и курсоров на общем хранилище.</summary>
/// <typeparam name="T">Тип элементов. Допускаются повторы, значения по умолчанию и null.</typeparam>
/// <remarks>
/// Условие получает элемент и его позицию от нуля. Поиск идёт от головы к хвосту
/// и прекращается на первом совпадении; каждая посещённая позиция проверяется один раз.
/// После нормально завершившегося вызова условия проверяется версия списка.
/// Изменения из условия обнаруживаются, но не откатываются; исключения делегата
/// передаются вызывающему коду. Чтение и вложенный поиск разрешены.
/// Делегаты должны быть ненулевыми; проверка этого предусловия не выполняется.
/// Изменение самого объекта через внешнюю ссылку версию списка не меняет.
/// Коллекция не обеспечивает синхронизацию доступа из нескольких потоков.
/// Values сохраняет операции над значениями; Cursors предоставляет позиции без
/// доступа к узлам. Count и перечисление общие для обоих API.
/// </remarks>
public sealed partial class SinglyLinkedList<T> : IEnumerable<T>
{
    /// <summary>Получает число элементов за O(1).</summary>
    public int Count { get; private set; }

    private int _version;
    private object _generation = new();

    /// <summary>Операции над значениями и выбор элементов по условию.</summary>
    public ValueApi Values { get; }

    /// <summary>Работа с сохранёнными позициями и операции после них.</summary>
    public CursorApi Cursors { get; }

    private Node? _head;
    private Node? _tail;

    /// <summary>Создаёт пустой список.</summary>
    public SinglyLinkedList()
    {
        _head = null;
        _tail = null;
        Count = 0;
        _version = 0;
        Values = new ValueApi(this);
        Cursors = new CursorApi(this);
    }

    // Общие механизмы работы с цепочкой, не привязанные к одному из публичных API.
    private void InsertAfterNode(Node? previous, T item)
    {
        var node = new Node { Value = item, Next = previous is null ? _head : previous.Next };
        if (previous is null)
            _head = node;
        else
            previous.Next = node;

        if (previous == _tail)
            _tail = node;

        Count++;
        IncrementVersion();
    }

    private void RemoveAfterNode(Node? previous, Node target)
    {
        if (previous is null)
            _head = target.Next;
        else
            previous.Next = target.Next;

        if (target == _tail)
            _tail = previous;

        target.IsAttached = false;
        target.Next = null;
        // Значение недоступно через курсоры после удаления; освобождаем ссылку.
        target.Value = default!;
        Count--;
        IncrementVersion();
    }

    private void SetNodeValue(Node node, T value)
    {
        node.Value = value;
        IncrementVersion();
    }

    private void IncrementVersion()
    {
        _version = unchecked(_version + 1);
    }

    private void ValidateVersion(int expectedVersion)
    {
        if (expectedVersion != _version)
            throw new InvalidOperationException("Collection was modified; operation cannot be continued.");
    }

    private (Node? previousNode, Node? targetNode, int depth) _find(IndexedPredicate<T> indexedPredicate)
    {
        if (Count == 0)
            return (null, null, -1);

        var versionSnapshot = _version;

        Node? previousNode = null;
        var currentNode = _head;
        var depth = 0;
        while (currentNode is not null)
        {
            var matches = indexedPredicate(currentNode.Value, depth);
            ValidateVersion(versionSnapshot);

            if (matches)
                return (previousNode, currentNode, depth);

            previousNode = currentNode;
            currentNode = currentNode.Next;
            depth++;
        }

        return (null, null, -1);
    }

    /// <summary>Создаёт независимый перечислитель от головы к хвосту за O(1).</summary>
    /// <returns>Перечислитель перед первым элементом; полный обход занимает O(n).</returns>
    /// <remarks>
    /// Версия фиксируется при создании. Успешные вставка, удаление, замена и обновление,
    /// а также каждый Clear инвалидируют старые перечислители: Current и MoveNext
    /// выбрасывают InvalidOperationException до первого шага и после завершения тоже.
    /// Current до первого шага и после конца выбрасывает InvalidOperationException.
    /// После конца MoveNext продолжает возвращать false, если версия не менялась.
    /// Reset не поддерживается. Dispose ничего не делает, продолжение обхода допустимо.
    /// </remarks>
    public IEnumerator<T> GetEnumerator()
    {
        return new Enumerator(this);
    }

    /// <inheritdoc cref="GetEnumerator"/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

}
