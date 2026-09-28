namespace KiteKey.Core;

/// <summary>Extensions for <see cref="Exception" /></summary>
public static class ExceptionExtensions
{
	/// <summary>Check if the exception represents a cancellation</summary>
	/// <param name="ex">Exception to check</param>
	/// <returns>True if the exception is about a task or thread being canceled</returns>
	public static bool IsCancellation(this Exception ex)
	{
		return ex is OperationCanceledException || // From CancellationToken
			ex is ThreadAbortException || // From Thread.Abort
			ex is ThreadInterruptedException || // From Thread.Interrupt
			(ex.InnerException?.IsCancellation() ?? false) || // Wrapped exception
			(ex is AggregateException ag && // From Task
				ag.Flatten().InnerExceptions.All(innerEx => innerEx.IsCancellation()));
	}
}
