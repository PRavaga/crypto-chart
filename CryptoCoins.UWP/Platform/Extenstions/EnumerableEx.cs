using System;
using System.Collections.Generic;

namespace CryptoCoins.UWP.Platform.Extenstions
{
    public static class EnumerableEx
    {
        public static IEnumerable<T> CreateItems<T>(int count) where T : new()
        {
            return CreateItems(count, () => new T());
        }

        public static IEnumerable<T> CreateItems<T>(int count, Func<T> creator)
        {
            for (var i = 0; i < count; i++)
            {
                yield return creator();
            }
        }
    }
}
