namespace KiteKey.Core.Utility.Files;

/// <summary>Information about an entry (which may be a file, directory, or non-existent) from a <see cref="IFileDirectory" /></summary>
public interface IFileEntry
{
	/// <summary>Whether the file actually exists</summary>
	bool Exists { get; }

	/// <summary>Whether this item is a directory (otherwise, it's a data file)</summary>
	bool IsDirectory { get; }

	/// <summary>Last modification time</summary>
	DateTimeOffset? LastModified { get; }

	/// <summary>Size of the file, in bytes</summary>
	long? Length { get; }

	/// <summary>Name of the file (not including any path)</summary>
	string Name { get; }

	/// <summary>Full path to the file, including the file name</summary>
	string PhysicalPath { get; }

	/// <summary>Read file contents as a stream</summary>
	/// <returns><see cref="Stream" /> owned by the caller (call <see cref="IDisposable.Dispose" /> when done)</returns>
	Stream CreateReadStream();

	/// <summary>Delete the file</summary>
	void Delete();
}
