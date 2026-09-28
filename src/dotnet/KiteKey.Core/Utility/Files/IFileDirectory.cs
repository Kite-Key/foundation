using System.Collections;

namespace KiteKey.Core.Utility.Files;

/// <summary>
///     Information about a directory of a file-system (which may be physically attached, embedded, or networked) that contains <see cref="IFileEntry" />
/// </summary>
public interface IFileDirectory : IEnumerable<IFileEntry>
{
	/// <summary><see cref="IFileEntry" /> for the current directory</summary>
	IFileEntry DirectoryEntry { get; }

	/// <summary>Get information about one file or directory</summary>
	/// <param name="name">File or directory name</param>
	/// <returns><see cref="IFileEntry" /> which may not exist, check <see cref="IFileEntry.Exists" /></returns>
	IFileEntry GetEntry(string name);

	/// <summary>Get information about the contents of a sub-directory</summary>
	/// <param name="subPath">Sub-path to open</param>
	/// <returns><see cref="IFileDirectory" /> to inspect the sub-directory</returns>
	IFileDirectory GetSubdirectory(string subPath);

	/// <inheritdoc />
	IEnumerator IEnumerable.GetEnumerator()
		=> GetEnumerator();
}
