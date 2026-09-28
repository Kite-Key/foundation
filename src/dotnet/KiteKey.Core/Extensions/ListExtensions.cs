using System.ComponentModel;
using KiteKey.Core.Collections;

namespace KiteKey.Core;

/// <summary>Extensions for <see cref="List{T}" /> and related interfaces</summary>
public static class ListExtensions
{
	/// <summary>Add converted items from another list</summary>
	/// <typeparam name="TItem">List item type</typeparam>
	/// <typeparam name="TSource">Source item type</typeparam>
	/// <param name="list">List to process</param>
	/// <param name="sourceList">Source items to convert</param>
	/// <param name="convertValue">Function that accepts the item index and source value and returns the new value</param>
	public static void AddValuesFrom<TItem, TSource>(this IList<TItem> list, IReadOnlyList<TSource> sourceList, Func<int, TSource, TItem> convertValue)
	{
		for(int index = 0; index < sourceList.Count; index++)
		{
			TSource sourceItem = sourceList[index];
			TItem converted = convertValue(index, sourceItem);
			list.Add(converted);
		}
	}

	/// <summary>Add converted items from another list</summary>
	/// <typeparam name="TItem">List item type</typeparam>
	/// <typeparam name="TSource">Source item type</typeparam>
	/// <param name="list">List to process</param>
	/// <param name="sourceList">Source items to convert</param>
	/// <param name="convertValue">Function that accepts the source value and returns the new value</param>
	public static void AddValuesFrom<TItem, TSource>(this IList<TItem> list, IReadOnlyList<TSource> sourceList, Func<TSource, TItem> convertValue)
		=> list.AddValuesFrom(sourceList, (_, value) => convertValue(value));

	/// <summary>Determine if collection is null or empty.</summary>
	/// <typeparam name="T">The generic type param</typeparam>
	/// <param name="source">The source for the static extension</param>
	/// <returns>True if null or empty, false otherwise.</returns>
	public static bool IsNullOrEmpty<T>(this IReadOnlyList<T>? source)
	{
		if(source == null)
			return true;
		else
			return source.Count == 0;
	}

	/// <summary>Get a random element from the list.</summary>
	/// <typeparam name="TItem">Element in list.</typeparam>
	/// <param name="source"><inheritdoc cref="IList{T}" /></param>
	/// <returns>Random Element from list.</returns>
	public static TItem Random<TItem>(this IReadOnlyList<TItem> source)
		=> source[System.Random.Shared.Next(source.Count)];

	/// <summary>Create a new list with values mapped from another list</summary>
	/// <typeparam name="TItem">New item type</typeparam>
	/// <typeparam name="TSource">Source item type</typeparam>
	/// <param name="sourceList">Source list to map from</param>
	/// <param name="convertValue">Function that accepts the source value and returns the new value</param>
	/// <returns>List of mapped values, or an empty list if <paramref name="sourceList" /> was <c>null</c></returns>
	/// <remarks>This functions like <c>Select</c> followed by <c>ToArray</c>, but is more efficient since we know the list size in advance</remarks>
	public static TItem[] SelectArray<TItem, TSource>(this IReadOnlyList<TSource>? sourceList, Func<TSource, TItem> convertValue)
	{
		if(sourceList is null || sourceList.Count == 0)
			return Array.Empty<TItem>();

		TItem[] mapped = new TItem[sourceList.Count];
		mapped.SetValuesFrom(sourceList, convertValue);
		return mapped;
	}

