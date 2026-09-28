using System.Text;
using System.Text.RegularExpressions;
using KiteKey.Core.Utility;

namespace KiteKey.Core;

/// <summary>Extensions methods for <see cref="string" /></summary>
public static partial class StringExtensions
{
	private static readonly Regex _punctuationRegex = GetPunctuationRegex();
	private static readonly Regex _spacesRegex = GetSpacesRegex();
	private static readonly Regex _lineBreaksRegex = GetNewlineRegex();
	private static readonly Regex _regex = new(@"^\+(\d{1,3})(\d{10,})$");

	/// <summary>Abbreviates a string using the first letters of each word.</summary>
	/// <param name="source">The string to abbreviate</param>
	/// <returns>The string abbreviation</returns>
	[return: NotNullIfNotNull(nameof(source))]
	public static string? Abbreviate(this string? source)
	{
		if(source is null)
			return null;

		source = source
			.RemovePunctuation()
			.CollapseWhitespace();

		string[] wordList = source.Split(' ');
		StringBuilder abbreviation = new();

		foreach(string word in wordList)
		{
			if(word.Length > 0)
				abbreviation.Append(word[0]);
		}

		return abbreviation.ToString().ToUpper();
	}

	/// <summary>Collapse all consecutive whitespace characters to a single space</summary>
	/// <param name="source">Source string to process</param>
	/// <returns><paramref name="source" /> spaces collapsed and leading/trailing whitespace removed</returns>
	[return: NotNullIfNotNull(nameof(source))]
	public static string? CollapseWhitespace(this string? source)
	{
		if(source is null)
			return null;

		string trimmed = source.Trim();
		return _spacesRegex.Replace(trimmed, " ");
	}

	/// <summary>
	/// Formats a Twilio-style phone number (+16089099868) into (XXX) XXX-XXXX.
	/// Displays the country code only if it's not "1".
	/// </summary>
	/// <param name="phoneNumber">Phone number in Twilio format (e.g., "+16089099868").</param>
	/// <returns>Formatted phone number (e.g., "(608) 909-9868" or "+44 20 7946 0958").</returns>
	public static string ToPhoneNumber(this string phoneNumber)
	{
		if(string.IsNullOrWhiteSpace(phoneNumber))
			return string.Empty;

		// Ensure the number starts with "+"
		phoneNumber = phoneNumber.Trim();
		if(!phoneNumber.StartsWith('+'))
			return phoneNumber; // Return as-is if not in international format

		// Extract country code and national number
		Match match = _regex.Match(phoneNumber);
		if(!match.Success)
			return phoneNumber; // Return as-is if it doesn't match the expected pattern

		string countryCode = match.Groups[1].Value; // First group is country code
		string nationalNumber = match.Groups[2].Value; // Second group is the national number

		// Format national number (assumes 10-digit numbers for North America)
		if(nationalNumber.Length == 10)
			nationalNumber = $"({nationalNumber[..3]}) {nationalNumber[3..6]}-{nationalNumber[6..]}";

		// Only show country code if it's not "1" (USA/Canada)
		return countryCode == "1" ? nationalNumber : $"+{countryCode} {nationalNumber}";
	}

	/// <summary>Case insensitive contains comparison.</summary>
	/// <param name="source">The source string.</param>
	/// <param name="compareTo">The string to compare to.</param>
	/// <returns>True if equal, false otherwise.</returns>
	public static bool ContainsInsensitive(this string? source, string? compareTo)
	{
		if(source == null || compareTo == null)
			return false;

		return source.Contains(compareTo, StringComparison.OrdinalIgnoreCase);
	}

	/// <summary>Case insensitive equals comparison.</summary>
	/// <param name="source">The source string.</param>
	/// <param name="compareTo">The string to compare to.</param>
	/// <returns>True if equal, false otherwise.</returns>
	public static bool EqualsInsensitive(this string? source, string? compareTo)
		=> string.Equals(source, compareTo, StringComparison.OrdinalIgnoreCase);

