using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace KiteKey.Hosting;

public static class ServiceCollectionExtensions
{
    public static OptionsBuilder<T> AddOptionsValidated<T>(this IServiceCollection services, string section)
        where T : class
        => services.AddOptions<T>().BindConfiguration(section).ValidateDataAnnotations().ValidateOnStart();

    public static OptionsBuilder<T> AddOptionsValidated<T>(this IServiceCollection services)
        where T : class, IConfigurationSettings
        => services.AddOptionsValidated<T>(T.Section);

    public static IServiceCollection AddScopeValue<T>(this IServiceCollection services) where T : class
    {
        services.AddScoped<ScopeValue<T>>();
        services.AddScoped(provider => provider.GetRequiredService<ScopeValue<T>>().Value
            ?? throw new InvalidOperationException($"Set {typeof(T).Name} in this scope before resolving it."));
        return services;
    }

    public static void SetScopeValue<T>(this IServiceScope scope, T value) where T : class
        => scope.ServiceProvider.GetRequiredService<ScopeValue<T>>().Value = value;

    public static IServiceCollection FindServices(this IServiceCollection services, params ServiceSearch[] searches)
    {
        ArgumentNullException.ThrowIfNull(searches);
        if (searches.Length == 0)
            throw new ArgumentException("At least one search is required.", nameof(searches));
        return services.FindServices(searches[0].InterfaceType.Assembly, searches);
    }

    public static IServiceCollection FindServices(this IServiceCollection services, Assembly assembly, params ServiceSearch[] searches)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        foreach (Type implementation in assembly.GetConcreteClassTypes())
        {
            foreach (ServiceSearch search in searches)
            {
                if (services.TryAddService(implementation, search.InterfaceType, search.Lifetime, search.RegisterInterface))
                    break;
            }
        }
        return services;
    }

    public static bool TryAddService(this IServiceCollection services, Type implementationType, Type interfaceType, ServiceLifetime lifetime, bool registerInterface = true)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(implementationType);
        ArgumentNullException.ThrowIfNull(interfaceType);
        Type? resolvedInterface = interfaceType.IsGenericTypeDefinition
            ? implementationType.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType)
            : interfaceType;
        if (resolvedInterface is null || !resolvedInterface.IsAssignableFrom(implementationType) || implementationType.IsAbstract)
            return false;
        services.Add(new ServiceDescriptor(registerInterface ? resolvedInterface : implementationType, implementationType, lifetime));
        return true;
    }
}
