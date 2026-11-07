using CsToml.Error;
using CsToml.Formatter;
using CsToml.Formatter.Resolver;
using System.Buffers;

namespace CsToml.Tests;

public class FormatterResolverTest
{
    private sealed class ObjectFormatter : ITomlValueFormatter<object>
    {
        public object Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
            => throw new NotSupportedException();

        public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, object target, CsTomlSerializerOptions options)
            where TBufferWriter : IBufferWriter<byte>
            => throw new NotSupportedException();
    }

    private sealed class IntFormatter : ITomlValueFormatter<int>
    {
        public int Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
            => throw new NotSupportedException();

        public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, int target, CsTomlSerializerOptions options)
            where TBufferWriter : IBufferWriter<byte>
            => throw new NotSupportedException();
    }

    private sealed class Marker { }

    private sealed class MarkerFormatter : ITomlValueFormatter<Marker>
    {
        public Marker Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
            => throw new NotSupportedException();

        public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, Marker target, CsTomlSerializerOptions options)
            where TBufferWriter : IBufferWriter<byte>
            => throw new NotSupportedException();
    }

    private sealed class LateMarker { }

    private sealed class LateMarkerFormatter : ITomlValueFormatter<LateMarker>
    {
        public LateMarker Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
            => throw new NotSupportedException();

        public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, LateMarker target, CsTomlSerializerOptions options)
            where TBufferWriter : IBufferWriter<byte>
            => throw new NotSupportedException();
    }

    private sealed class LazyMarker { }

    private sealed class CountingLazyMarkerFormatter : ITomlValueFormatter<LazyMarker>
    {
        public static int Constructed;

        public CountingLazyMarkerFormatter()
        {
            Interlocked.Increment(ref Constructed);
        }

        public LazyMarker Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
            => throw new NotSupportedException();

        public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, LazyMarker target, CsTomlSerializerOptions options)
            where TBufferWriter : IBufferWriter<byte>
            => throw new NotSupportedException();
    }

    private sealed class CountingIntFormatter : ITomlValueFormatter<int>
    {
        public static int Constructed;

        public CountingIntFormatter()
        {
            Interlocked.Increment(ref Constructed);
        }

        public int Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
            => throw new NotSupportedException();

        public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, int target, CsTomlSerializerOptions options)
            where TBufferWriter : IBufferWriter<byte>
            => throw new NotSupportedException();
    }

    [Fact]
    public void LazyRegisterConstructsTheFormatterOnce()
    {
        TomlValueFormatterResolver.Register<LazyMarker, CountingLazyMarkerFormatter>();
        TomlValueFormatterResolver.Register<LazyMarker, CountingLazyMarkerFormatter>();
        TomlValueFormatterResolver.Register<LazyMarker, CountingLazyMarkerFormatter>();

        CountingLazyMarkerFormatter.Constructed.ShouldBe(1);
        TomlValueFormatterResolver.Instance.GetFormatter<LazyMarker>().ShouldBeOfType<CountingLazyMarkerFormatter>();
    }

    [Fact]
    public void LazyRegisterDoesNotConstructWhenBuiltinExists()
    {
        TomlValueFormatterResolver.Register<int, CountingIntFormatter>();

        CountingIntFormatter.Constructed.ShouldBe(0);
        TomlValueFormatterResolver.Instance.GetFormatter<int>().ShouldBeSameAs(Int32Formatter.Instance);
    }

    [Fact]
    public void ConcurrentRegisterPublishesExactlyOneInstance()
    {
        const int threadCount = 16;
        var formatters = new MarkerFormatter[threadCount];
        var results = new ITomlValueFormatter<Marker>?[threadCount];
        using var start = new ManualResetEventSlim();
        var threads = new Thread[threadCount];
        for (var i = 0; i < threadCount; i++)
        {
            var index = i;
            formatters[index] = new MarkerFormatter();
            threads[index] = new Thread(() =>
            {
                start.Wait();
                TomlValueFormatterResolver.Register(formatters[index]);
                results[index] = TomlValueFormatterResolver.Instance.GetFormatter<Marker>();
            });
            threads[index].Start();
        }
        start.Set();
        foreach (var thread in threads)
        {
            thread.Join();
        }

        var published = TomlValueFormatterResolver.Instance.GetFormatter<Marker>();
        formatters.ShouldContain(published);
        results.ShouldAllBe(r => ReferenceEquals(r, published));
    }

    [Fact]
    public void RegisterAfterFailedLookup()
    {
        TomlValueFormatterResolver.IsRegistered<LateMarker>().ShouldBeFalse();
        Should.Throw<CsTomlException>(() => TomlValueFormatterResolver.Instance.GetFormatter<LateMarker>());

        var formatter = new LateMarkerFormatter();
        TomlValueFormatterResolver.Register(formatter);
        TomlValueFormatterResolver.IsRegistered<LateMarker>().ShouldBeTrue();
        TomlValueFormatterResolver.Instance.GetFormatter<LateMarker>().ShouldBeSameAs(formatter);
    }

    [Fact]
    public void ObjectIsRegisteredAsBuiltin()
    {
        TomlValueFormatterResolver.IsRegistered<object>().ShouldBeTrue();
        TomlValueFormatterResolver.Instance.GetFormatter<object>().ShouldBeSameAs(PrimitiveObjectFormatter.Instance);
    }

    [Fact]
    public void RegisterDoesNotReplaceBuiltinFormatter()
    {
        TomlValueFormatterResolver.Register(new ObjectFormatter());
        TomlValueFormatterResolver.Instance.GetFormatter<object>().ShouldBeSameAs(PrimitiveObjectFormatter.Instance);

        TomlValueFormatterResolver.Register(new IntFormatter());
        TomlValueFormatterResolver.Instance.GetFormatter<int>().ShouldBeSameAs(Int32Formatter.Instance);
    }
}
