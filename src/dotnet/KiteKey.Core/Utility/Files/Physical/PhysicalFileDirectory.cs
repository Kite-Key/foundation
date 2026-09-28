namespace KiteKey.Core.Utility.Files.Physical;

/// <summary><see cref="IFileDirectory" /> for physical files</summary>
public class PhysicalFileDirectory : IFileDirectory
{
	private readonly PhysicalFileEntryDirectory _directory;

	/// <inheritdoc />
	public IFileEntry DirectoryEntry => _directory;

	/// <summary>Constructor</summary>
	/// <param name="baseDirectory">Path of directory on which to operate</param>
	public PhysicalFileDirectory(string baseDirectory)
		=> _directory = new PhysicalFileEntryDirectory(baseDirectory);

	/// <inheritdoc />
	public IFileEntry GetEntry(string name)
	{
		string fullPath = ResolveWithinDirectory(name);
		if(File.Exists(fullPath))
			return new PhysicalFileEntry(fullPath);
		else if(Directory.Exists(fullPath))
			return new PhysicalFileEntryDirectory(fullPath);
		else
			return new NotFoundFileEntry(name, fullPath);
	}

	/// <inheritdoc />
	public IEnumerator<IFileEntry> GetEnumerator()
	{
		foreach(FileSystemInfo info in _directory.Info.EnumerateFileSystemInfos())
		{
			yield return info switch
			{
				FileInfo file => new PhysicalFileEntry(file),
				DirectoryInfo directory => new PhysicalFileEntryDirectory(directory),
				_ => throw new IOException($"Unexpected file system entry type {info.GetType()} at {info.FullName}"),
			};
		}
	}

	/// <inheritdoc />
	public IFileDirectory GetSubdirectory(string subPath)
	{
		string fullPath = ResolveWithinDirectory(subPath);
		return new PhysicalFileDirectory(fullPath);
	}

	private string ResolveWithinDirectory(string relativePath)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);
		string fullPath = Path.GetFullPath(Path.Combine(_directory.PhysicalPath, relativePath));
		string relative = Path.GetRelativePath(_directory.PhysicalPath, fullPath);
		if(Path.IsPathRooted(relative) || relative == ".." || relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)
			|| relative == ".")
			throw new ArgumentException("Path must be within this directory.", nameof(relativePath));
		return fullPath;
	}
}
