namespace KiteKey.Core.Models.Base;

/// <summary>Base class for models that represent an encapsulated value</summary>
/// <typeparam name="TModel">Actual derived type</typeparam>
/// <typeparam name="TValue">Encapsulated value type</typeparam>
public abstract class SimpleValueBase<TModel, TValue> : IComparable<TModel>, IEquatable<TModel>, IComparable
	where TModel : SimpleValueBase<TModel, TValue>
	where TValue : notnull, IComparable<TValue>, IEquatable<TValue>
{
	/// <summary>Encapsulated value</summary>
	public TValue Value { get; init; } = default!; // Cannot use "required" due to generic constraints

	/// <inheritdoc />
	public int CompareTo(TModel? other)
	{
		if(other is null)
			return 1;
		else
			return Value.CompareTo(other.Value);
	}

	/// <inheritdoc />
	int IComparable.CompareTo(object? obj)
	{
		return obj switch
		{
			null => CompareTo(null),
			TModel x => CompareTo(x),
			_ => throw new ArgumentOutOfRangeException(nameof(obj)),
		};
	}

	/// <inheritdoc />
	public bool Equals(TModel? other)
	{
		if(other is null)
			return false;
		else
			return Value.Equals(other.Value);
	}

	/// <inheritdoc />
	public override bool Equals(object? obj)
		=> Equals(obj as TModel);

	/// <inheritdoc />
	public override int GetHashCode()
		=> Value.GetHashCode();

	/// <inheritdoc />
	public override string ToString()
		=> Value.ToString() ?? "";

	public static bool operator !=(SimpleValueBase<TModel, TValue> left, TModel right)
		=> !(left == right);

	public static bool operator <(SimpleValueBase<TModel, TValue> left, TModel right)
		=> left.CompareTo(right) < 0;

	public static bool operator <=(SimpleValueBase<TModel, TValue> left, TModel right)
		=> left.CompareTo(right) <= 0;

	public static bool operator ==(SimpleValueBase<TModel, TValue> left, TModel right)
		=> left.Equals(right);

	public static bool operator >(SimpleValueBase<TModel, TValue> left, TModel right)
		=> left.CompareTo(right) > 0;

	public static bool operator >=(SimpleValueBase<TModel, TValue> left, TModel right)
		=> left.CompareTo(right) >= 0;
}