	/// <summary>Create a new list with values mapped from another list</summary>
	/// <typeparam name="TItem">New item type</typeparam>
	/// <typeparam name="TSource">Source item type</typeparam>
	/// <param name="sourceList">Source list to map from</param>
	/// <param name="loadValue">Function that accepts the source value and loads the new value asynchronously</param>
	/// <returns>List of mapped values, or an empty list if <paramref name="sourceList" /> was <c>null</c></returns>
	/// <remarks>This functions like <c>Select</c> followed by <c>ToArray</c>, but is more efficient since we know the list size in advance</remarks>
	public static async Task<TItem[]> SelectArray<TItem, TSource>(this IReadOnlyList<TSource>? sourceList, Func<TSource, Task<TItem>> loadValue)
	{
		if(sourceList is null || sourceList.Count == 0)
			return Array.Empty<TItem>();

		TItem[] mapped = new TItem[sourceList.Count];
		await mapped.SetValuesFrom(sourceList, loadValue);
		return mapped;
	}

	/// <summary>Create a new list with values mapped from another list</summary>
	/// <typeparam name="TItem">New item type</typeparam>
	/// <typeparam name="TSource">Source item type</typeparam>
	/// <param name="sourceList">Source list to map from</param>
	/// <param name="convertValue">Function that accepts the source value and returns the new value</param>
	/// <returns>List of mapped values, or an empty list if <paramref name="sourceList" /> was <c>null</c></returns>
	/// <remarks>This functions like <c>Select</c> followed by <c>ToList</c>, but is more efficient since we know the list size in advance</remarks>
	public static List<TItem> SelectList<TItem, TSource>(this IReadOnlyList<TSource>? sourceList, Func<TSource, TItem> convertValue)
	{
		if(sourceList is null || sourceList.Count == 0)
			return [];

		List<TItem> mappedList = new(sourceList.Count);
		foreach(TSource sourceItem in sourceList)
		{
			TItem mappedItem = convertValue(sourceItem);
			mappedList.Add(mappedItem);
		}

		return mappedList;
	}

	/// <summary>Create a new list with values mapped from another list</summary>
	/// <typeparam name="TItem">New item type</typeparam>
	/// <typeparam name="TSource">Source item type</typeparam>
	/// <param name="sourceList">Source list to map from</param>
	/// <param name="loadValue">Function that accepts the source value and loads the new value asynchronously</param>
	/// <returns>List of mapped values, or an empty list if <paramref name="sourceList" /> was <c>null</c></returns>
	/// <remarks>This functions like <c>Select</c> followed by <c>ToList</c>, but is more efficient since we know the list size in advance</remarks>
	public static async Task<List<TItem>> SelectList<TItem, TSource>(this IReadOnlyList<TSource>? sourceList, Func<TSource, Task<TItem>> loadValue)
	{
		if(sourceList is null || sourceList.Count == 0)
			return [];

		List<TItem> mappedList = new(sourceList.Count);
		foreach(TSource sourceItem in sourceList)
		{
			TItem mappedItem = await loadValue(sourceItem);
			mappedList.Add(mappedItem);
		}

		return mappedList;
	}

	/// <summary>Set each item in the list to a new value</summary>
	/// <typeparam name="TItem">List item type</typeparam>
	/// <param name="list">List to process</param>
	/// <param name="getNewValue">Function that accepts the item index and value and returns the new value</param>
	public static void SetValues<TItem>(this IList<TItem> list, Func<int, TItem, TItem> getNewValue)
	{
		for(int index = 0; index < list.Count; index++)
			list[index] = getNewValue(index, list[index]);
	}

	/// <summary>Set each item in the list to a new value</summary>
	/// <typeparam name="TItem">List item type</typeparam>
	/// <param name="list">List to process</param>
	/// <param name="getNewValue">Function that accepts the item index and returns the new value</param>
	public static void SetValues<TItem>(this IList<TItem> list, Func<int, TItem> getNewValue)
		=> list.SetValues((index, _) => getNewValue(index));

