using System.Runtime.CompilerServices;

namespace KiteKey.Core;

/// <summary>Extensions for <see cref="TextReader"/></summary>
public static class TextReaderExtensions
{
	/// <summary>Enumerate all text lines</summary>
	/// <param name="reader">Source <see cref="TextReader"/></param>
	/// <returns>All text lines returned by <see cref="TextReader.ReadLine"/></returns>
	public static IEnumerable<string> EnumerateLines(this TextReader reader)
	{
		for(string? line = reader.ReadLine(); line is not null; line = reader.ReadLine())
			yield return line;
	}

	/// <summary>Enumerate all text lines asynchronously</summary>
	/// <param name="reader">Source <see cref="TextReader"/></param>
	/// <param name="cancellationToken"><see cref="CancellationToken"/></param>
	/// <returns>All text lines returned by <see cref="TextReader.ReadLineAsync(CancellationToken)"/></returns>
	public static async IAsyncEnumerable<string> EnumerateLinesAsync(this TextReader reader, [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		string? line = await reader.ReadLineAsync(cancellationToken);
		while(line is not null)
		{
			cancellationToken.ThrowIfCancellationRequested();
			yield return line;
			line = await reader.ReadLineAsync(cancellationToken);
		}
	}
}
