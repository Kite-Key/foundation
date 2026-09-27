namespace KiteKey.Core.Caching;

/// <summary>Cache that stores synchronously loaded values by their key</summary>
/// <typeparam name="TKey">Input key type</typeparam>
/// <typeparam name="TArg">Input additional load argument type</typeparam>
/// <typeparam name="TValue">Output value type</typeparam>
public class KeyedCacheSync<TKey, TArg, TValue> : KeyedCacheBase<TKey, TValue>
	where TKey : notnull
{
	private readonly Func<TKey, TArg, TValue> _loadValue;

	/// <summary>Constructor</summary>
	/// <param name="loadValue">Function to load a value</param>
	public KeyedCacheSync(Func<TKey, TArg, TValue> loadValue)
		=> _loadValue = loadValue;

	/// <summary>Get the value associated with <paramref name="key" /> from the cache or by loading it, if necessary</summary>
	/// <param name="key">Lookup key that describes the value to load</param>
	/// <param name="extraArg">Additional argument data, only used if this is the first time loading this value</param>
	/// <returns>Cached or loaded value</returns>
	public TValue GetValue(TKey key, TArg extraArg)
	{
		if(!CachedValues.TryGetValue(key, out TValue? value))
		{
			value = _loadValue(key, extraArg);
			CachedValues.Add(key, value);
		}

		return value;
	}
}
