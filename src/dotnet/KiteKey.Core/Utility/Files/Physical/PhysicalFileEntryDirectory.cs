namespace KiteKey.Core.Utility.Files.Physical;

/// <summary><see cref="IFileEntry" /> for a physical directory</summary>
public class PhysicalFileEntryDirectory : IFileEntry
{
	/// <inheritdoc />
	public bool Exists => Info.Exists;

	/// <summary><see cref="DirectoryInfo" /></summary>
	public DirectoryInfo Info { get; }

	/// <inheritdoc />
	public bool IsDirectory => true;

	/// <inheritdoc />
	public DateTimeOffset? LastModified => Info.LastWriteTime;

	/// <inheritdoc />
	public long? Length => null;

	/// <inheritdoc />
	public string Name => Info.Name;

	/// <inheritdoc />
	public string PhysicalPath => Info.FullName;

	/// <summary>Constructor</summary>
	/// <param name="path">Directory path</param>
	public PhysicalFileEntryDirectory(string path)
		: this(new DirectoryInfo(path)) { }

	/// <summary>Constructor</summary>
	/// <param name="info">Directory info</param>
	public PhysicalFileEntryDirectory(DirectoryInfo info)
		=> Info = info;

	/// <inheritdoc />
	public Stream CreateReadStream()
		=> throw new NotSupportedException();

	/// <inheritdoc />
	public void Delete()
		=> Info.Delete();
}
