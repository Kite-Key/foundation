using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace KiteKey.Hosting;

public static class ServiceCollectionExtensions
{
    public static OptionsBuilder<T> AddOptionsValidated<T>(this IServiceCollection services, string section)
        where T : class
        => services.AddOptions<T>().BindConfiguration(section).ValidateDataAnnotations().ValidateOnStart();

    public static IServiceCollection AddScopeValue<T>(this IServiceCollection services) where T : class
    {
        services.AddScoped<ScopeValue<T>>();
        services.AddScoped(provider => provider.GetRequiredService<ScopeValue<T>>().Value
            ?? throw new InvalidOperationException($"Set {typeof(T).Name} in this scope before resolving it."));
        return services;
    }

    public static void SetScopeValue<T>(this IServiceScope scope, T value) where T : class
        => scope.ServiceProvider.GetRequiredService<ScopeValue<T>>().Value = value;
}