	/// <summary>Set each item in the list to a new value</summary>
	/// <typeparam name="TItem">List item type</typeparam>
	/// <typeparam name="TSource">Source item type</typeparam>
	/// <param name="list">List to process</param>
	/// <param name="sourceList">Source items to convert</param>
	/// <param name="convertValue">Function that accepts the item index and source value and returns the new value</param>
	public static void SetValuesFrom<TItem, TSource>(this IList<TItem> list, IReadOnlyList<TSource> sourceList, Func<int, TSource, TItem> convertValue)
	{
		for(int index = 0; index < list.Count; index++)
		{
			TSource sourceItem = sourceList[index];
			list[index] = convertValue(index, sourceItem);
		}
	}

	/// <summary>Set each item in the list to a new value</summary>
	/// <typeparam name="TItem">List item type</typeparam>
	/// <typeparam name="TSource">Source item type</typeparam>
	/// <param name="list">List to process</param>
	/// <param name="sourceList">Source items to convert</param>
	/// <param name="loadValue">Function that accepts the item index and source value and loads the new value asynchronously</param>
	/// <returns>Async task</returns>
	public static async Task SetValuesFrom<TItem, TSource>(this IList<TItem> list, IReadOnlyList<TSource> sourceList, Func<int, TSource, Task<TItem>> loadValue)
	{
		for(int index = 0; index < list.Count; index++)
		{
			TSource sourceItem = sourceList[index];
			list[index] = await loadValue(index, sourceItem);
		}
	}

	/// <summary>Set each item in the list to a new value</summary>
	/// <typeparam name="TItem">List item type</typeparam>
	/// <typeparam name="TSource">Source item type</typeparam>
	/// <param name="list">List to process</param>
	/// <param name="sourceList">Source items to convert</param>
	/// <param name="convertValue">Function that accepts the source value and returns the new value</param>
	public static void SetValuesFrom<TItem, TSource>(this IList<TItem> list, IReadOnlyList<TSource> sourceList, Func<TSource, TItem> convertValue)
		=> list.SetValuesFrom(sourceList, (_, value) => convertValue(value));

	/// <summary>Set each item in the list to a new value</summary>
	/// <typeparam name="TItem">List item type</typeparam>
	/// <typeparam name="TSource">Source item type</typeparam>
	/// <param name="list">List to process</param>
	/// <param name="sourceList">Source items to convert</param>
	/// <param name="loadValue">Function that accepts the source value and loads the new value asynchronously</param>
	/// <returns>Async task</returns>
	public static async Task SetValuesFrom<TItem, TSource>(this IList<TItem> list, IReadOnlyList<TSource> sourceList, Func<TSource, Task<TItem>> loadValue)
		=> await list.SetValuesFrom(sourceList, (_, value) => loadValue(value));

	/// <summary>Shuffle the list randomly.</summary>
	/// <typeparam name="T">Type of item in list</typeparam>
	/// <param name="list">the list to shuffle.</param>
	/// <param name="rnd"><see cref="Random" /></param>
	/// <remarks>Uses the Fisher-Yates shuffle.</remarks>
	public static void Shuffle<T>(this IList<T> list, Random rnd)
	{
		for(int sourceIndex = list.Count - 1; sourceIndex > 0; sourceIndex--)
		{
			int targetIndex = rnd.Next(0, sourceIndex + 1);
			if(sourceIndex != targetIndex)
				list.Swap(sourceIndex, targetIndex);
		}
	}

	/// <summary>Sort a list in the direction specified</summary>
	/// <typeparam name="TItem">List item type</typeparam>
	/// <param name="list">List to sort</param>
	/// <param name="direction">Sort direction</param>
	public static void Sort<TItem>(this List<TItem> list, ListSortDirection direction)
	{
		Comparison<TItem> compareItems = GetComparer<TItem>(direction);
		list.Sort(compareItems);
	}

