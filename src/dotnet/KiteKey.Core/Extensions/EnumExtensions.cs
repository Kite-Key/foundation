using System.ComponentModel;
using System.Reflection;

namespace KiteKey.Core;

/// <summary>Adds additional methods for enums.</summary>
public static class EnumExtensions
{
	/// <summary>Get the <see cref="DescriptionAttribute.Description" /> for the enum value.</summary>
	/// <param name="value">The enum value.</param>
	/// <returns><see cref="DescriptionAttribute.Description" />, or <c>null</c> if the attribute was not found</returns>
	public static string? GetDescription(this Enum value)
	{
		DescriptionAttribute? attribute = GetAttribute<DescriptionAttribute>(value);
		return attribute?.Description;
	}

	/// <summary>Using reflection, get the display name of the enum value.</summary>
	/// <param name="value">The enum value.</param>
	/// <returns>The <see cref="DisplayAttribute.Name" /> of the enum value.</returns>
	public static string GetEnumDisplayName(this Enum value)
	{
		DisplayAttribute? attribute = GetAttribute<DisplayAttribute>(value);
		return attribute?.Name ?? value.ToString();
	}

	/// <summary>Using reflection, determine if value is sortable..</summary>
	/// <param name="value">The enum value.</param>
	/// <returns>Whether the enum value can be sorted on.</returns>
	public static bool IsSortable(this Enum value)
	{
		SortableAttribute? attribute = GetAttribute<SortableAttribute>(value);
		return attribute is not null;
	}

	/// <summary>Using reflection, get the display name of the enum value.</summary>
	/// <param name="value">The enum value.</param>
	/// <param name="defaultValue">The default value if nothing is set.</param>
	/// <returns>The <see cref="DisplayAttribute.Order" /> of the enum value.</returns>
	public static int GetOrder(this Enum value, int defaultValue = 99)
	{
		DisplayAttribute? attribute = GetAttribute<DisplayAttribute>(value);
		return attribute?.Order ?? defaultValue;
	}

	/// <summary>Using reflection, translate from output of <see cref="GetEnumDisplayName(Enum)"/> to Enum</summary>
	/// <typeparam name="TEnum">The enum to parse.</typeparam>
	/// <param name="value">The enum value.</param>
	/// <returns>The parsed enum, if any.</returns>
	public static TEnum? FromEnumDisplayName<TEnum>(this string value)
		where TEnum : struct, Enum
	{
		Dictionary<string, TEnum> enumValuesByName = Enum.GetValues<TEnum>().ToDictionary(e => e.GetEnumDisplayName());

		if(enumValuesByName.TryGetValue(value, out TEnum response))
			return response;

		return null;
	}

	/// <summary>Using reflection, determine if this enum value should be displayed on any end-user user-interface.</summary>
	/// <param name="value">The enum value.</param>
	/// <returns>True if it can be displayed, false otherwise.</returns>
	public static bool IsHidden(this Enum value)
	{
		HiddenAttribute? attribute = GetAttribute<HiddenAttribute>(value);
		return attribute is not null;
	}

	/// <summary>Map from an enum value to another value via a <see cref="MapsToAttribute" /> on the source</summary>
	/// <typeparam name="T">Target type to map to</typeparam>
	/// <param name="value">Source <see cref="Enum" /> value</param>
	/// <returns>The value of type <typeparamref name="T" /> specified on the <see cref="MapsToAttribute" /> on the enum value</returns>
	public static T MapTo<T>(this Enum value)
	{
		if(TryMapTo(value, out T? mappedValue))
			return mappedValue;
		else
			throw new ArgumentException($"{value.GetEnumDisplayName()} Cannot be mapped to {typeof(T)}", nameof(value));
	}

	/// <summary>Extension method to return an enum value of type T for the given string.</summary>
	/// <typeparam name="T">The enum.</typeparam>
	/// <param name="value">.</param>
	/// <returns>The enum type.</returns>
	public static T ToEnum<T>(this string value)
		where T : struct, Enum
		=> TryToEnum<T>(value) ?? throw new ArgumentOutOfRangeException(nameof(value));

	/// <summary>Extension method to return an enum value of type T for the given string.</summary>
	/// <typeparam name="T">The enum.</typeparam>
	/// <param name="value">.</param>
	/// <returns>The enum type.</returns>
	public static T? TryToEnum<T>(this string value)
		where T : struct, Enum
	{
		if(Enum.TryParse<T>(value, true, out T result))
			return result;

		foreach(T enumVal in Enum.GetValues<T>())
		{
			if(value == enumVal.GetEnumDisplayName())
				return enumVal;
		}

		return null;
	}

	/// <summary>Extension method to return an enum values of type T for the given list of strings.</summary>
	/// <typeparam name="T">The enum.</typeparam>
	/// <param name="source">Source set of strings.</param>
	/// <returns>The enum type.</returns>
	/// <remarks>Fails silently on unmatching strings.</remarks>
	public static List<T> ToEnum<T>(this List<string> source)
		where T : struct, Enum
	{
		List<T> results = [];
		foreach(string value in source)
		{
			T? enumVal = TryToEnum<T>(value);

			if(enumVal.HasValue)
				results.Add(enumVal.Value);
		}

		return results;
	}

	/// <summary>Extension method to return an enum value of type T for the given int.</summary>
	/// <typeparam name="T">The enum.</typeparam>
	/// <param name="value">.</param>
	/// <returns>The enum type.</returns>
	public static T ToEnum<T>(this int value)
		where T : struct, Enum
	{
		T enumValue = (T)Enum.ToObject(typeof(T), value);
		if(!Enum.IsDefined(enumValue))
			throw new InvalidOperationException($"The enum does not have an element with {value} value");

		return enumValue;
	}

	/// <summary>Map from an enum value to another value via a <see cref="MapsToAttribute" /> on the source</summary>
	/// <typeparam name="T">Target type to map to</typeparam>
	/// <param name="value">Source <see cref="Enum" /> value</param>
	/// <param name="mappedValue">The value of type <typeparamref name="T" /> specified on the <see cref="MapsToAttribute" /> on the enum value</param>
	/// <returns><c>true</c> if the value was mapped, <c>false</c> if a matching <see cref="MapsToAttribute" /> could not be found</returns>
	public static bool TryMapTo<T>(Enum value, [NotNullWhen(true)] out T? mappedValue)
	{
		MapsToAttribute? attribute = GetAttributes<MapsToAttribute>(value)
			.SingleOrDefault(a => typeof(T).IsAssignableFrom(a.Type));

		if(attribute is null)
		{
			mappedValue = default;
			return false;
		}
		else
		{
			mappedValue = (T)attribute.Value;
			return true;
		}
	}

	private static TAttrib? GetAttribute<TAttrib>(Enum value)
		where TAttrib : Attribute
	{
		try
		{
			IEnumerable<TAttrib> attributes = GetAttributes<TAttrib>(value);
			return attributes.SingleOrDefault();
		}
		catch
		{
			// No attributes
		}

		return default;
	}

	private static IEnumerable<TAttrib> GetAttributes<TAttrib>(Enum value)
			where TAttrib : Attribute
	{
		Type type = value.GetType();
		string name = value.ToString();

		FieldInfo? field = type.GetField(name)
			?? throw new ArgumentException("The provided value was not found", nameof(value));

		IEnumerable<TAttrib> attributes = field.GetCustomAttributes<TAttrib>(false);
		return attributes;
	}
}
