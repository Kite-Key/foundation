using System.Runtime.Versioning;

namespace KiteKey.Core.Concurrency;

/// <summary>Throttles actions such that they are not performed more often than a given rate</summary>
public sealed class Throttler : IDisposable
{
	private readonly int _maxCount;
	private readonly SemaphoreSlim _semaphore;
	private readonly TimeSpan _updateFrequency;
	private readonly AsyncLock _updateLock = new();
	private DateTime _lastUpdate;

	/// <summary>Constructor</summary>
	/// <param name="count">Number of actions allowed within <paramref name="interval" /></param>
	/// <param name="interval">Time interval over which <paramref name="count" /> actions are allowed</param>
	public Throttler(int count, TimeSpan interval)
	{
		if(count <= 0)
			throw new ArgumentOutOfRangeException(nameof(count));
		if(interval <= TimeSpan.Zero || interval.Ticks < count)
			throw new ArgumentOutOfRangeException(nameof(interval));
		_semaphore = new(count, count);
		_maxCount = count;
		_lastUpdate = DateTime.Now;
		_updateFrequency = interval / count;
	}

	/// <summary>Wait until performing an action is allowed</summary>
	/// <param name="cancellationToken">Token that can be used to cancel the task safely</param>
	/// <returns>Async task completed when the action is allowed</returns>
	public async Task DelayAction(CancellationToken cancellationToken = default)
	{
		bool canAct;
		do
		{
			using(AsyncLock.Handle handle = await _updateLock.WaitAsync(cancellationToken))
				UpdateCount();

			TimeSpan timeSinceUpdate = DateTime.Now - _lastUpdate;
			TimeSpan waitTime = _updateFrequency - timeSinceUpdate;
			if(waitTime < TimeSpan.Zero)
				waitTime = TimeSpan.Zero;

			canAct = await _semaphore.WaitAsync(waitTime, cancellationToken);
		}
		while(!canAct);
	}

	/// <inheritdoc />
	public void Dispose()
	{
		_semaphore.Dispose();
		_updateLock.Dispose();
	}

	/// <summary>Test if performing an action is allowed</summary>
	/// <returns>True if the action can be performed, in which case this action is also counted against the action count</returns>
	[UnsupportedOSPlatform("browser")]
	public bool TryAction()
	{
		using(AsyncLock.Handle handle = _updateLock.WaitSync())
			UpdateCount();

		return _semaphore.Wait(0);
	}

	private void UpdateCount()
	{
		DateTime now = DateTime.Now;
		TimeSpan diff = now - _lastUpdate;

		int count = (int)(diff / _updateFrequency);
		if(count <= 0)
			return;

		_lastUpdate += _updateFrequency * count;

		int maxAddCount = _maxCount - _semaphore.CurrentCount;
		int addCount = Math.Min(count, maxAddCount);

		if(addCount > 0)
			_semaphore.Release(addCount);
	}
}
