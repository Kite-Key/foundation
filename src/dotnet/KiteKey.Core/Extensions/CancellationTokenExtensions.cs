using System.Runtime.CompilerServices;

namespace KiteKey.Core;

/// <summary>Extensions for <see cref="CancellationToken" /></summary>
public static class CancellationTokenExtensions
{
	/// <summary>Allows a cancellation token to be awaited</summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns><see cref="CancellationTokenAwaiter" /></returns>
	public static CancellationTokenAwaiter GetAwaiter(this CancellationToken cancellationToken)
		=> new(cancellationToken);

	/// <summary>Awaiter for <see cref="_cancellationToken" /></summary>
	public readonly struct CancellationTokenAwaiter : INotifyCompletion, ICriticalNotifyCompletion
	{
		private readonly CancellationToken _cancellationToken;

		/// <summary>Called by the compiler to determine if the <c>await</c> is done</summary>
		public bool IsCompleted => _cancellationToken.IsCancellationRequested;

		/// <summary>Constructor</summary>
		public CancellationTokenAwaiter(CancellationToken cancellationToken)
			=> _cancellationToken = cancellationToken;

		/// <summary>Called by the compiler to get the result of the <c>await</c> expression</summary>
		/// <returns>The <see cref="CancellationToken" /></returns>
		public CancellationToken GetResult()
			=> _cancellationToken;

		/// <inheritdoc />
		public void OnCompleted(Action continuation)
			=> _cancellationToken.Register(continuation);

		/// <inheritdoc />
		public void UnsafeOnCompleted(Action continuation)
			=> _cancellationToken.Register(continuation);
	}
}
