namespace KiteKey.Core.Attributes;

/// <summary>Attribute that indicates this property is whitelisted to be indexed for searching.</summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public class SearchableAttribute : Attribute
{
	/// <summary>Default constructor.</summary>
	public SearchableAttribute() { }

	/// <summary>Default constructor.</summary>
	public SearchableAttribute(bool isUnique) => IsUniqueId = isUnique;

	/// <summary>Whether or not the supplied attribute is the unique id for the searchable record.</summary>
	public bool IsUniqueId { get; set; }
}
