namespace KiteKey.Core;

/// <summary>Extensions for <see cref="SemaphoreSlim" /></summary>
public static class SemaphoreSlimExtensions
{
	/// <summary>Try to release (increment) the semaphore</summary>
	/// <param name="semaphore">Semaphore to release</param>
	/// <param name="count">Number of times to release the semaphore</param>
	/// <returns><c>true</c> if the semaphore was released, or <c>false</c> if the semaphore was already too full</returns>
	public static bool TryRelease(this SemaphoreSlim semaphore, int count = 1)
	{
		try
		{
			semaphore.Release(count);
			return true;
		}
		catch(SemaphoreFullException)
		{
			return false;
		}
	}
}
