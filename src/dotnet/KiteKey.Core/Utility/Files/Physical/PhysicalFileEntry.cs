namespace KiteKey.Core.Utility.Files.Physical;

/// <summary><see cref="IFileEntry" /> for a physical directory</summary>
public class PhysicalFileEntry : IFileEntry
{
	/// <inheritdoc />
	public bool Exists => Info.Exists;

	/// <summary><see cref="FileInfo" /></summary>
	public FileInfo Info { get; }

	/// <inheritdoc />
	public bool IsDirectory => false;

	/// <inheritdoc />
	public DateTimeOffset? LastModified => Info.LastWriteTime;

	/// <inheritdoc />
	public long? Length => Info.Length;

	/// <inheritdoc />
	public string Name => Info.Name;

	/// <inheritdoc />
	public string PhysicalPath => Info.FullName;

	/// <summary>Constructor</summary>
	/// <param name="path">File path</param>
	public PhysicalFileEntry(string path)
		: this(new FileInfo(path)) { }

	/// <summary>Constructor</summary>
	/// <param name="info">File info</param>
	public PhysicalFileEntry(FileInfo info)
		=> Info = info;

	/// <inheritdoc />
	public Stream CreateReadStream()
		=> Info.OpenRead();

	/// <inheritdoc />
	public void Delete()
		=> Info.Delete();
}
