using System;
using System.Collections.Generic;

namespace CryptoCoins.UWP.Models.Services.Entries.Compare
{
    internal class LambdaComparer<T> : IComparer<T>
    {
        private readonly Func<T, T, int> _compareFunc;

        public LambdaComparer(Func<T, T, int> compareFunc)
        {
            _compareFunc = compareFunc;
        }

        public int Compare(T x, T y)
        {
            return _compareFunc(x, y);
        }
    }
}
