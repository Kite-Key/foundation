namespace KiteKey.Core.Utility.Files;

/// <summary><see cref="IFileEntry" /> for a file that doesn't exist</summary>
public class NotFoundFileEntry : IFileEntry
{
	/// <inheritdoc />
	public bool Exists => false;

	/// <inheritdoc />
	public bool IsDirectory => false;

	/// <inheritdoc />
	public DateTimeOffset? LastModified => null;

	/// <inheritdoc />
	public long? Length => null;

	/// <inheritdoc />
	public string Name { get; }

	/// <inheritdoc />
	public string PhysicalPath { get; }

	/// <summary>Constructor</summary>
	/// <param name="name"><see cref="Name" /></param>
	/// <param name="physicalPath"><see cref="PhysicalPath" /></param>
	public NotFoundFileEntry(string name, string physicalPath)
	{
		Name = name;
		PhysicalPath = physicalPath;
	}

	/// <inheritdoc />
	public Stream CreateReadStream()
		=> throw new NotSupportedException();

	/// <inheritdoc />
	public void Delete()
		=> throw new NotSupportedException();
}
