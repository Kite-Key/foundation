using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace KiteKey.Hosting;

public static class ConfigurationExtensions
{
    public static string GetRequiredValue(this IConfiguration configuration, string key)
        => !string.IsNullOrEmpty(configuration[key])
            ? configuration[key]!
            : throw new InvalidOperationException($"\"{key}\" must be configured");

    public static string GetValueWithDevDefault(this IConfiguration configuration, string key, bool isDevelopment, Func<string> getDevDefault)
        => !string.IsNullOrEmpty(configuration[key])
            ? configuration[key]!
            : isDevelopment ? getDevDefault() : throw new InvalidOperationException($"\"{key}\" must be configured");

    public static string GetValueWithDevDefault(this IConfiguration configuration, string key, IHostEnvironment environment, Func<string> getDevDefault)
        => configuration.GetValueWithDevDefault(key, environment.IsDevelopment(), getDevDefault);

    public static string GetValueWithDevDefault(this IConfiguration configuration, string key, bool isDevelopment, string devDefault)
        => configuration.GetValueWithDevDefault(key, isDevelopment, () => devDefault);

    public static string GetValueWithDevDefault(this IConfiguration configuration, string key, IHostEnvironment environment, string devDefault)
        => configuration.GetValueWithDevDefault(key, environment.IsDevelopment(), () => devDefault);
}
