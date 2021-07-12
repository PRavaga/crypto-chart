using System;
using System.Collections.Specialized;

namespace CryptoCoins.UWP.Platform.Collection
{
    public class CollectionEventWatcher<T>
    {
        private readonly INotifyCollectionChanged _collection;
        private int _removedIndex;
        private T _removedItem;

        public CollectionEventWatcher(INotifyCollectionChanged collection)
        {
            _collection = collection;
            _collection.CollectionChanged += OnCollectionChanged;
        }

        public void Unsubscribe()
        {
            _collection.CollectionChanged -= OnCollectionChanged;
        }

        public event EventHandler<ReorderEventArgs<T>> CollectionReordered;

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    _removedIndex = args.OldStartingIndex;
                    _removedItem = (T) args.OldItems[0];
                    break;
                case NotifyCollectionChangedAction.Add:
                    var newItem = (T) args.NewItems[0];
                    if (newItem.Equals(_removedItem))
                    {
                        CollectionReordered?.Invoke(this, new ReorderEventArgs<T>(_removedIndex, args.NewStartingIndex, newItem));
                    }
                    break;
            }
        }
    }
}
