namespace KiteKey.Core.Caching;

/// <summary>Cache that stores asynchronously loaded values by their key</summary>
/// <typeparam name="TKey">Input key type</typeparam>
/// <typeparam name="TArg">Input additional load argument type</typeparam>
/// <typeparam name="TValue">Output value type</typeparam>
public class KeyedCacheAsync<TKey, TArg, TValue> : KeyedCacheBase<TKey, TValue>
	where TKey : notnull
{
	private readonly Func<TKey, TArg, CancellationToken, Task<TValue>> _loadValue;

	/// <summary>Constructor</summary>
	/// <param name="loadValue">Function to load a value</param>
	public KeyedCacheAsync(Func<TKey, TArg, CancellationToken, Task<TValue>> loadValue)
		=> _loadValue = loadValue;

	/// <summary>Constructor</summary>
	/// <param name="loadValue">Function to load a value</param>
	public KeyedCacheAsync(Func<TKey, TArg, Task<TValue>> loadValue)
		: this((key, arg, _) => loadValue(key, arg)) { }

	/// <summary>Get the value associated with <paramref name="key" /> from the cache or by loading it, if necessary</summary>
	/// <param name="key">Lookup key that describes the value to load</param>
	/// <param name="extraArg">Additional argument data, only used if this is the first time loading this value</param>
	/// <param name="cancellationToken">Token to cancel the task safely</param>
	/// <returns>Cached or loaded value</returns>
	public async Task<TValue> GetValue(TKey key, TArg extraArg, CancellationToken cancellationToken = default)
	{
		if(!CachedValues.TryGetValue(key, out TValue? value))
		{
			value = await _loadValue(key, extraArg, cancellationToken);
			CachedValues.Add(key, value);
		}

		return value;
	}
}
