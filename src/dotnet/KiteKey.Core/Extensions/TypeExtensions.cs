using System.Reflection;
using System.Text.RegularExpressions;

namespace KiteKey.Core;

/// <summary>Extensions for <see cref="Type" /></summary>
public static class TypeExtensions
{
	private static readonly Regex _regex = new("(?<=\\p{Ll})(?=\\p{Lu})");

	/// <summary>Check if the class implements an interface by name</summary>
	/// <param name="classType">Class type</param>
	/// <param name="interfaceType">Interface to find (may be a generic template)</param>
	/// <returns>Whether the class has an interface of the same name as <paramref name="interfaceType" /></returns>
	/// <remarks><see cref="Type.IsAssignableTo(Type?)" /> when the exact type is known</remarks>
	public static bool HasInterface(this Type classType, Type interfaceType)
	{
		Type? interfaceRef = classType.GetInterface(interfaceType.Name, false);
		return interfaceRef is not null;
	}

	/// <summary>Gets the human readable name for this class via splitting on PascalCasing or looking for the DisplayAttribute.</summary>
	/// <param name="type">The Type object</param>
	/// <returns>Human readable name.</returns>
	public static string GetHumanReadableName(this Type type)
	{
		// Check if the class has a Display attribute

		if(type.GetCustomAttributes(typeof(DisplayAttribute), true).FirstOrDefault() is DisplayAttribute displayAttribute)
			return displayAttribute.Name!;

		return _regex.Replace(type.Name, " ");
	}

	/// <inheritdoc cref="HasInterface(Type, Type)" />
	public static bool HasInterface<T>(this Type classType)
		=> classType.HasInterface(typeof(T));

	/// <summary>Load a resource that is adjacent (in project structure) to the specified type</summary>
	/// <param name="adjacentType">Any type adjacent (same namespace and assembly) to the embedded resource file</param>
	/// <param name="resourceName">Resource file name (including extension, without path)</param>
	/// <returns>Embedded resource string data</returns>
	public static async Task<string> LoadResourceString(this Type adjacentType, string resourceName)
	{
		await using Stream stream = adjacentType.OpenResourceStream(resourceName);
		using StreamReader reader = new(stream);
		return await reader.ReadToEndAsync();
	}

	/// <summary>Load a resource that is adjacent (in project structure) to the specified type</summary>
	/// <param name="adjacentType">Any type adjacent (same namespace and assembly) to the embedded resource file</param>
	/// <param name="resourceName">Resource file name (including extension, without path)</param>
	/// <returns>Embedded resource byte data</returns>
	public static async Task<byte[]> LoadResourceBytes(this Type adjacentType, string resourceName)
	{
		await using Stream stream = adjacentType.OpenResourceStream(resourceName);
		await using MemoryStream memory = new();
		await stream.CopyToAsync(memory);
		return memory.ToArray();
	}

	/// <summary>Get a <see cref="Stream" /> to read a embedded data</summary>
	/// <param name="adjacentType">Any type adjacent (same namespace and assembly) to the embedded resource file</param>
	/// <param name="resourceName">Resource file name (including extension, without path)</param>
	/// <returns><see cref="Stream" /> that reads the embedded resource data</returns>
	public static Stream OpenResourceStream(this Type adjacentType, string resourceName)
	{
		string resourcePath = $"{adjacentType.Namespace}.{resourceName}";

		Stream? stream = adjacentType.Assembly.GetManifestResourceStream(resourcePath);

		if(stream is null)
			throw new ArgumentException("Resource not found", nameof(resourceName));
		else
			return stream;
	}

	/// <summary>Read the value of a private field</summary>
	/// <typeparam name="T">Field value type</typeparam>
	/// <param name="type">Source type</param>
	/// <param name="source">Source object</param>
	/// <param name="fieldName">Field name</param>
	/// <returns>Field value</returns>
	public static T? GetPrivateField<T>(this Type type, object source, string fieldName)
		where T : notnull
	{
		FieldInfo field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
			?? throw new KeyNotFoundException($"Failed to read field {fieldName}");

		object? value = field.GetValue(source);
		return (T?)value;
	}

	/// <inheritdoc cref="GetPrivateField"/>
	public static T GetPrivateRequiredField<T>(this Type type, object source, string fieldName)
		where T : notnull
	{
		return type.GetPrivateField<T>(source, fieldName)
			?? throw new InvalidDataException($"Unexpected null value in {fieldName}");
	}
}
