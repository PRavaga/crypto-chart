using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CryptoCoins.UWP.Platform.Collection
{
    public class SortedList<T> : IReadOnlyList<T>, IList, IList<T>
    {
        private readonly IComparer<T> _comparer;
        private readonly List<T> _sorted;

        public SortedList() : this(Enumerable.Empty<T>(), null)
        {
        }

        public SortedList(IComparer<T> comparer) : this(Enumerable.Empty<T>(), comparer)
        {
        }

        public SortedList(IEnumerable<T> sorted) : this(sorted, null)
        {
        }

        public SortedList(IEnumerable<T> collection, IComparer<T> comparer)
        {
            _sorted = collection.ToList();
            _sorted.Sort(comparer);
            _comparer = comparer;
        }

        int ICollection.Count => Count;
        public bool IsSynchronized { get; } = false;
        public object SyncRoot { get; } = new object();

        void ICollection.CopyTo(Array array, int index)
        {
            _sorted.CopyTo((T[]) array, index);
        }

        public void Clear()
        {
            _sorted.Clear();
        }

        public bool IsReadOnly { get; } = false;
        public bool IsFixedSize { get; } = false;

        object IList.this[int index]
        {
            get => this[index];
            set => throw new NotSupportedException();
        }

        int IList.Add(object value)
        {
            return Add((T) value);
        }

        bool IList.Contains(object value)
        {
            return Contains((T) value);
        }

        int IList.IndexOf(object value)
        {
            return IndexOf((T) value);
        }

        void IList.Insert(int index, object value)
        {
            throw new NotSupportedException();
        }

        void IList.Remove(object value)
        {
            Remove((T) value);
        }

        void IList.RemoveAt(int index)
        {
            RemoveAt(index);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _sorted.CopyTo(array, arrayIndex);
        }

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        public bool Contains(T item)
        {
            return IndexOf(item) >= 0;
        }

        public bool Remove(T item)
        {
            var i = IndexOf(item);
            if (i == -1)
            {
                return false;
            }
            RemoveAt(i);
            return true;
        }

        public int Count => _sorted.Count;

        public int IndexOf(T item)
        {
            if (_comparer != null)
            {
                var i = _sorted.BinarySearch(item, _comparer);
                return i >= 0 ? i : -1;
            }
            else
            {
                return _sorted.IndexOf(item);
            }
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            _sorted.RemoveAt(index);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _sorted.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        public T this[int index]
        {
            get => _sorted[index];
            set => throw new NotSupportedException();
        }

        int IReadOnlyCollection<T>.Count => Count;

        public int Add(T item)
        {
            if (_comparer != null)
            {
                var i = _sorted.BinarySearch(item, _comparer);
                if (i >= 0)
                {
                    while (i + 1 < _sorted.Count && _comparer.Compare(item, _sorted[i + 1]) == 0)
                    {
                        i++;
                    }
                }
                else
                {
                    i = ~i;
                }

                _sorted.Insert(i, item);
                return i;
            }
            else
            {
                _sorted.Add(item);
                return _sorted.Count - 1;
            }
        }
    }
}
