using System.Collections;
using System.Collections.Generic;

namespace Balla.Core
{
    public readonly struct SizeCollection<T> : ICollection<T>
    {
        public SizeCollection(int size)
        {
            Count = size;
        }

        public int Count { get; }

        public readonly bool IsSynchronized => true;

        public readonly bool IsReadOnly => false;

        public readonly void Add(T item) { }

        public readonly void Clear() { }

        public readonly bool Contains(T item) => true;

        public readonly void CopyTo(System.Array array, int index) { }

        public readonly void CopyTo(T[] array, int arrayIndex) { }

        public readonly IEnumerator GetEnumerator() => null;

        public readonly bool Remove(T item) => true;

        readonly IEnumerator<T> IEnumerable<T>.GetEnumerator() => null;
    }
}
