using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using KiteKey.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace KiteKey.Hosting;

/// <summary>Extensions for <see cref="Assembly" /></summary>
public static class AssemblyExtensions
{
	/// <summary>Create an instance of every class that derives from <typeparamref name="TBase" /></summary>
	/// <typeparam name="TBase">Base class/interface</typeparam>
	/// <param name="assembly">Assembly to search</param>
	/// <param name="constructorParameters">Parameters to pass to the constructors</param>
	/// <returns>An instance of every class that derives from <typeparamref name="TBase" /></returns>
	public static IEnumerable<TBase> CreateImplementations<TBase>(this Assembly assembly, params object?[] constructorParameters)
	{
		foreach(Type type in assembly.GetImplementationTypes<TBase>())
			yield return TypeHelper.CreateDerivedInstance<TBase>(type, constructorParameters);
	}

	/// <summary>Create an instance of every class that derives from <typeparamref name="TBase" /></summary>
	/// <typeparam name="TBase">Base class/interface</typeparam>
	/// <param name="assembly">Assembly to search</param>
	/// <param name="serviceProvider">Service provider</param>
	/// <param name="constructorParameters">Additional objects that can be resolved by <paramref name="serviceProvider" /> for constructors</param>
	/// <returns>An instance of every class that derives from <typeparamref name="TBase" /></returns>
	[SuppressMessage("Syntax", "OG0001:Non-nullable variable should not be treated as nullable", Justification = "Incorrect API annotation")]
	public static IEnumerable<TBase> CreateImplementations<TBase>(this Assembly assembly, IServiceProvider serviceProvider, params object?[] constructorParameters)
	{
		foreach(Type type in assembly.GetImplementationTypes<TBase>())
			yield return (TBase)ActivatorUtilities.CreateInstance(serviceProvider, type, constructorParameters!);
	}

	/// <summary>Get all concrete (non-abstract) classes</summary>
	/// <param name="assembly">Assembly to search</param>
	/// <returns>Non-abstract class types</returns>
	public static IEnumerable<Type> GetConcreteClassTypes(this Assembly assembly)
	{
		foreach(Type type in assembly.GetTypes())
		{
			if(type.IsClass && !type.IsAbstract)
				yield return type;
		}
	}

	/// <summary>Get all concrete (non-abstract) implementations of <typeparamref name="TBase" /></summary>
	/// <typeparam name="TBase">Base class/interface</typeparam>
	/// <param name="assembly">Assembly to search</param>
	/// <returns>Non-abstract class types inheriting <typeparamref name="TBase" /></returns>
	public static IEnumerable<Type> GetImplementationTypes<TBase>(this Assembly assembly)
		=> assembly.GetImplementationTypes(typeof(TBase));

	/// <summary>Get all concrete (non-abstract) implementations of <paramref name="baseType" /></summary>
	/// <param name="assembly">Assembly to search</param>
	/// <param name="baseType">Base class/interface</param>
	/// <returns>Non-abstract class types inheriting <paramref name="baseType" /></returns>
	public static IEnumerable<Type> GetImplementationTypes(this Assembly assembly, Type baseType)
	{
		foreach(Type type in assembly.GetConcreteClassTypes())
		{
			if(type.IsAssignableTo(baseType))
				yield return type;
		}
	}
}
