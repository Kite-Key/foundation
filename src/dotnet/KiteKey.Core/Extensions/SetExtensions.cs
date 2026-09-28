namespace KiteKey.Core;

/// <summary>Extensions for <see cref="ISet{T}"/></summary>
public static class SetExtensions
{
	/// <summary>Add multiple values</summary>
	/// <typeparam name="T">Value type</typeparam>
	/// <param name="set">Set to update</param>
	/// <param name="values">Values to add</param>
	public static void AddRange<T>(this ISet<T> set, IEnumerable<T> values)
	{
		foreach(T value in values)
			set.Add(value);
	}
}
