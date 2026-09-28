namespace KiteKey.Core.Attributes;

/// <summary>Indicates that this value can be mapped to another value</summary>
/// <remarks>
///     This currently intended to be applied on <see cref="Enum" /> members to map to other <see cref="Enum" /> values, but more targets can be added
///     if we add mapping logic for them.
/// </remarks>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
public class MapsToAttribute : Attribute
{
	/// <summary>Target type</summary>
	public Type Type { get; }

	/// <summary>Target value</summary>
	public object Value { get; }

	/// <summary>Constructor</summary>
	/// <param name="value">Target value</param>
	public MapsToAttribute(object value)
	{
		Value = value;
		Type = value.GetType();
	}
}
