namespace KiteKey.Core.Caching;

/// <summary>Cache that stores asynchronously loaded values by their key</summary>
/// <typeparam name="TKey">Input key type</typeparam>
/// <typeparam name="TValue">Output value type</typeparam>
public class KeyedCacheAsync<TKey, TValue> : KeyedCacheBase<TKey, TValue>
	where TKey : notnull
{
	private readonly Func<TKey, CancellationToken, Task<TValue>> _loadValue;

	/// <summary>Constructor</summary>
	/// <param name="loadValue">Function to load a value</param>
	public KeyedCacheAsync(Func<TKey, CancellationToken, Task<TValue>> loadValue)
		=> _loadValue = loadValue;

	/// <summary>Constructor</summary>
	/// <param name="loadValue">Function to load a value</param>
	public KeyedCacheAsync(Func<TKey, Task<TValue>> loadValue)
		: this((key, _) => loadValue(key)) { }

	/// <summary>Get the value associated with <paramref name="key" /> from the cache or by loading it, if necessary</summary>
	/// <param name="key">Lookup key that describes the value to load</param>
	/// <param name="cancellationToken">Token to cancel the task safely</param>
	/// <returns>Cached or loaded value</returns>
	public async Task<TValue> GetValue(TKey key, CancellationToken cancellationToken = default)
	{
		if(!CachedValues.TryGetValue(key, out TValue? value))
		{
			value = await _loadValue(key, cancellationToken);
			CachedValues.Add(key, value);
		}

		return value;
	}
}
