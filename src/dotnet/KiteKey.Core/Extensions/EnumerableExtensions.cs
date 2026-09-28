using System.ComponentModel;
using System.Linq.Expressions;

namespace KiteKey.Core;

/// <summary>Extensions for <see cref="IEnumerable{T}" /></summary>
public static class EnumerableExtensions
{
	/// <summary>
	///     Enumerate an <see cref="IEnumerable{T}" /> as an <see cref="IAsyncEnumerable{T}" />, for APIs that might accept both sync and async results
	/// </summary>
	/// <typeparam name="T">Element type</typeparam>
	/// <param name="items">Items to enumerate</param>
	/// <returns><see cref="IAsyncEnumerable{T}" /> for <paramref name="items" /></returns>
	public static async IAsyncEnumerable<T> AsAsyncEnumerable<T>(this IEnumerable<T> items)
	{
		foreach(T item in items)
			yield return item;
	}

	/// <summary>
	///     <see cref="Enumerable.OrderBy{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey})" /> that uses <see cref="ListSortDirection" />
	/// </summary>
	/// <typeparam name="TItem">Query item type</typeparam>
	/// <typeparam name="TKey">Sort value type</typeparam>
	/// <param name="source">Query source</param>
	/// <param name="getSortValue">Function that returns the value used for sorting</param>
	/// <param name="direction">Sort direction</param>
	/// <returns>Ordered query</returns>
	public static IOrderedEnumerable<TItem> OrderBy<TItem, TKey>(this IEnumerable<TItem> source, Func<TItem, TKey> getSortValue, ListSortDirection? direction)
	{
		return direction switch
		{
			ListSortDirection.Ascending => source.OrderBy(getSortValue),
			ListSortDirection.Descending => source.OrderByDescending(getSortValue),
			null => source.OrderBy(getSortValue),
			_ => throw new ArgumentOutOfRangeException(nameof(direction)),
		};
	}

	/// <summary>
	///     <see cref="Queryable.ThenBy{TSource, TKey}(IOrderedQueryable{TSource}, Expression{Func{TSource, TKey}})"/>
	///     that uses <see cref="ListSortDirection" />
	/// </summary>
	/// <typeparam name="TItem">Query item type</typeparam>
	/// <typeparam name="TKey">Sort value type</typeparam>
	/// <param name="source">Query source</param>
	/// <param name="getSortValue">Function that determines the value used for sorting</param>
	/// <param name="direction">Sort direction</param>
	/// <returns>Ordered query</returns>
	public static IOrderedEnumerable<TItem> ThenBy<TItem, TKey>(this IOrderedEnumerable<TItem> source, Func<TItem, TKey> getSortValue, ListSortDirection? direction)
	{
		return direction switch
		{
			ListSortDirection.Ascending => source.ThenBy(getSortValue),
			ListSortDirection.Descending => source.ThenByDescending(getSortValue),
			null => source,
			_ => throw new ArgumentOutOfRangeException(nameof(direction)),
		};
	}

	/// <summary>Project to a <see cref="SortedList{TKey, TValue}" /> sorted by the item value itself</summary>
	/// <typeparam name="TItem">Item type</typeparam>
	/// <param name="source">Source items</param>
	/// <returns>New <see cref="SortedList{TKey, TValue}" /></returns>
	public static SortedList<TItem, TItem> ToSortedList<TItem>(this IEnumerable<TItem> source)
		where TItem : IComparable<TItem>
	{
		SortedList<TItem, TItem> list = new();

		foreach(TItem item in source)
			list.Add(item, item);

		return list;
	}
}
