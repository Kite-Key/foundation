using Microsoft.Extensions.DependencyInjection;

namespace KiteKey.Hosting;

/// <summary>Extensions for <see cref="IServiceScope"/></summary>
public static class ServiceScopeExtensions
{
	/// <inheritdoc cref="IServiceProvider.GetService"/>
	public static T? GetService<T>(this IServiceScope scope)
		=> scope.ServiceProvider.GetService<T>();

	/// <inheritdoc cref="ServiceProviderServiceExtensions.GetRequiredService"/>
	public static T GetRequiredService<T>(this IServiceScope scope)
		where T : notnull
		=> scope.ServiceProvider.GetRequiredService<T>();
}
