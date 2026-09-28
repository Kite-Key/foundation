using KiteKey.Hosting;

namespace KiteKey.Hosting;

/// <summary>Configuration settings that can be registered using <see cref="ServiceCollectionExtensions.AddOptionsValidated{TSettings}(Microsoft.Extensions.DependencyInjection.IServiceCollection, string)"/></summary>
public interface IConfigurationSettings
{
    /// <summary>Configuration section</summary>
    /// <example><![CDATA[App:Some:Configuration:Settings]]></example>
    static abstract string Section { get; }
}
