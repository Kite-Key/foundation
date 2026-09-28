using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace KiteKey.Core.Utility.Files;

/// <summary>Information about a file in a <see cref="FileRotation" /></summary>
public class RotationFileInfo
{
	/// <summary>File being managed</summary>
	public IFileEntry Entry { get; }

	/// <summary>Extra file name parts</summary>
	public IReadOnlyList<string> Names { get; }

	/// <summary>Timestamp when the file was created</summary>
	public DateTime Time { get; }

	/// <summary>Constructor with known values and debug validation</summary>
	public RotationFileInfo(Regex fileNameRegex, IFileEntry fileEntry, DateTime timestamp, IReadOnlyList<string> nameParts)
	{
		Entry = fileEntry;
		Names = nameParts;
		Time = timestamp;

		ValidateParsing(fileNameRegex);
	}

	private RotationFileInfo(IFileEntry fileEntry, IReadOnlyList<string> names, DateTime time)
	{
		Entry = fileEntry;
		Names = names;
		Time = time;
	}

	/// <summary>Parse from an existing file name</summary>
	/// <param name="fileNameRegex"><see cref="Regex" /> to parse file name</param>
	/// <param name="fileEntry">Source file</param>
	/// <returns>Parsed data</returns>
	public static RotationFileInfo? ParseFromFile(Regex fileNameRegex, IFileEntry fileEntry)
	{
		ParsedValues? values = ParseValues(fileNameRegex, fileEntry);
		if(values is null)
			return null;
		else
			return new RotationFileInfo(fileEntry, values.NameParts, values.Timestamp);
	}

	private static int GetGroupInt(Match match, string groupName)
	{
		string value = match.GetRequiredGroup(groupName);
		return int.Parse(value, NumberStyles.None);
	}

	private static ParsedValues? ParseValues(Regex fileNameRegex, IFileEntry fileEntry)
	{
		Match nameMatch = fileNameRegex.Match(fileEntry.Name);
		if(!nameMatch.Success)
			return null;

		int year = GetGroupInt(nameMatch, "year");
		int month = GetGroupInt(nameMatch, "month");
		int day = GetGroupInt(nameMatch, "day");
		int hour = GetGroupInt(nameMatch, "hour");
		int minute = GetGroupInt(nameMatch, "minute");
		int second = GetGroupInt(nameMatch, "second");
		int fraction = GetGroupInt(nameMatch, "fraction");

		DateTime timestamp = new(year, month, day, hour, minute, second, fraction * 10, DateTimeKind.Local);
		IReadOnlyList<string> nameParts = nameMatch.Groups["name"].Captures.Select(c => c.Value).ToList();
		return new ParsedValues(timestamp, nameParts);
	}

	[Conditional("DEBUG")]
	private void ValidateParsing(Regex fileNameRegex)
	{
		ParsedValues? values = ParseValues(fileNameRegex, Entry);
		Debug.Assert(values is not null, $"Invalid file name format. \"{Entry.Name}\" does not match \"{fileNameRegex}\"");

		TimeSpan diff = values.Timestamp - Time;
		Debug.Assert(Math.Abs(diff.TotalSeconds) < 1, "Failed to parse timestamp from file name");

		Debug.Assert(values.NameParts.SequenceEqual(Names), "Failed to parse name parts from file name");
	}

	private record ParsedValues(DateTime Timestamp, IReadOnlyList<string> NameParts);
}
