using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CryptoCoins.UWP.Platform.Collection
{
    public class FilterCollection<T> : IList<T>, IList, IReadOnlyList<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private readonly SortedList<T> _sortedList;
        private readonly List<T> _sourceList;
        private Func<T, bool> _filterFunc;
        private int _removedIndex;
        private T _removedItem;

        public FilterCollection() : this(Enumerable.Empty<T>())
        {
        }

        public FilterCollection(IEnumerable<T> collection) : this(collection, null)
        {
        }

        public FilterCollection(IComparer<T> comparer): this(Enumerable.Empty<T>(), comparer)
        {
        }

        public FilterCollection(IEnumerable<T> collection, IComparer<T> comparer)
        {
            _sourceList = collection.ToList();
            _sortedList = new SortedList<T>(_sourceList, comparer);
            PropertyChanged += OnPropertyChanged;
        }

        public IReadOnlyList<T> SourceList => _sourceList;

        public Func<T, bool> FilterFunc
        {
            get => _filterFunc;
            set => Set(ref _filterFunc, value);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            CopyTo((T[]) array, index);
        }

        public bool IsSynchronized { get; } = false;
        public object SyncRoot { get; } = new object();

        object IList.this[int index]
        {
            get => _sortedList[index];
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
            ((IList<T>) this).Insert(index, (T) value);
        }

        void IList.Remove(object value)
        {
            Remove((T) value);
        }

        public bool IsFixedSize { get; } = false;

        public int Count => _sortedList.Count;
        public bool IsReadOnly { get; } = false;

        public IEnumerator<T> GetEnumerator()
        {
            return _sortedList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        public void Clear()
        {
            _sourceList.Clear();
            _sortedList.Clear();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public bool Contains(T item)
        {
            return _sourceList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _sourceList.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            var index = IndexOf(item);
            if (index == -1)
            {
                return false;
            }
            RemoveAt(index);
            return true;
        }

        public int IndexOf(T item)
        {
            return _sortedList.IndexOf(item);
        }

        void IList<T>.Insert(int index, T item)
        {
            if (ReferenceEquals(_removedItem, item))
            {
                OnCollectionReordering(new ReorderEventArgs<T>(_removedIndex, index, item));
            }
            Add(item);
        }

        public void RemoveAt(int index)
        {
            _sourceList.Remove(this[index]);
            RemoveFilteredAt(index);
        }

        public void RemoveSourceAt(int index)
        {
            var item = _sourceList[index];
            if (!Remove(item))
            {
                _sourceList.RemoveAt(index);
            }
        }

        public T this[int index]
        {
            get => _sortedList[index];
            set => throw new NotSupportedException();
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        T IReadOnlyList<T>.this[int index] => _sortedList[index];
        public event EventHandler<ReorderEventArgs<T>> CollectionReordering;

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FilterFunc))
            {
                Filter();
            }
        }

        public int Add(T item)
        {
            _sourceList.Add(item);
            if (FilterItem(item))
            {
                return AddFiltered(item);
            }
            return -1;
        }

        public void Filter()
        {
            if (FilterFunc == null)
            {
                return;
            }
            for (var i = _sortedList.Count - 1; i >= 0; i--)
            {
                if (!FilterFunc(_sortedList[i]))
                {
                    RemoveFilteredAt(i);
                }
            }
            foreach (var missingItem in _sourceList.Except(_sortedList))
            {
                if (FilterFunc(missingItem))
                {
                    AddFiltered(missingItem);
                }
            }
        }

        protected void Set<TK>(ref TK storage, TK value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }

        protected int AddFiltered(T item)
        {
            var index = _sortedList.Add(item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
            return index;
        }

        protected void RemoveFilteredAt(int index)
        {
            _removedIndex = index;
            _removedItem = _sortedList[index];
            _sortedList.RemoveAt(index);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, _removedItem, index));
        }

        private bool FilterItem(T item)
        {
            if (_filterFunc == null)
            {
                return true;
            }
            return _filterFunc(item);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            CollectionChanged?.Invoke(this, args);
        }

        protected void OnCollectionReordering(ReorderEventArgs<T> args)
        {
            CollectionReordering?.Invoke(this, args);
        }
    }
}
