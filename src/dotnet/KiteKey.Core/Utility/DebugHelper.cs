using System.Diagnostics;

namespace KiteKey.Core.Utility;

/// <summary>Helper functions for debugging</summary>
public static class DebugHelper
{
	/// <summary>Wait for the debugger to be attached</summary>
	public static void WaitForDebugger(bool showOutput = true)
	{
		int counter = 1;

		while(!Debugger.IsAttached)
		{
			if(showOutput)
				Console.WriteLine($"Waiting for debugger to attach ({counter++})...");

			Thread.Sleep(100);
		}

		if(showOutput)
			Console.WriteLine("Debugger attached");
	}
}
