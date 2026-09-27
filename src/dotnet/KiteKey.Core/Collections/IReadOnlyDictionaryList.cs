namespace KiteKey.Core.Collections;

/// <summary>Read-only version of <see cref="DictionaryList{TKey, TValue}"/></summary>
/// <typeparam name="TKey">Key type</typeparam>
/// <typeparam name="TValue">Value type</typeparam>

public interface IReadOnlyDictionaryList<TKey, TValue> : IReadOnlyDictionary<TKey, IReadOnlyList<TValue>>
	where TKey : notnull
{
	/// <summary>All <typeparamref name="TValue"/> across all lists</summary>
	/// <remarks>Items may be repeated if they have been added multiple times, distinctness is not guaranteed</remarks>
	IEnumerable<TValue> AllValues { get; }
}
