namespace KiteKey.Core.Attributes;

/// <summary>Attribute that indicates this property is whitelisted to be recieved from a remote client</summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public class TransferableAttribute : Attribute;
