namespace KiteKey.Core;

/// <summary>Extensions for <see cref="Delegate" /> and generic delegates like <see cref="Func{T}" /> and <see cref="Action" /></summary>
[SuppressMessage("Roslynator", "RCS1047:Non-asynchronous method name should not end with 'Async'.", Justification = "AsAsync is the whole phrase, not an Async suffix")]
public static class DelegateExtensions
{
	/// <summary>Convert a synchronous <see cref="Action" /> to an asynchronous <see cref="Func{T}" /></summary>
	/// <param name="action">Action to return</param>
	/// <returns>Wrapper around <paramref name="action" /> that returns a <see cref="Task" /></returns>
	/// <remarks>Note: the returned function still executes entirely synchronously, it just also returns a <see cref="Task" /> now</remarks>
	public static Func<Task> AsAsync(this Action action)
	{
		return () =>
		{
			action();
			return Task.CompletedTask;
		};
	}

	/// <summary>Convert a synchronous <see cref="Action{T}" /> to an asynchronous <see cref="Func{T, T}" /></summary>
	/// <param name="action">Action to return</param>
	/// <typeparam name="T">Argument type</typeparam>
	/// <returns>Wrapper around <paramref name="action" /> that returns a <see cref="Task" /></returns>
	/// <remarks>Note: the returned function still executes entirely synchronously, it just also returns a <see cref="Task" /> now</remarks>
	public static Func<T, Task> AsAsync<T>(this Action<T> action)
	{
		return data =>
		{
			action(data);
			return Task.CompletedTask;
		};
	}

	/// <summary>Convert a synchronous <see cref="Action{T1, T2}" /> to an asynchronous <see cref="Func{T1, T2, T}" /></summary>
	/// <param name="action">Action to return</param>
	/// <typeparam name="T1">Argument 1 type</typeparam>
	/// <typeparam name="T2">Argument 2 type</typeparam>
	/// <returns>Wrapper around <paramref name="action" /> that returns a <see cref="Task" /></returns>
	/// <remarks>Note: the returned function still executes entirely synchronously, it just also returns a <see cref="Task" /> now</remarks>
	public static Func<T1, T2, Task> AsAsync<T1, T2>(this Action<T1, T2> action)
	{
		return (data1, data2) =>
		{
			action(data1, data2);
			return Task.CompletedTask;
		};
	}

	/// <summary>Invoke an async event handler delegate</summary>
	/// <param name="eventHandler">Event handler delegate</param>
	/// <returns>Async task</returns>
	public static async Task InvokeAsync(this Func<Task>? eventHandler)
		=> await eventHandler.InvokeAsync(d => d.Invoke());

	/// <summary>Invoke an async event handler delegate</summary>
	/// <typeparam name="TArg">Argument type</typeparam>
	/// <param name="eventHandler">Event handler delegate</param>
	/// <param name="value">Argument value</param>
	/// <returns>Async task</returns>
	public static async Task InvokeAsync<TArg>(this Func<TArg, Task>? eventHandler, TArg value)
		=> await eventHandler.InvokeAsync(d => d.Invoke(value));

	/// <summary>Invoke an async event handler delegate</summary>
	/// <typeparam name="TDelegate">Async event handler delegate (should return <see cref="Task" />)</typeparam>
	/// <param name="eventHandler">Event handler delegate</param>
	/// <param name="invoke">Callback to invoke a single <typeparamref name="TDelegate" /></param>
	/// <returns>Async task</returns>
	public static async Task InvokeAsync<TDelegate>(this TDelegate? eventHandler, Func<TDelegate, Task> invoke)
		where TDelegate : Delegate
	{
		if(eventHandler is null)
			return;

		IEnumerable<Task> tasks = eventHandler
			.GetInvocationList()
			.Cast<TDelegate>()
			.Select(invoke);

		await Task.WhenAll(tasks);
	}
}
