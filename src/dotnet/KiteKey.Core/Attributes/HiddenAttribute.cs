namespace KiteKey.Core.Attributes;

/// <summary>Indicates this should be hidden from any UI rendered on the client.</summary>
[AttributeUsage(AttributeTargets.Field, Inherited = false)]
public class HiddenAttribute : Attribute;
