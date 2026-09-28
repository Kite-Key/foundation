using System.Reflection;
using KiteKey.Hosting;

namespace KiteKey.Hosting;

/// <summary>Helper functions for <see cref="Type" /></summary>
public static class TypeHelper
{
	/// <summary><inheritdoc cref="Activator.CreateInstance(Type, object[])" /></summary>
	/// <typeparam name="TBase">Type of value to return</typeparam>
	/// <param name="actualType">Actual instance type to create (must be assignable to <typeparamref name="TBase" />)</param>
	/// <param name="constructorParameters">Parameters to pass to the constructors</param>
	/// <returns>New instance of <paramref name="actualType" /></returns>
	public static TBase CreateDerivedInstance<TBase>(Type actualType, params object?[] constructorParameters)
	{
		object? instance = Activator.CreateInstance(actualType, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, constructorParameters, null);
		if(instance is null)
			throw new TargetInvocationException($"Failed to create {actualType}", null);
		else
			return (TBase)instance;
	}

	/// <summary>Create an instance of every class that derives from <typeparamref name="TBase" /></summary>
	/// <typeparam name="TBase">Base class/interface</typeparam>
	/// <param name="constructorParameters">Parameters to pass to the constructors</param>
	/// <returns>An instance of every class that derives from <typeparamref name="TBase" /> in the same assembly</returns>
	public static IEnumerable<TBase> CreateImplementations<TBase>(params object?[] constructorParameters)
		=> typeof(TBase).Assembly.CreateImplementations<TBase>(constructorParameters);

	/// <summary>Create an instance of every class that derives from <typeparamref name="TBase" /></summary>
	/// <typeparam name="TBase">Base class/interface</typeparam>
	/// <param name="serviceProvider">Service provider</param>
	/// <param name="constructorParameters">Additional objects that can be resolved by <paramref name="serviceProvider" /> for constructors</param>
	/// <returns>An instance of every class that derives from <typeparamref name="TBase" /> in the same assembly</returns>
	public static IEnumerable<TBase> CreateImplementations<TBase>(IServiceProvider serviceProvider, params object?[] constructorParameters)
		=> typeof(TBase).Assembly.CreateImplementations<TBase>(serviceProvider, constructorParameters);

	/// <summary><inheritdoc cref="Activator.CreateInstance(Type, object[])" /></summary>
	/// <typeparam name="T">Type of object to create</typeparam>
	/// <param name="constructorParameters">Parameters to pass to the constructors</param>
	/// <returns>New instance of <typeparamref name="T" /></returns>
	public static T CreateInstance<T>(params object?[] constructorParameters)
		=> CreateDerivedInstance<T>(typeof(T), constructorParameters);

	/// <summary><inheritdoc cref="Activator.CreateInstance(Type, object[])" /></summary>
	/// <param name="actualType">Actual instance type to create</param>
	/// <param name="constructorParameters">Parameters to pass to the constructors</param>
	/// <returns>New instance of <paramref name="actualType" /></returns>
	public static object CreateInstance(Type actualType, params object?[] constructorParameters)
		=> CreateDerivedInstance<object>(actualType, constructorParameters);

	/// <summary>Get all concrete (non-abstract) implementations of <typeparamref name="TBase" /></summary>
	/// <typeparam name="TBase">Base class/interface</typeparam>
	/// <returns>Non-abstract class types inheriting <typeparamref name="TBase" /> in the same assembly</returns>
	public static IEnumerable<Type> GetImplementationTypes<TBase>()
		=> typeof(TBase).Assembly.GetImplementationTypes<TBase>();
}
