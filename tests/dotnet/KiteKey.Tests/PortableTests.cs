using System.Net.Mail;
using System.ComponentModel.DataAnnotations;
using KiteKey.Core.Caching;
using KiteKey.Core.Collections;
using KiteKey.Core.Concurrency;
using KiteKey.Core.Text;
using KiteKey.Hosting;
using KiteKey.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace KiteKey.Tests;

public class PortableTests
{
    [Fact]
    public void DictionaryList_CollectsAndCountsItems()
    {
        DictionaryList<string, int> items = new();
        items.Add("a", 1);
        items.Add("a", 2);
        items.Add("b", 3);
        Assert.Equal(2, items.GetCount("a"));
        Assert.Equal(0, items.GetCount("missing"));
        Assert.True(items.Contains("a", 2));
        Assert.Equal(new[] { 1, 2, 3 }, items.AllValues);
        IReadOnlyDictionaryList<string, int> readOnly = items;
        Assert.Equal(2, readOnly["a"].Count);
    }

    [Fact]
    public async Task KeyedCaches_LoadOnceAndReloadAfterRemoval()
    {
        int syncLoads = 0;
        KeyedCacheSync<string, int> sync = new(_ => ++syncLoads);
        Assert.Equal(1, sync.GetValue("a"));
        Assert.Equal(1, sync.GetValue("a"));
        Assert.True(sync.Remove("a"));
        Assert.Equal(2, sync.GetValue("a"));

        int asyncLoads = 0;
        KeyedCacheAsync<string, string, int> asyncCache = new((_, _, _) => Task.FromResult(++asyncLoads));
        Assert.Equal(1, await asyncCache.GetValue("a", "first"));
        Assert.Equal(1, await asyncCache.GetValue("a", "ignored"));
        asyncCache.Clear();
        Assert.Equal(2, await asyncCache.GetValue("a", "second"));
    }

    [Fact]
    public async Task FailedAsyncCacheLoad_IsNotCached()
    {
        int loads = 0;
        KeyedCacheAsync<int, int> cache = new(_ =>
            ++loads == 1 ? Task.FromException<int>(new InvalidOperationException()) : Task.FromResult(42));
        await Assert.ThrowsAsync<InvalidOperationException>(() => cache.GetValue(1));
        Assert.Equal(42, await cache.GetValue(1));
        Assert.Equal(2, loads);
    }

    [Fact]
    public async Task AsyncLock_RespectsCancellation()
    {
        using AsyncLock gate = new();
        using AsyncLock.Handle held = await gate.WaitAsync();
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => gate.WaitAsync(cancellation.Token));
    }

    [Fact]
    public void SpanReader_EnumeratesTrimmedLines()
    {
        List<string> lines = [];
        foreach (ReadOnlySpan<char> line in new SpanReader(" one \r\n two\n".AsSpan()))
            lines.Add(line.ToString());
        Assert.Equal(new[] { "one", "two" }, lines);
    }

    [Fact]
    public void ScopeValue_RequiresExplicitScopedAssignment()
    {
        ServiceCollection services = new();
        services.AddScopeValue<string>();
        using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope first = provider.CreateScope();
        Assert.Throws<InvalidOperationException>(() => first.ServiceProvider.GetRequiredService<string>());
        first.SetScopeValue("scope one");
        Assert.Equal("scope one", first.ServiceProvider.GetRequiredService<string>());
        using IServiceScope second = provider.CreateScope();
        Assert.Throws<InvalidOperationException>(() => second.ServiceProvider.GetRequiredService<string>());
    }

    [Fact]
    public void Configuration_RequiresValuesExceptForDevelopmentFallback()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Present"] = "value" }).Build();
        Assert.Equal("value", configuration.GetRequiredValue("Present"));
        Assert.Throws<InvalidOperationException>(() => configuration.GetRequiredValue("Missing"));
        Assert.Equal("fallback", configuration.GetValueWithDevDefault("Missing", true, "fallback"));
        Assert.Throws<InvalidOperationException>(() => configuration.GetValueWithDevDefault("Missing", false, "fallback"));
    }

    [Fact]
    public void HostingOptions_AreBoundAndValidated()
    {
        IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { ["Demo:Name"] = "" }).Build();
        ServiceCollection services = new();
        services.AddSingleton(configuration);
        services.AddOptionsValidated<DemoOptions>("Demo");
        using ServiceProvider provider = services.BuildServiceProvider();
        Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<DemoOptions>>().Value);
    }

    [Fact]
    public async Task Mail_DoesNotSendUnconfiguredOrDisallowedMessages()
    {
        using MailSender unconfigured = new(new MailSettings());
        using MailMessage missingConfig = unconfigured.CreateMailMessage("person@example.com", "Subject", "Body");
        Assert.False(unconfigured.CanSend);
        Assert.False(await unconfigured.SendAsync(missingConfig));

        MailSettings settings = new() { SmtpServer = "localhost", FromAddress = "sender@example.com" };
        using MailSender restricted = new(settings, ["allowed@example.com"]);
        using MailMessage message = restricted.CreateMailMessage("allowed@example.com", "Subject", "Body");
        message.Bcc.Add("blocked@example.com");
        Assert.True(restricted.CanSend);
        Assert.False(await restricted.SendAsync(message));
        Assert.Null(message.From);
    }

    [Fact]
    public void EmailBuilder_UsesNeutralGreetingAndOptionalSignature()
    {
        PlainTextEmailBuilder builder = new();
        builder.AppendGreeting("Ari");
        builder.AppendField("Status", "Ready");
        builder.AppendSignature("Team");
        string text = builder.ToString();
        Assert.Contains("Hello Ari,", text);
        Assert.Contains("Status", text);
        Assert.Contains("Ready", text);
        Assert.Contains("Team", text);
        Assert.DoesNotContain("OurGov", text);
    }

    public sealed class DemoOptions
    {
        [Required]
        public string? Name { get; set; }
    }
}
