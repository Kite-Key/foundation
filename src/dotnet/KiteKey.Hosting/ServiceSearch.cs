using KiteKey.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace KiteKey.Hosting;

/// <summary>Search criteria for registering a service</summary>
/// <param name="InterfaceType">Interface type for which we will find and register all implementations</param>
/// <param name="Lifetime"><see cref="ServiceLifetime" /> to register</param>
/// <param name="RegisterInterface">
///     <c>true</c> if the service should be registered under <paramref name="InterfaceType" />, <c>false</c> if the service should be registered
///     under the actual implementing class type
/// </param>
/// <seealso cref="ServiceCollectionExtensions" />
public record ServiceSearch
(
	Type InterfaceType,
	ServiceLifetime Lifetime,
	bool RegisterInterface
);

/// <inheritdoc cref="ServiceSearch" />
public record ServiceSearch<T>
(
	ServiceLifetime Lifetime,
	bool RegisterInterface
) : ServiceSearch(typeof(T), Lifetime, RegisterInterface);