	/// <summary>Sort a list by a selected value in the direction specified</summary>
	/// <typeparam name="TItem">List item type</typeparam>
	/// <typeparam name="TValue">Sort value type</typeparam>
	/// <param name="list">List to sort</param>
	/// <param name="getValue">Function to select the value to sort by</param>
	/// <param name="direction">Sort direction</param>
	public static void Sort<TItem, TValue>(this List<TItem> list, Func<TItem, TValue> getValue, ListSortDirection direction)
	{
		Comparison<TValue> compareValues = GetComparer<TValue>(direction);
		int CompareItems(TItem a, TItem b) => compareValues(getValue(a), getValue(b));
		list.Sort(CompareItems);
	}

	/// <summary>Swap two elements in a list</summary>
	/// <typeparam name="T">Type of item in the list</typeparam>
	/// <param name="list"><see cref="IList{T}" /></param>
	/// <param name="a">Item a to be swapped (first)</param>
	/// <param name="b">Item b to be swapped (second)</param>
	public static void Swap<T>(this IList<T> list, int a, int b) => (list[b], list[a]) = (list[a], list[b]);

	/// <summary>Given a list, convert it to a <see cref="DictionaryList{TKey, TValue}"/></summary>
	/// <typeparam name="TKey">Not null key.</typeparam>
	/// <typeparam name="TValue">Items in list</typeparam>
	/// <param name="source">List itself</param>
	/// <param name="selector">Key selection function</param>
	/// <returns><see cref="DictionaryList{TKey, TValue}"/></returns>
	public static DictionaryList<TKey, TValue> ToDictionaryList<TKey, TValue>(this List<TValue> source, Func<TValue, TKey> selector)
		where TKey : notnull
	{
		DictionaryList<TKey, TValue> dictionary = [];

		foreach(IGrouping<TKey, TValue> grp in source.GroupBy(selector))
			dictionary.Add(grp.Key, grp.ToList());

		return dictionary;
	}

	/// <inheritdoc cref="ToDictionaryList{TKey, TValue}(List{TValue}, Func{TValue, TKey})"/>
	public static IReadOnlyDictionaryList<TKey, TValue> ToDictionaryList<TKey, TValue>(this IReadOnlyList<TValue> source, Func<TValue, TKey> selector)
		where TKey : notnull
	{
		DictionaryList<TKey, TValue> dictionary = [];

		foreach(IGrouping<TKey, TValue> grp in source.GroupBy(selector))
			dictionary.Add(grp.Key, grp.ToList());

		return dictionary;
	}

	/// <summary>Convert a list into a 2-dimensional array</summary>
	/// <typeparam name="T">The item type.</typeparam>
	/// <param name="source">Source list.</param>
	/// <param name="columns">The columns in the grid.</param>
	/// <param name="skippedSpaces">The number of spots to skip in the first row.</param>
	/// <returns>The 2D array.</returns>
	public static T[][] To2DArray<T>(this IReadOnlyList<T> source, int columns, int skippedSpaces)
	{
		if(columns <= 0)
			throw new ArgumentOutOfRangeException(nameof(columns));
		if(skippedSpaces < 0 || skippedSpaces >= columns)
			throw new ArgumentOutOfRangeException(nameof(skippedSpaces));
		int rows = (source.Count + skippedSpaces + columns - 1) / columns;
		T[][] result = new T[rows][];
		for(int i = 0; i < rows; i++)
		{
			int start = i * columns - skippedSpaces;
			int end = Math.Min(start + columns, source.Count);
			if(start < 0)
				start = 0;

			int length = end - start;
			result[i] = new T[length];
			for(int j = 0; j < length; j++)
				result[i][j] = source[start + j];
		}

		return result;
	}

	private static Comparison<TValue> GetComparer<TValue>(ListSortDirection direction)
	{
		Comparer<TValue> defaultComparer = Comparer<TValue>.Default;
		int directionNum = direction switch
		{
			ListSortDirection.Ascending => 1,
			ListSortDirection.Descending => -1,
			_ => throw new ArgumentOutOfRangeException(nameof(direction)),
		};

		int CompareValues(TValue a, TValue b) => directionNum * defaultComparer.Compare(a, b);
		return CompareValues;
	}
}
