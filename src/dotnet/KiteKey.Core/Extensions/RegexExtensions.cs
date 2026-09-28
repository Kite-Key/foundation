using System.Data;
using System.Text.RegularExpressions;

namespace KiteKey.Core;

/// <summary>Extensions methods for <see cref="Regex" /> and <see cref="Match" /></summary>
public static class RegexExtensions
{
	/// <summary>Get the value of an <see cref="Regex" /> optional capture group</summary>
	/// <param name="match"><see cref="Regex" /> match</param>
	/// <param name="groupName">Capture group name</param>
	/// <returns>The group match value, or <c>null</c> if there was no match</returns>
	public static string? GetOptionalGroup(this Match match, string groupName)
	{
		Group group = match.Groups[groupName];
		string value = group.Value;
		if(group.Success && !string.IsNullOrEmpty(value))
			return value;
		else
			return null;
	}

	/// <summary>Get the value of a <see cref="Regex" /> required capture group</summary>
	/// <param name="match"><see cref="Regex" /> match</param>
	/// <param name="groupName">Capture group name</param>
	/// <returns>The group match value</returns>
	public static string GetRequiredGroup(this Match match, string groupName)
	{
		string? value = GetOptionalGroup(match, groupName);
		if(!string.IsNullOrEmpty(value))
			return value;
		else
			throw new DataException(groupName + " is required in match " + match);
	}
}
