using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.RegularExpressions;
using KiteKey.Core;
using KiteKey.Core.Attributes;
using KiteKey.Core.Concurrency;
using KiteKey.Core.Models.Calendars;
using KiteKey.Core.Models.Base;
using KiteKey.Core.Utility;
using KiteKey.Core.Utility.Files;
using KiteKey.Core.Utility.Files.Physical;
using KiteKey.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace KiteKey.Tests;

public class ExpandedUtilityTests
{
    [Fact]
    public async Task AsyncSequence_QueriesAndProjectsWithoutExternalPackages()
    {
        IAsyncEnumerable<int> source = new[] { 1, 2, 3 }.AsAsyncEnumerable();
        Assert.True(await source.Any(item => item == 2));
        Assert.Equal(3, await source.Count());
        Assert.Equal(new[] { 4, 6 }, await source.Where(x => x > 1).Select(x => x * 2).ToListAsync());
        Assert.Equal(3, await source.Max());
        Assert.Equal(1, await source.FirstAsync());
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await source.SingleAsync());
    }

    [Fact]
    public async Task TextAndValueHelpers_WorkWithOrdinaryTypes()
    {
        using StringReader reader = new("first\nsecond\n");
        Assert.Equal(new[] { "first", "second" }, reader.EnumerateLines());
        Assert.Equal("value", Regex.Match("value", "(?<item>value)").GetRequiredGroup("item"));
        Assert.Equal(new NumericValue { Value = 7 }, new NumericValue { Value = 7 });
        Assert.True(new NumericValue { Value = 7 }.CompareTo(new NumericValue { Value = 8 }) < 0);
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();
        Assert.Equal(cancellation.Token, await cancellation.Token);
    }

    [Fact]
    public void ListAndStringHelpers_HandleValuesAndInvalidGridDimensions()
    {
        List<string> items = ["b", "a"];
        items.Sort(ListSortDirection.Ascending);
        Assert.Equal(new[] { "a", "b" }, items);
        Assert.Equal(2, items.ToDictionaryList(x => x).Count);
        Assert.Equal(new[] { "a", "b" }, items.SelectArray(x => x));
        Assert.Throws<ArgumentOutOfRangeException>(() => items.To2DArray(0, 0));
        Assert.Equal("A B", " A   B ".CollapseWhitespace());
        Assert.Equal("Hello", "Hello!.".RemovePunctuation());
        Assert.NotEmpty("x".Obfuscate());
        Assert.Equal("V", "Value".Abbreviate());
    }

    [Fact]
    public void EnumHelpers_ParseNumericValuesAndMapAttributedMembers()
    {
        Assert.Equal(Mapped.Source, 1.ToEnum<Mapped>());
        Assert.Equal(Mapped.Source, "Source".ToEnum<Mapped>());
        Assert.Equal(Target.Value, Mapped.Source.MapTo<Target>());
        Assert.True(Mapped.Source.IsSortable());
    }

    [Fact]
    public void DateHelpers_RetainKindAndSubsecondPrecision()
    {
        DateTime original = new(2026, 9, 28, 12, 30, 40, 123, DateTimeKind.Utc);
        original = original.AddTicks(4567);
        DateTime updated = original.SetDay(15);
        Assert.Equal(DateTimeKind.Utc, updated.Kind);
        Assert.Equal(original.TimeOfDay, updated.TimeOfDay);
        Assert.Equal(new DateTime(2026, 10, 28, 12, 30, 40, 123, DateTimeKind.Utc).AddTicks(4567), original.Add(CalendarUnit.Months, 1));
        DateTime newTime = original.SetTime(new TimeOnly(7, 12, 45, 678).Add(TimeSpan.FromTicks(4567)));
        Assert.Equal(DateTimeKind.Utc, newTime.Kind);
        Assert.Equal(6784567, newTime.Ticks % TimeSpan.TicksPerSecond);
    }

    [Fact]
    public void AssemblyHelpers_FindAndRegisterGenericImplementations()
    {
        ServiceCollection services = new();
        Assert.True(services.TryAddService(typeof(Concrete), typeof(IContract<>), ServiceLifetime.Transient));
        Assert.False(services.TryAddService(typeof(string), typeof(IContract<>), ServiceLifetime.Transient));
        using ServiceProvider provider = services.BuildServiceProvider();
        Assert.IsType<Concrete>(provider.GetRequiredService<IContract<int>>());
        Assert.Contains(typeof(Concrete), typeof(Concrete).Assembly.GetImplementationTypes<IContract<int>>());
    }

    [Fact]
    public void PhysicalFiles_RejectPathsOutsideDirectory()
    {
        PhysicalFileDirectory directory = new(Directory.GetCurrentDirectory());
        Assert.Throws<ArgumentException>(() => directory.GetEntry(".." + Path.DirectorySeparatorChar + "outside"));
        Assert.Throws<ArgumentException>(() => directory.GetSubdirectory(".."));
        Assert.Equal("file.txt", directory.GetEntry("file.txt").Name);
    }

    [Fact]
    public async Task Debouncer_BatchesQueuedValues()
    {
        TaskCompletionSource<int[]> processed = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using Debouncer<int> debouncer = new(values =>
        {
            processed.TrySetResult(values.ToArray());
            return Task.CompletedTask;
        }, TimeSpan.FromMilliseconds(20));
        debouncer.Enqueue(1);
        debouncer.Enqueue(2);
        Assert.Equal(new[] { 1, 2 }, await processed.Task.WaitAsync(TimeSpan.FromSeconds(3)));
    }

    [Fact]
    public async Task Throttler_EnforcesCapacityAndChecksConstructor()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Throttler(0, TimeSpan.FromSeconds(1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Throttler(2, TimeSpan.Zero));
        using Throttler throttler = new(1, TimeSpan.FromSeconds(30));
        Assert.True(throttler.TryAction());
        Assert.False(throttler.TryAction());
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => throttler.DelayAction(cancellation.Token));
    }

    [Fact]
    public void FileRotation_PrunesOldestByTimestampAcrossDifferentPrefixes()
    {
        string path = Path.Combine(Directory.GetCurrentDirectory(), $"kitekey-rotation-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        try
        {
            FileRotation rotation = new(path, 1, "log");
            RotationFileInfo old = rotation.CreateFileName(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Local), ["z"]);
            RotationFileInfo recent = rotation.CreateFileName(new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Local), ["a"]);
            File.WriteAllText(old.Entry.PhysicalPath, "old");
            File.WriteAllText(recent.Entry.PhysicalPath, "new");
            Assert.Throws<ArgumentException>(() => rotation.CreateFileName(["../escape"]));
            Assert.Equal(2, rotation.ListFiles().Count());
            Assert.Equal(1, rotation.PruneFiles(null, 1));
            Assert.False(File.Exists(old.Entry.PhysicalPath));
            Assert.True(File.Exists(recent.Entry.PhysicalPath));
        }
        finally
        {
            Directory.Delete(path, recursive: true);
        }
    }

    [Fact]
    public void ValidatedAttribute_DetectsMissingAnnotatedValues()
    {
        ValidatedAttribute attribute = new();
        ValidationContext context = new(new DataModel());
        ValidationResult? result = attribute.GetValidationResult(new DataModel(), context);
        Assert.NotNull(result);
        Assert.Contains("Name", result.ErrorMessage);
    }

    [Fact]
    public void SlugHelper_HandlesNamesAndNumericSuffixes()
    {
        Assert.Equal("test-name", SlugHelper.CreateSlugFromName(" Test, Name "));
        Assert.Equal("item2-2", SlugHelper.Increment("item2"));
        Assert.Equal("item2-3", SlugHelper.Increment("item2-2"));
        Assert.Equal("?a%20b=c%26d", SlugHelper.AddQueryStringValue("", "a b", "c&d"));
    }

    [Fact]
    public async Task DisposableStream_DisposesOwnedResourcesForBothPaths()
    {
        TrackingStream stream = new();
        TrackingResource owned = new();
        await using (DisposableStream wrapper = new(stream, owned))
        {
            await wrapper.WriteAsync(new byte[] { 1, 2 });
        }
        Assert.True(stream.Disposed);
        Assert.True(owned.Disposed);

        TrackingResource synchronous = new();
        new DisposableStream(new MemoryStream(), synchronous).Dispose();
        Assert.True(synchronous.Disposed);
    }

    private enum Mapped { [MapsTo(Target.Value), Sortable] Source = 1 }
    private enum Target { Value }
    private interface IContract<T> { }
    public sealed class Concrete : IContract<int> { }
    private sealed class DataModel { public string Name { get; set; } = ""; }
    private sealed class NumericValue : SimpleValueBase<NumericValue, int> { }
    private sealed class TrackingResource : IDisposable
    {
        public bool Disposed { get; private set; }
        public void Dispose() => Disposed = true;
    }
    private sealed class TrackingStream : MemoryStream
    {
        public bool Disposed { get; private set; }
        protected override void Dispose(bool disposing)
        {
            Disposed = true;
            base.Dispose(disposing);
        }
    }
}
