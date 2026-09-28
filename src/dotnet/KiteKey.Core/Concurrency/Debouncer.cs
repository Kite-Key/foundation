using System.Collections.Concurrent;
using System.Runtime.Versioning;

namespace KiteKey.Core.Concurrency;

/// <summary>Runs a background thread that queues rapid actions to be processed in a batch after a delay</summary>
/// <typeparam name="TData">Type of data to queue for each action</typeparam>
[UnsupportedOSPlatform("browser")]
public sealed class Debouncer<TData> : IDisposable
{
	private readonly CancellationTokenSource _cancel = new();
	private readonly TimeSpan _delay;
	private readonly Func<IReadOnlyList<TData>, Task> _processActions;
	private readonly ConcurrentQueue<TData> _queue = new();
	private readonly SemaphoreSlim _semaphore = new(0, 1);
	private readonly Task _thread;

	/// <summary>Constructor</summary>
	/// <param name="processActions">Function that processes queued actions</param>
	/// <param name="delay">Delay to wait after actions are added before they are processed</param>
	public Debouncer(Func<IReadOnlyList<TData>, Task> processActions, TimeSpan delay)
	{
		ArgumentNullException.ThrowIfNull(processActions);
		if(delay < TimeSpan.Zero)
			throw new ArgumentOutOfRangeException(nameof(delay));
		_processActions = processActions;
		_delay = delay;
		_thread = Task.Run(WorkerThread, _cancel.Token);
	}

	/// <inheritdoc />
	public void Dispose()
	{
		_cancel.Cancel();
		_thread.Wait();
		_cancel.Dispose();
		_semaphore.Dispose();
	}

	/// <summary>Queue an action to perform from the background thread after a delay</summary>
	/// <param name="data">Data for the action</param>
	/// <param name="trigger">If not set, the item will be queued up but the handler will not be invoked until triggered later</param>
	public void Enqueue(TData data, bool trigger = true)
	{
		_queue.Enqueue(data);

		if(trigger)
			_semaphore.TryRelease();
	}

	private async Task WorkerThread()
	{
		List<TData> items = new();

		try
		{
			while(!_cancel.IsCancellationRequested)
			{
				// Wait for items
				await _semaphore.WaitAsync(_cancel.Token);

				// Copy items locally
				while(_queue.TryDequeue(out TData? data))
					items.Add(data);

				// Make sure we got items
				if(items.Count == 0)
					continue;

				try
				{
					// Wait for delay
					await Task.Delay(_delay, _cancel.Token);

					// Restart if more items were added during delay
					if(!_queue.IsEmpty)
						continue;
				}
				catch(TaskCanceledException) { }

				// Process items, even if we're cancelling
				await _processActions(items);
				items = new List<TData>(items.Count); // Don't re-use list in case they kept a reference to the old list
			}
		}
		catch(OperationCanceledException) when(_cancel.IsCancellationRequested) { }
	}
}
