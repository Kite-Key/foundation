namespace KiteKey.Core.Models.Base;

/// <summary>Base class for models that represent an encapsulated string value</summary>
/// <typeparam name="TModel">Actual derived type</typeparam>
public abstract class StringValueBase<TModel> : IComparable<TModel>, IEquatable<TModel>, IComparable
	where TModel : StringValueBase<TModel>
{
	/// <summary>Get the encapsulated value</summary>
	/// <returns>String value</returns>
	protected abstract string StringValue { get; }

	/// <inheritdoc />
	public int CompareTo(TModel? other)
	{
		if(other is null)
			return 1;
		else
			return StringValue.CompareTo(other.StringValue);
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
		=> StringValue == other?.StringValue;

	/// <inheritdoc />
	public override bool Equals(object? obj)
		=> Equals(obj as TModel);

	/// <inheritdoc />
	public override int GetHashCode()
		=> StringValue.GetHashCode();

	/// <inheritdoc />
	public override string ToString()
		=> StringValue;

	public static bool operator !=(StringValueBase<TModel> left, TModel right)
		=> !(left == right);

	public static bool operator <(StringValueBase<TModel> left, TModel right)
		=> left.CompareTo(right) < 0;

	public static bool operator <=(StringValueBase<TModel> left, TModel right)
		=> left.CompareTo(right) <= 0;

	public static bool operator ==(StringValueBase<TModel> left, TModel right)
		=> left.Equals(right);

	public static bool operator >(StringValueBase<TModel> left, TModel right)
		=> left.CompareTo(right) > 0;

	public static bool operator >=(StringValueBase<TModel> left, TModel right)
		=> left.CompareTo(right) >= 0;
}
