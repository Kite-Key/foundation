using System.Collections;

namespace KiteKey.Core.Text;

/// <summary>Reads lines of text from a <see cref="ReadOnlySpan{T}"/>, like a <see cref="TextReader"/></summary>
public readonly ref struct SpanReader
{
	private readonly ReadOnlySpan<char> _source;

	/// <summary>Constructor</summary>
	public SpanReader(ReadOnlySpan<char> source)
		=> _source = source;

	/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
	public LineEnumerator GetEnumerator()
		=> EnumerateLines();

	/// <summary>Enumerate each line of text in the source string</summary>
	/// <returns><see cref="IEnumerable{T}"/> for reading lines</returns>
	public LineEnumerator EnumerateLines()
		=> new(_source);

	/// <summary>Enumerate indices of each line of text in the source string</summary>
	/// <returns><see cref="IEnumerable{T}"/> for reading line indices</returns>
	public RangeEnumerator EnumerateRanges()
		=> new(_source);

	/// <summary><see cref="IEnumerable{T}"/> for reading lines</summary>
	public ref struct LineEnumerator
	{
		private readonly ReadOnlySpan<char> _source;
		private RangeEnumerator _ranges;

		/// <inheritdoc cref="IEnumerator{T}.Current"/>
		public ReadOnlySpan<char> Current { get; private set; }

		/// <summary>Constructor</summary>
		public LineEnumerator(ReadOnlySpan<char> source)
		{
			_source = source;
			_ranges = new RangeEnumerator(source);
		}

		/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
		public readonly LineEnumerator GetEnumerator()
			=> this;

		/// <inheritdoc cref="IEnumerator.MoveNext"/>
		public bool MoveNext()
		{
			bool found = _ranges.MoveNext();
			Current = _source[_ranges.Current].Trim();
			return found;
		}

		/// <inheritdoc cref="IEnumerator.Reset"/>
		public void Reset()
		{
			_ranges.Reset();
			Current = default;
		}
	}

	/// <summary><see cref="IEnumerable{T}"/> for reading line indices</summary>
	public ref struct RangeEnumerator
	{
		private readonly ReadOnlySpan<char> _source;
		private int _index;

		/// <inheritdoc cref="IEnumerator{T}.Current"/>
		public Range Current { get; private set; }

		/// <summary>Constructor</summary>
		public RangeEnumerator(ReadOnlySpan<char> source)
			=> _source = source;

		/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
		public readonly RangeEnumerator GetEnumerator()
			=> this;

		/// <inheritdoc cref="IEnumerator.MoveNext"/>
		public bool MoveNext()
		{
			bool found = TryReadRange(out Range range);
			Current = range;
			return found;
		}

		/// <inheritdoc cref="IEnumerator.Reset"/>
		public void Reset()
		{
			_index = 0;
			Current = default;
		}

		private bool TryReadRange(out Range range)
		{
			if(_index >= _source.Length)
			{
				range = default;
				return false;
			}

			int endIndex;
			int newlineIndex = _source[_index..].IndexOf('\n') + 1;
			if(newlineIndex > 0)
				endIndex = _index + newlineIndex;
			else
				endIndex = _source.Length;

			range = _index..endIndex;
			_index = endIndex;
			return true;
		}
	}
}