	/// <summary>
	/// Randomize the contents of a string, returning a string of <em>similar</em> length.
	/// </summary>
	/// <param name="source">Source string</param>
	/// <returns>A string of similar length to that of <paramref name="source"/> but with randomly generated characters.</returns>
	public static string Obfuscate(this string? source)
	{
		if(source is null)
			return StringHelper.GetRandomString();
		else
			return StringHelper.GetRandomString(Math.Max(source.Length - 2, 1), Math.Max(source.Length + 2, 2));
	}

	/// <summary>Remove everything in the source after a found substring</summary>
	/// <param name="source">String to process</param>
	/// <param name="toRemove">Substring to find</param>
	/// <param name="comparison"><see cref="StringComparison" /></param>
	/// <returns>Everything before <paramref name="toRemove" /> if found, or <paramref name="source" /> if not found</returns>
	/// <example><c>"Funco (formerly Sadco)".RemoveAfter(" (formerly ") == "Funco"</c></example>
	public static string RemoveAfter(this string source, string toRemove, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
	{
		int index = source.IndexOf(toRemove, comparison);
		if(index < 0)
			return source;

		string removed = source[..index];
		return removed.Trim();
	}

	/// <summary>Remove the end of the string if it matches given the substring</summary>
	/// <param name="source">String to process</param>
	/// <param name="toRemove">Substring to find</param>
	/// <param name="comparison"><see cref="StringComparison" /></param>
	/// <returns>Everything before <paramref name="toRemove" /> if found, or <paramref name="source" /> if not found</returns>
	public static string RemoveEnd(this string source, string toRemove, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
	{
		bool found = source.EndsWith(toRemove, comparison);
		if(!found)
			return source;

		int end = source.Length - toRemove.Length;
		string removed = source[..end];
		return removed.Trim();
	}

	/// <summary>Remove the start of the string if it matches given the substring</summary>
	/// <param name="source">String to process</param>
	/// <param name="toRemove">Substring to find</param>
	/// <param name="comparison"><see cref="StringComparison" /></param>
	/// <returns>Everything after <paramref name="toRemove" /> if found, or <paramref name="source" /> if not found</returns>
	public static string RemoveStart(this string source, string toRemove, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
	{
		bool found = source.StartsWith(toRemove, comparison);
		if(!found)
			return source;

		string removed = source[toRemove.Length..];
		return removed.Trim();
	}

	/// <summary>Removes all non-alphanumeric characters from the string.</summary>
	/// <param name="source">The string to modify.</param>
	/// <returns><paramref name="source" /> without quotes and other non-alphanumerics.</returns>
	[return: NotNullIfNotNull(nameof(source))]
	public static string? RemovePunctuation(this string? source)
	{
		if(source is null)
			return null;
		else
			return _punctuationRegex.Replace(source, "");
	}

	/// <summary>Removes all line break characters from the string.</summary>
	/// <param name="source">The string to modify.</param>
	/// <returns><paramref name="source" /> without line breaks</returns>
	[return: NotNullIfNotNull(nameof(source))]
	public static string? RemoveLineBreaks(this string? source)
	{
		if(source is null)
			return null;
		else
			return _lineBreaksRegex.Replace(source, "");
	}

	/// <summary>Removes all whitespace characters from the string.</summary>
	/// <param name="source">The string to modify.</param>
	/// <returns><paramref name="source" /> without whitespace.</returns>
	[return: NotNullIfNotNull(nameof(source))]
	public static string? RemoveWhitespace(this string? source)
	{
		if(source is null)
			return null;
		else
			return _spacesRegex.Replace(source, "");
	}

	/// <summary>Remove words from a string</summary>
	/// <param name="source">Source string to process</param>
	/// <param name="wordsToRemove">Words to remove</param>
	/// <returns><paramref name="source" /> without <paramref name="wordsToRemove" /></returns>
	public static string RemoveWords(this string source, params string[] wordsToRemove)
	{
		IEnumerable<string> wordsEscaped = wordsToRemove.Select(Regex.Escape);
		string regexString = @"\b(" + string.Join('|', wordsEscaped) + @")\b";
		string replaced = Regex.Replace(source, regexString, "", RegexOptions.IgnoreCase);
		return replaced.CollapseWhitespace();
	}

	/// <summary>Detects if a string is all upper case letters.</summary>
	/// <param name="source">Source string to process</param>
	/// <returns><c>true</c> if no lowercase characters, <c>false</c> otherwise</returns>
	public static bool IsUpper(this string source)
		=> !source.Any(char.IsLower);

	/// <summary>Take everything in the source after a found substring</summary>
	/// <param name="source">String to process</param>
	/// <param name="toTake">Substring to find</param>
	/// <param name="comparison"><see cref="StringComparison" /></param>
	/// <returns>Everything after <paramref name="toTake" /> if found, or <paramref name="source" /> if not found</returns>
	/// <example><c>"Sadco dba Funco".TakeAfter(" dba ") == "Funco"</c></example>
	public static string TakeAfter(this string source, string toTake, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
	{
		int index = source.IndexOf(toTake, comparison);
		if(index < 0)
			return source;

		int takeIndex = index + toTake.Length;
		string removed = source[takeIndex..];
		return removed.Trim();
	}

	/// <summary>Convert a string to a boolean</summary>
	/// <param name="value">String to parse ("true", "t", "1", "y", "yes", etc)</param>
	/// <returns>Parsed boolean</returns>
	public static bool ToBoolean(this string value)
	{
		return value.ToUpperInvariant() switch
		{
			"TRUE" or "T" or "1" or "Y" or "YES" => true,
			"FALSE" or "F" or "0" or "N" or "NO" => false,
			_ => throw new ArgumentOutOfRangeException(nameof(value)),
		};
	}

	/// <summary>Extension method to return an enum value of type T for the given list of strings.</summary>
	/// <typeparam name="T">The enum.</typeparam>
	/// <param name="value">.</param>
	/// <returns>The enum values in the list.</returns>
	public static IEnumerable<T> ToEnum<T>(this IEnumerable<string> value)
		where T : struct, Enum
		=> value.Select(str => str.ToEnum<T>());

	/// <summary>Convert a string to a <see cref="Guid" /></summary>
	/// <param name="value">String to parse</param>
	/// <returns>Parsed <see cref="Guid" /></returns>
	public static Guid ToGuid(this string value)
		=> Guid.Parse(value);

	/// <summary>Truncate a string.</summary>
	/// <param name="source">The source string.</param>
	/// <param name="maxLength">The maximum numbers of characters to include from <paramref name="source" />.</param>
	/// <param name="truncationSuffix">A suffix to append if longer than <paramref name="maxLength" />. Defaults to nothing.</param>
	/// <returns>The truncated string, or <c>null</c> if <paramref name="source" /> is <c>null</c>.</returns>
	[return: NotNullIfNotNull(nameof(source))]
	public static string? Truncate(this string? source, int maxLength, string truncationSuffix = "")
	{
		if(source is null)
			return null;
		else if(source.Length > maxLength)
			return source[..maxLength] + truncationSuffix;
		else
			return source;
	}

	/// <summary>Converts a name to possessive.</summary>
	/// <param name="name">The name to change to possessive.</param>
	/// <returns>The possessive form of a word.</returns>
	[return: NotNullIfNotNull(nameof(name))]
	public static string? ToPossessive(this string? name)
	{
		if(string.IsNullOrEmpty(name))
			return null;

		char lastChar = name[^1];

		if(lastChar is 's' or 'S')
			return name + "'";
		else
			return name + "'s";
	}

	[GeneratedRegex("[^\\w\\s]")]
	private static partial Regex GetPunctuationRegex();

	[GeneratedRegex("\\s+")]
	private static partial Regex GetSpacesRegex();

	[GeneratedRegex("(\r|\n|\r\n)*")]
	private static partial Regex GetNewlineRegex();
}
