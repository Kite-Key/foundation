using System.Runtime.Versioning;

namespace KiteKey.Core.Concurrency;

/// <summary>Lock that uses allows waiting using an async <see cref="Task" /></summary>
public sealed class AsyncLock : IDisposable
{
	private readonly SemaphoreSlim _semaphore = new(1, 1);

	/// <inheritdoc />
	public void Dispose()
		=> _semaphore.Dispose();

	/// <summary>Wait for the lock</summary>
	/// <param name="cancellationToken">Token that can be canceled to abort waiting</param>
	/// <returns>Handle that will release the lock when used with a <c>using</c> block</returns>
	public async Task<Handle> WaitAsync(CancellationToken cancellationToken = default)
	{
		await _semaphore.WaitAsync(cancellationToken);
		return new Handle(_semaphore);
	}

	/// <summary>Wait for the lock</summary>
	/// <returns>Handle that will release the lock when used with a <c>using</c> block</returns>
	[UnsupportedOSPlatform("browser")]
	public Handle WaitSync()
	{
		_semaphore.Wait();
		return new Handle(_semaphore);
	}

	/// <summary>Handle to release a lock when disposed</summary>
	public sealed class Handle : IDisposable
	{
		private readonly SemaphoreSlim _semaphore;
		private bool _disposed;

		/// <summary>Constructor</summary>
		public Handle(SemaphoreSlim semaphore)
		{
			_semaphore = semaphore;
			_disposed = false;
		}

		/// <inheritdoc />
		public void Dispose()
		{
			if(_disposed)
				return;

			try
			{
				_semaphore.Release();
			}
			catch(ObjectDisposedException) { } // Avoid overwriting exceptions in the caller's finally block

			_disposed = true;
		}
	}
}
