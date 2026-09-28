namespace KiteKey.Core;

public static class LongExtensions
{
	private static readonly DateTime Epoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	/// <summary>
	/// Converts an epoch timestamp (in milliseconds) to a UTC DateTime.
	/// </summary>
	/// <param name="epochMilliseconds">The epoch timestamp in milliseconds.</param>
	/// <returns>A DateTime representing the UTC time.</returns>
	public static DateTime ToDateTime(this long epochMilliseconds)
	{
		return Epoch.AddMilliseconds(epochMilliseconds);
	}
}
