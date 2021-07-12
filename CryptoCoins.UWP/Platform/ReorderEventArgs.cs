using System;

namespace CryptoCoins.UWP.Platform
{
    public class ReorderEventArgs<T> : EventArgs
    {
        public ReorderEventArgs(int oldIndex, int newIndex, T item)
        {
            OldIndex = oldIndex;
            NewIndex = newIndex;
            Item = item;
        }

        public int OldIndex { get; }
        public int NewIndex { get; }
        public T Item { get; }
    }
}
