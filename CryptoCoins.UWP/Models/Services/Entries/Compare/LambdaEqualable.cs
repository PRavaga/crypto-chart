using System;
using System.Collections.Generic;

namespace CryptoCoins.UWP.Models.Services.Entries.Compare
{
    public class LambdaEqualable<TKey, T> : IEqualityComparer<T>
    {
        private readonly Func<T, TKey> _keySelector;

        public LambdaEqualable(Func<T, TKey> keySelector)
        {
            _keySelector = keySelector;
        }

        public bool Equals(T x, T y)
        {
            return EqualityComparer<TKey>.Default.Equals(_keySelector(x), _keySelector(y));
        }

        public int GetHashCode(T obj)
        {
            return _keySelector(obj).GetHashCode();
        }
    }
}
