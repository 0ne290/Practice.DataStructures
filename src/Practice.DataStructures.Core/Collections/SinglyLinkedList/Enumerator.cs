using System.Collections;

namespace Practice.DataStructures.Core.Collections;

public sealed partial class SinglyLinkedList<T>
{
    private sealed class Enumerator : IEnumerator<T>
    {
        private readonly SinglyLinkedList<T> _owner;
        private readonly int _expectedVersion;
        private Node? _currentNode;
        private bool _started;

        internal Enumerator(SinglyLinkedList<T> owner)
        {
            _owner = owner;
            _expectedVersion = owner._version;
            _currentNode = null;
        }

        public T Current
        {
            get
            {
                _owner.ValidateVersion(_expectedVersion);
                return _currentNode is null
                    ? throw new InvalidOperationException("Enumeration has not started or has already finished.")
                    : _currentNode.Value;
            }
        }

        object? IEnumerator.Current => Current;

        public bool MoveNext()
        {
            _owner.ValidateVersion(_expectedVersion);

            _currentNode = _started ? _currentNode?.Next : _owner._head;
            _started = true;

            return _currentNode != null;
        }

        public void Dispose()
        {
            // Перечислитель не владеет ресурсами, требующими освобождения.
        }

        void IEnumerator.Reset()
        {
            throw new NotSupportedException("Reset is not supported. Obtain a new enumerator instead.");
        }

    }
}
