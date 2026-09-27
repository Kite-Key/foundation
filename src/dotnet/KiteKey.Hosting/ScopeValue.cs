namespace KiteKey.Hosting;

/// <summary>A value provided by the creator of a dependency injection scope.</summary>
public sealed class ScopeValue<T> where T : class
{
    public T? Value { get; set; }
}
