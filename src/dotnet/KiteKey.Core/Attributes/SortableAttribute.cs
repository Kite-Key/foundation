namespace KiteKey.Core.Attributes;

/// <summary>Attribute that indicates this property or enum value can be used for sorting.</summary>
[AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public class SortableAttribute : Attribute;
