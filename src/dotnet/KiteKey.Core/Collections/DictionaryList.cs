using System.Diagnostics.CodeAnalysis;

namespace KiteKey.Core.Collections;

/// <summary>Keyed collection with multiple values per key (like a <see cref="Dictionary{TKey, TValue}"/> of <see cref="List{T}"/>).</summary>
/// <typeparam name="TKey">Key type.</typeparam>
/// <typeparam name="TValue">Value type.</typeparam>
public class DictionaryList<TKey, TValue> : Dictionary<TKey, List<TValue>>, IReadOnlyDictionaryList<TKey, TValue>
    where TKey : notnull
{
    /// <summary>Add an item.</summary>
    /// <param name="key">Item key.</param>
    /// <param name="value">Item value.</param>
    public void Add(TKey key, TValue value)
    {
        if (!this.TryGetValue(key, out List<TValue>? list))
        {
            list = [];
            this.Add(key, list);
        }

        list.Add(value);
    }

    /// <inheritdoc/>
    public IEnumerable<TValue> AllValues
    {
        get
        {
            foreach (IReadOnlyList<TValue> list in this.Values)
            {
                foreach (TValue item in list)
                {
                    yield return item;
                }
            }
        }
    }

    /// <summary>Get the number of <typeparamref name="TKey"/> entries.</summary>
    /// <param name="key">Key to count.</param>
    /// <returns>Number of entries.</returns>
    public int GetCount(TKey key)
    {
        return this.TryGetValue(key, out List<TValue>? list) ? list.Count : 0;
    }

    /// <summary>Check if a given key/value pair has been added.</summary>
    /// <param name="key">Key to check.</param>
    /// <param name="value">Value to check.</param>
    /// <param name="comparer">Override <see cref="IEqualityComparer{T}"/>, or <c>null</c> to use the default.</param>
    /// <returns>Whether <see cref="Add"/> has been called for these values.</returns>
    public bool Contains(TKey key, TValue value, IEqualityComparer<TValue>? comparer = null)
    {
        return this.TryGetValue(key, out List<TValue>? list) && list.Contains(value, comparer);
    }

    /// <inheritdoc/>
    IReadOnlyList<TValue> IReadOnlyDictionary<TKey, IReadOnlyList<TValue>>.this[TKey key] => this[key];

    /// <inheritdoc/>
    IEnumerable<TKey> IReadOnlyDictionary<TKey, IReadOnlyList<TValue>>.Keys => this.Keys;

    /// <inheritdoc/>
    IEnumerable<IReadOnlyList<TValue>> IReadOnlyDictionary<TKey, IReadOnlyList<TValue>>.Values => this.Values;

    /// <inheritdoc/>
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out IReadOnlyList<TValue> value)
    {
        bool ret = this.TryGetValue(key, out List<TValue>? listValue);
        value = listValue;
        return ret;
    }

    /// <inheritdoc/>
    IEnumerator<KeyValuePair<TKey, IReadOnlyList<TValue>>> IEnumerable<KeyValuePair<TKey, IReadOnlyList<TValue>>>.GetEnumerator()
    {
        foreach (KeyValuePair<TKey, List<TValue>> pair in this)
        {
            yield return new KeyValuePair<TKey, IReadOnlyList<TValue>>(pair.Key, pair.Value);
        }
    }
}
