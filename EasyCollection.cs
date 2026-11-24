using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SkyWebFramework.Utility
{
    /// <summary>
    /// Represents a single page of items along with pagination metadata.
    /// </summary>
    public sealed class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
        public int TotalCount { get; init; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    /// <summary>
    /// Collection and enumeration utilities and extension methods.
    /// </summary>
    public static class EasyCollection
    {
        /// <summary>
        /// Safely gets a value from a dictionary or returns default if key doesn't exist.
        /// </summary>
        public static TValue? GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue>? dictionary, TKey key, TValue? defaultValue = default)
        {
            if (dictionary is null) return defaultValue;
            return dictionary.TryGetValue(key, out var value) ? value : defaultValue;
        }

        /// <summary>
        /// Checks if a collection is null or contains no elements.
        /// </summary>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T>? collection)
            => collection is null || !collection.Any();

        /// <summary>
        /// Checks if a collection is not null and contains at least one element.
        /// </summary>
        public static bool HasItems<T>(this IEnumerable<T>? collection)
            => collection is not null && collection.Any();

        /// <summary>
        /// Safely checks whether a potentially null sequence contains any elements (optionally matching a predicate).
        /// </summary>
        public static bool SafeAny<T>(this IEnumerable<T>? source, Func<T, bool>? predicate = null)
        {
            if (source is null) return false;
            return predicate is null ? source.Any() : source.Any(predicate);
        }

        /// <summary>
        /// Safely returns the first element of a sequence (optionally matching a predicate), or the specified default value if null or empty.
        /// </summary>
        public static T? SafeFirstOrDefault<T>(this IEnumerable<T>? source, Func<T, bool>? predicate = null, T? defaultValue = default)
        {
            if (source is null) return defaultValue;
            foreach (var item in source)
            {
                if (predicate is null || predicate(item))
                    return item;
            }
            return defaultValue;
        }

        /// <summary>
        /// Returns the count of elements in a sequence, or 0 if the sequence is null.
        /// </summary>
        public static int SafeCount<T>(this IEnumerable<T>? source)
            => source?.Count() ?? 0;

        /// <summary>
        /// Returns an empty sequence if the source is null; otherwise returns the source.
        /// </summary>
        public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T>? source)
            => source ?? Enumerable.Empty<T>();

        /// <summary>
        /// Batches a collection into smaller chunks of the specified size.
        /// </summary>
        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (batchSize <= 0) throw new ArgumentException("Batch size must be positive.", nameof(batchSize));

            var batch = new List<T>(batchSize);
            foreach (var item in source)
            {
                batch.Add(item);
                if (batch.Count == batchSize)
                {
                    yield return batch;
                    batch = new List<T>(batchSize);
                }
            }

            if (batch.Count > 0)
                yield return batch;
        }

        /// <summary>
        /// Returns distinct elements from a sequence according to a specified key selector.
        /// </summary>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (keySelector is null) throw new ArgumentNullException(nameof(keySelector));

            var seenKeys = new HashSet<TKey>();
            foreach (var element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                    yield return element;
            }
        }

        /// <summary>
        /// Converts a collection to a delimited string (returns empty string if source is null).
        /// </summary>
        public static string ToDelimitedString<T>(this IEnumerable<T>? source, string delimiter = ", ")
            => source is null ? string.Empty : string.Join(delimiter, source);

        /// <summary>
        /// Executes an action for each element in the sequence.
        /// </summary>
        public static void ForEach<T>(this IEnumerable<T>? source, Action<T> action)
        {
            if (source is null) return;
            if (action is null) throw new ArgumentNullException(nameof(action));

            foreach (var item in source)
                action(item);
        }

        /// <summary>
        /// Executes an action for each element in the sequence along with its zero-based index.
        /// </summary>
        public static void ForEach<T>(this IEnumerable<T>? source, Action<T, int> action)
        {
            if (source is null) return;
            if (action is null) throw new ArgumentNullException(nameof(action));

            int index = 0;
            foreach (var item in source)
                action(item, index++);
        }

        /// <summary>
        /// Asynchronously executes an action for each element in the sequence with optional concurrency control.
        /// </summary>
        public static async Task ForEachAsync<T>(
            this IEnumerable<T>? source,
            Func<T, Task> action,
            int maxDegreeOfParallelism = 1,
            CancellationToken cancellationToken = default)
        {
            if (source is null) return;
            if (action is null) throw new ArgumentNullException(nameof(action));
            if (maxDegreeOfParallelism <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxDegreeOfParallelism), "Must be at least 1.");

            if (maxDegreeOfParallelism == 1)
            {
                foreach (var item in source)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await action(item).ConfigureAwait(false);
                }
                return;
            }

            using var semaphore = new SemaphoreSlim(maxDegreeOfParallelism);
            var tasks = new List<Task>();

            foreach (var item in source)
            {
                await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        await action(item).ConfigureAwait(false);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }, cancellationToken));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <summary>
        /// Adds multiple items to an <see cref="ICollection{T}"/>.
        /// </summary>
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T>? items)
        {
            if (collection is null) throw new ArgumentNullException(nameof(collection));
            if (items is null) return;

            if (collection is List<T> list)
            {
                list.AddRange(items);
                return;
            }

            foreach (var item in items)
                collection.Add(item);
        }

        /// <summary>
        /// Adds an item to the collection only if the item is not null.
        /// </summary>
        public static bool AddIfNotNull<T>(this ICollection<T> collection, T? item) where T : class
        {
            if (collection is null) throw new ArgumentNullException(nameof(collection));
            if (item is null) return false;

            collection.Add(item);
            return true;
        }

        /// <summary>
        /// Removes all items matching the specified predicate from an <see cref="ICollection{T}"/> and returns the count removed.
        /// </summary>
        public static int RemoveAll<T>(this ICollection<T> collection, Func<T, bool> predicate)
        {
            if (collection is null) throw new ArgumentNullException(nameof(collection));
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));

            if (collection is List<T> list)
                return list.RemoveAll(new Predicate<T>(predicate));

            var toRemove = collection.Where(predicate).ToList();
            foreach (var item in toRemove)
                collection.Remove(item);

            return toRemove.Count;
        }

        /// <summary>
        /// Returns a shuffled list of all elements from the source using Fisher-Yates shuffle.
        /// </summary>
        public static List<T> Shuffle<T>(this IEnumerable<T> source)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));

            var list = source.ToList();
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Shared.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
            return list;
        }

        /// <summary>
        /// Returns a random sample of up to <paramref name="count"/> distinct-position elements from the source.
        /// </summary>
        public static List<T> Sample<T>(this IEnumerable<T> source, int count)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (count <= 0) return new List<T>();

            return source.Shuffle().Take(count).ToList();
        }

        /// <summary>
        /// Returns a single random element from the sequence, or default if empty.
        /// </summary>
        public static T? RandomItem<T>(this IEnumerable<T>? source)
        {
            if (source is null) return default;
            var list = source as IList<T> ?? source.ToList();
            if (list.Count == 0) return default;
            return list[Random.Shared.Next(list.Count)];
        }

        /// <summary>
        /// Returns a single page of items using 1-based page numbering.
        /// </summary>
        public static IEnumerable<T> Paginate<T>(this IEnumerable<T> source, int pageNumber, int pageSize)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (pageNumber < 1) throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be >= 1.");
            if (pageSize < 1) throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be >= 1.");

            return source.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }

        /// <summary>
        /// Materializes a sequence into a <see cref="PagedResult{T}"/> with 1-based page numbering and metadata.
        /// </summary>
        public static PagedResult<T> ToPagedResult<T>(this IEnumerable<T> source, int pageNumber, int pageSize)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (pageNumber < 1) throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be >= 1.");
            if (pageSize < 1) throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be >= 1.");

            var list = source as ICollection<T> ?? source.ToList();
            var items = list.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return new PagedResult<T>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = list.Count
            };
        }

        /// <summary>
        /// Safely retrieves an element at the specified index or returns the default value if out of bounds or null.
        /// </summary>
        public static T? GetAtOrDefault<T>(this IReadOnlyList<T>? list, int index, T? defaultValue = default)
        {
            if (list is null || index < 0 || index >= list.Count)
                return defaultValue;

            return list[index];
        }

        /// <summary>
        /// Filters out null elements from a reference-type sequence with proper nullable annotation.
        /// </summary>
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source) where T : class
        {
            if (source is null) yield break;
            foreach (var item in source)
            {
                if (item is not null)
                    yield return item;
            }
        }

        /// <summary>
        /// Filters out null elements from a nullable value-type sequence.
        /// </summary>
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source) where T : struct
        {
            if (source is null) yield break;
            foreach (var item in source)
            {
                if (item.HasValue)
                    yield return item.Value;
            }
        }

        /// <summary>
        /// Returns the zero-based index of the first element matching the predicate, or -1 if not found.
        /// </summary>
        public static int IndexOf<T>(this IEnumerable<T>? source, Func<T, bool> predicate)
        {
            if (source is null) return -1;
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));

            int index = 0;
            foreach (var item in source)
            {
                if (predicate(item))
                    return index;
                index++;
            }
            return -1;
        }
    }
}
