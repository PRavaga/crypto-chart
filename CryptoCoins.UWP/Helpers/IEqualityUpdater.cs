using System.Collections.Generic;

namespace CryptoCoins.UWP.Helpers
{
    public interface IEqualityUpdater<in T> : IEqualityComparer<T>
    {
        void Update(T target, T source);
    }
}
