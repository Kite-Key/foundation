using System.Collections;
using System.Text.RegularExpressions;
using KiteKey.Core.Utility.Files.Physical;

namespace KiteKey.Core.Utility.Files;

/// <summary>Manages a collection of files where the oldest get deleted as new files are created</summary>
public class FileRotation : IEnumerable<RotationFileInfo>
{
	private const string _nameSeparator = "-";
	private const string _partSeparator = "_";

	private readonly string _fileExtension;
	private readonly Regex _fileNameRegex;
	private readonly int _nameCount;

	/// <inheritdoc cref="IFileDirectory" />
	public IFileDirectory FileDirectory { get; }

	/// <summary>Constructor</summary>
	/// <param name="directoryPath">Path to local physical directory where files are located</param>
	/// <param name="nameCount">Number of file name parts (in addition to the timestamp)</param>
	/// <param name="fileExtension">File extension (without dot)</param>
	public FileRotation(string directoryPath, int nameCount, string fileExtension)
		: this(new PhysicalFileDirectory(directoryPath), nameCount, fileExtension) { }

	/// <summary>Constructor</summary>
	/// <param name="fileDirectory">Directory where files are located (local or remote)</param>
	/// <param name="nameCount">Number of file name parts (in addition to the timestamp)</param>
	/// <param name="fileExtension">File extension (without dot)</param>
	public FileRotation(IFileDirectory fileDirectory, int nameCount, string fileExtension)
	{
		ArgumentNullException.ThrowIfNull(fileDirectory);
		FileDirectory = fileDirectory;
		_nameCount = nameCount;
		_fileExtension = fileExtension;

		RegexParams regexParams = new(nameCount, fileExtension);
		_fileNameRegex = BuildNameRegex(regexParams);
	}

	/// <summary>Get the name of a new file to add to the rotation</summary>
	/// <returns>New file information</returns>
	/// <remarks>This method does not actually create the new file</remarks>
	public RotationFileInfo CreateFileName(IReadOnlyList<string> nameParts)
		=> CreateFileName(DateTime.Now, nameParts);

	/// <summary>Get the name of a new file to add to the rotation</summary>
	/// <param name="timestamp">Timestamp to use in the file name</param>
	/// <param name="nameParts">File name parts</param>
	/// <returns>New file information</returns>
	/// <remarks>This method does not actually create the new file</remarks>
	public RotationFileInfo CreateFileName(DateTime timestamp, IReadOnlyList<string> nameParts)
	{
		if(nameParts.Count != _nameCount)
			throw new ArgumentOutOfRangeException(nameof(nameParts));

		string namePrefix = string.Join(_nameSeparator, nameParts);
		string fileName = $"{namePrefix}{_partSeparator}{timestamp:yyyy-MM-dd-HH-mm-ss-ff}.{_fileExtension}";
		if(!_fileNameRegex.IsMatch(fileName))
			throw new ArgumentException("File name parts must contain only letters, digits, or underscores.", nameof(nameParts));

		IFileEntry fileInfo = FileDirectory.GetEntry(fileName);
		if(fileInfo.Exists)
			throw new IOException("Failed to allocate next file in the rotation"); // Caller is trying to create files too quickly

		return new RotationFileInfo(_fileNameRegex, fileInfo, timestamp, nameParts);
	}

	/// <inheritdoc />
	public IEnumerator<RotationFileInfo> GetEnumerator()
		=> ListFiles().GetEnumerator();

	/// <summary>Find all existing files in the rotation</summary>
	/// <returns>List of files in the directory matching the file name prefix and extension</returns>
	public IEnumerable<RotationFileInfo> ListFiles()
	{
		if(!FileDirectory.DirectoryEntry.Exists)
			yield break;

		foreach(IFileEntry file in FileDirectory)
		{
			RotationFileInfo? rotationFile = RotationFileInfo.ParseFromFile(_fileNameRegex, file);
			if(rotationFile is not null)
				yield return rotationFile;
		}
	}

	/// <summary>Delete old files.</summary>
	/// <param name="maxAge">Oldest age for a file to be retained, or null if no maximum age.</param>
	/// <param name="maxCount">Maximum number of files to retain, or null if no max file count.</param>
	/// <returns>Number of files deleted</returns>
	/// <remarks>If neither parameter is set, then no pruning occurs.</remarks>
	public int PruneFiles(TimeSpan? maxAge, int? maxCount)
	{
		if(maxCount < 0)
			throw new ArgumentOutOfRangeException(nameof(maxCount));
		if(maxAge < TimeSpan.Zero)
			throw new ArgumentOutOfRangeException(nameof(maxAge));

		DateTime? minTime = DateTime.Now - maxAge;
		List<RotationFileInfo>? sortedFiles = maxCount.HasValue ? [] : null;
		int deleteCount = 0;

		foreach(RotationFileInfo file in ListFiles())
		{
			if(minTime.HasValue && file.Time < minTime)
			{
				file.Entry.Delete();
				deleteCount++;
			}
			else
			{
				sortedFiles?.Add(file);
			}
		}

		if(sortedFiles is not null && maxCount.HasValue)
		{
			sortedFiles.Sort((x, y) => y.Time.CompareTo(x.Time));
			for(int i = maxCount.Value; i < sortedFiles.Count; i++)
			{
				RotationFileInfo file = sortedFiles[i];
				file.Entry.Delete();
				deleteCount++;
			}
		}

		return deleteCount;
	}

	/// <inheritdoc />
	IEnumerator IEnumerable.GetEnumerator()
		=> GetEnumerator();

	private static Regex BuildNameRegex(RegexParams rp)
	{
		// lang=regex
		const string timestampRegex = @"(?<year>\d{4})-(?<month>\d{2})-(?<day>\d{2})-(?<hour>\d{2})-(?<minute>\d{2})-(?<second>\d{2})-(?<fraction>\d{2})";

		// lang=regex
		const string nameRegex = @"(?<name>[\w\d]+)";

		if(rp.NameCount is not (> 0 and < 10))
			throw new ArgumentOutOfRangeException(nameof(rp));

		if(rp.FileExtension.Length is not (> 0 and < 10))
			throw new ArgumentOutOfRangeException(nameof(rp));

		IEnumerable<string> nameRegexes = Enumerable.Repeat(nameRegex, rp.NameCount);
		string namesRegex = string.Join(_nameSeparator, nameRegexes);
		string extensionRegex = Regex.Escape(rp.FileExtension);

		return new Regex(@$"^{namesRegex}{_partSeparator}{timestampRegex}\.{extensionRegex}$", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
	}

	private record RegexParams(int NameCount, string FileExtension);
}
