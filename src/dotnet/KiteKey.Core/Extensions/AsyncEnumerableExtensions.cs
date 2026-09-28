using System.Runtime.CompilerServices;

namespace KiteKey.Core;

/// <summary>Additional async sequence operations not provided by .NET 10's System.Linq.AsyncEnumerable.</summary>
public static class AsyncEnumerableExtensions
{
    public static async Task<bool> Any<T>(this IAsyncEnumerable<T> source, Func<T, bool> predicate, CancellationToken cancellationToken = default)
    {
        await foreach (T item in source.WithCancellation(cancellationToken))
            if (predicate(item))
                return true;
        return false;
    }

    public static async Task<int> Count<T>(this IAsyncEnumerable<T> source, CancellationToken cancellationToken = default)
    {
        int count = 0;
        await foreach (T _ in source.WithCancellation(cancellationToken))
            count = checked(count + 1);
        return count;
    }

    public static async Task<T?> Max<T>(this IAsyncEnumerable<T> source, CancellationToken cancellationToken = default)
        where T : struct, IComparable<T>
        => await source.Max<T, T>(item => item, cancellationToken);

    public static async Task<TValue?> Max<TItem, TValue>(this IAsyncEnumerable<TItem> source, Func<TItem, TValue?> selector, CancellationToken cancellationToken = default)
        where TValue : struct, IComparable<TValue>
    {
        TValue? max = null;
        await foreach (TItem item in source.WithCancellation(cancellationToken))
        {
            TValue? value = selector(item);
            if (value.HasValue && (!max.HasValue || value.Value.CompareTo(max.Value) > 0))
                max = value;
        }
        return max;
    }

    public static async Task<TItem?> MaxBy<TItem, TValue>(this IAsyncEnumerable<TItem> source, Func<TItem, TValue?> selector, CancellationToken cancellationToken = default)
        where TValue : struct, IComparable<TValue>
    {
        TValue? max = null;
        TItem? result = default;
        await foreach (TItem item in source.WithCancellation(cancellationToken))
        {
            TValue? value = selector(item);
            if (value.HasValue && (!max.HasValue || value.Value.CompareTo(max.Value) > 0))
            {
                max = value;
                result = item;
            }
        }
        return result;
    }

    public static async IAsyncEnumerable<TOut> Select<TIn, TOut>(this IAsyncEnumerable<TIn> source, Func<TIn, CancellationToken, TOut> selector, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (TIn item in source.WithCancellation(cancellationToken))
            yield return selector(item, cancellationToken);
    }
}
