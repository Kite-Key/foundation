namespace KiteKey.Core.Caching;

/// <summary>Cache that stores loaded values by their key</summary>
/// <typeparam name="TKey">Input key type</typeparam>
/// <typeparam name="TValue">Output value type</typeparam>
public abstract class KeyedCacheBase<TKey, TValue>
	where TKey : notnull
{
	/// <summary>Cache value entries by key</summary>
	protected Dictionary<TKey, TValue> CachedValues { get; } = new();

	/// <summary>Clear all cache entries</summary>
	public void Clear()
		=> CachedValues.Clear();

	/// <summary>Clear one cache entry</summary>
	/// <param name="key">Key of the cached value to clear</param>
	/// <returns>Whether the cached value existed and was cleared</returns>
	public bool Remove(TKey key)
		=> CachedValues.Remove(key);
}
