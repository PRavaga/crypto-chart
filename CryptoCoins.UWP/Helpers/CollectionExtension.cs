using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CryptoCoins.UWP.Helpers
{
    public static class CollectionExtension
    {
        /// <summary>
        /// Update only existing objects
        /// </summary>
        public static void UpdateElements<T>(this IEnumerable<T> target, IEnumerable<T> source, IEqualityUpdater<T> equalityUpdater)
            where T : class
        {
            var sourceMap = source.AddToDictionary(equalityUpdater.GetHashCode);
            foreach (var targetItem in target)
            {
                var hash = equalityUpdater.GetHashCode(targetItem);
                sourceMap.TryGetValue(hash, out var sourceItem);
                if (sourceItem != default(T))
                {
                    equalityUpdater.Update(targetItem, sourceItem);
                }
            }
        }

        /// <summary>
        /// Update only existing objects
        /// </summary>
        public static void UpdateElements<T, TK, TKey>(this IEnumerable<T> target, IEnumerable<TK> source, Func<T, TKey> innerKeySelector,
            Func<TK, TKey> outerKeySelector, Action<T, TK> updateAction)
        {
            var sourceMap = source.AddToDictionary(outerKeySelector);
            foreach (var targetItem in target)
            {
                var key = innerKeySelector(targetItem);
                if (sourceMap.TryGetValue(key, out var sourceItem))
                {
                    updateAction(targetItem, sourceItem);
                }
            }
        }

        /// <summary>
        /// Update only existing objects
        /// </summary>
        public static void UpdateElements<T, TK, TKey>(this IDictionary<TKey, T> target, IEnumerable<TK> source,
            Func<TK, TKey> outerKeySelector, Action<T, TK> updateAction)
        {
            foreach (var sourceItem in source)
            {
                var key = outerKeySelector(sourceItem);
                if (target.TryGetValue(key, out var targetItem))
                {
                    updateAction(targetItem, sourceItem);
                }
            }
        }

        /// <summary>
        /// Update existing objects or add them if they are missing
        /// </summary>
        public static void AddOrUpdate<T>(this IList<T> target, T source, IEqualityUpdater<T> equalityUpdater) where T : class
        {
            var match = target.FirstOrDefault(arg => equalityUpdater.Equals(arg, source));
            if (match != null)
            {
                equalityUpdater.Update(match, source);
            }
            else
            {
                target.Add(source);
            }
        }
        /// <summary>
        /// Update existing objects or add them if they are missing
        /// </summary>
        public static void AddOrUpdate<T>(this IList<T> target, T source, IEqualityComparer<T> equalityUpdater, Action<T, T> updateAction) where T : class
        {
            var match = target.FirstOrDefault(arg => equalityUpdater.Equals(arg, source));
            if (match != null)
            {
                updateAction(match, source);
            }
            else
            {
                target.Add(source);
            }
        }

        /// <summary>
        /// Update existing objects, add them if they are missing
        /// </summary>
        public static void AddOrUpdate<T>(this IList<T> target, IEnumerable<T> source, IEqualityComparer<T> equalityUpdater, Action<T, T> updateAction) where T : class
        {
            if (target.Count == 0)
            {
                foreach (var element in source)
                {
                    target.Add(element);
                }
                return;
            }

            var sourceMap = source.AddToDictionary(equalityUpdater.GetHashCode);
            for (var i = target.Count - 1; i >= 0; i--)
            {
                var targetItem = target[i];
                var hash = equalityUpdater.GetHashCode(targetItem);
                sourceMap.TryGetValue(hash, out var sourceItem);
                if (sourceItem != default(T))
                {
                    updateAction(targetItem, sourceItem);
                    sourceMap.Remove(hash);
                }
            }

            foreach (var missingElement in sourceMap.Values)
            {
                target.Add(missingElement);
            }
        }
        /// <summary>
        /// Update existing objects, add them if they are missing and delete not updated
        /// </summary>
        public static void RightJoin<T>(this IList<T> target, IEnumerable<T> source, IEqualityComparer<T> equalityUpdater, Action<T, T> updateAction) where T : class
        {
            if (target.Count == 0)
            {
                foreach (var element in source)
                {
                    target.Add(element);
                }
                return;
            }

            var sourceMap = source.AddToDictionary(equalityUpdater.GetHashCode);
            for (var i = target.Count - 1; i >= 0; i--)
            {
                var targetItem = target[i];
                var hash = equalityUpdater.GetHashCode(targetItem);
                sourceMap.TryGetValue(hash, out var sourceItem);
                if (sourceItem != default(T))
                {
                    updateAction(targetItem, sourceItem);
                    sourceMap.Remove(hash);
                }
                else
                {
                    target.RemoveAt(i);
                }
            }

            foreach (var missingElement in sourceMap.Values)
            {
                target.Add(missingElement);
            }
        }

        /// <summary>
        /// Update existing objects, add them if they are missing and delete not updated
        /// </summary>
        public static void RightJoin<T>(this IList<T> target, IEnumerable<T> source, IEqualityUpdater<T> equalityUpdater) where T : class
        {
            RightJoin(target, source, equalityUpdater, equalityUpdater.Update);
        }


        /// <summary>
        /// Update existing object, add them if they are missing and remove existing if they are not presented in source
        /// </summary>
        public static void CompleteElements<T>(this IList<T> target, IList<T> source, IEqualityComparer<T> equalityUpdater) where T : class
        {
            if (target.Count == 0)
            {
                foreach (var element in source)
                {
                    target.Add(element);
                }
                return;
            }

            var sourceMap = source.AddToDictionary(equalityUpdater.GetHashCode);
            for (var i = target.Count - 1; i >= 0; i--)
            {
                var targetItem = target[i];
                var hash = equalityUpdater.GetHashCode(targetItem);
                if(sourceMap.TryGetValue(hash, out var _))
                {
                    sourceMap.Remove(hash);
                }
                else
                {
                    target.RemoveAt(i);
                }
            }

            foreach (var missingElement in sourceMap.Values)
            {
                target.Add(missingElement);
            }
        }

        public static void AddRange<T>(this ICollection<T> list, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                list.Add(item);
            }
        }

        public static int FindIndex<T>(this IEnumerable<T> list, Predicate<T> predicate)
        {
            var index = 0;
            foreach (var item in list)
            {
                if(predicate(item))
                {
                    return index;
                }
                index++;
            }
            return -1;
        }

        public static void Sort<TSource, TKey>(this ObservableCollection<TSource> source, Func<TSource, TKey> keySelector)
        {
            var sortedSource = source.OrderBy(keySelector).ToList();

            for (var i = 0; i < sortedSource.Count; i++)
            {
                var itemToSort = sortedSource[i];

                // If the item is already at the right position, leave it and continue.
                if (EqualityComparer<TSource>.Default.Equals(source[i],itemToSort))
                {
                    continue;
                }

                var oldIndex = source.IndexOf(itemToSort);
                source.Move(oldIndex, i);
            }
        }

        public static Dictionary<TKey,T> AddToDictionary<TKey,T>(this IEnumerable<T> source, Func<T, TKey> keySelector)
        {
            var result = new Dictionary<TKey,T>();
            foreach (var item in source)
            {
                var key = keySelector(item);
                if (!result.ContainsKey(key))
                {
                    result.Add(key, item);
                }
            }

            return result;
        }
    }
}
