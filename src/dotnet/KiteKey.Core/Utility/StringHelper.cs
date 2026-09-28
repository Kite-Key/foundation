using System.Globalization;

namespace KiteKey.Core.Utility;

/// <summary>
/// Various utility functions relating to <see cref="string"/>.
/// </summary>
public static class StringHelper
{
	/// <summary>
	/// Generate a string of random characters.
	/// </summary>
	/// <param name="minimumLength">The minimum length of the generated string</param>
	/// <param name="maximumLength">The maximum length of the generated string</param>
	/// <returns>A randomized string using alphabet characters.</returns>
	/// <remarks>This is commonly used by services that wish to obfuscate responses for various reasons.</remarks>
	public static string GetRandomString(int minimumLength = 5, int maximumLength = 13)
	{
		const string chars = "abcdefghijklmnopqrstuvwxyz  ";
		Random random = Random.Shared;

		int length = random.Next(minimumLength, maximumLength);
		char[] stringChars = new char[length];

		for(int i = 0; i < stringChars.Length; i++)
		{
			int charIndex = random.Next(chars.Length);
			stringChars[i] = chars[charIndex];
		}

		string result = new(stringChars);
		return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(result);
	}
}
