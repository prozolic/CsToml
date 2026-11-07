using CsToml.Formatter;
using CsToml.Formatter.Resolver;
using Shouldly;

namespace CsToml.Generator.Tests;

public class FormatterResolverTest
{
    [Fact]
    public void ConcurrentLookupResolvesGeneratedTypeOnce()
    {
        const int threadCount = 16;
        var results = new ITomlValueFormatter<TypeConcurrentResolve>?[threadCount];
        using var start = new ManualResetEventSlim();
        var threads = new Thread[threadCount];
        for (var i = 0; i < threadCount; i++)
        {
            var index = i;
            threads[index] = new Thread(() =>
            {
                start.Wait();
                results[index] = TomlValueFormatterResolver.Instance.GetFormatter<TypeConcurrentResolve>();
            });
            threads[index].Start();
        }
        start.Set();
        foreach (var thread in threads)
        {
            thread.Join();
        }

        var published = TomlValueFormatterResolver.Instance.GetFormatter<TypeConcurrentResolve>();
        published.ShouldBeOfType<TomlSerializedObjectFormatter<TypeConcurrentResolve>>();
        results.ShouldAllBe(r => ReferenceEquals(r, published));

        // The lookup ran the generated Register(), which also registered the member types.
        TomlValueFormatterResolver.IsRegistered<TypeConcurrentResolveChild>().ShouldBeTrue();
        TomlValueFormatterResolver.IsRegistered<TypeConcurrentResolveChild[]>().ShouldBeTrue();

        var value = new TypeConcurrentResolve()
        {
            Value = 1,
            Child = new() { Name = "child" },
            Children = [new() { Name = "a" }, new() { Name = "b" }],
        };
        using var bytes = CsTomlSerializer.Serialize(value);
        var deserialized = CsTomlSerializer.Deserialize<TypeConcurrentResolve>(bytes.ByteSpan);
        deserialized.Value.ShouldBe(1);
        deserialized.Child.Name.ShouldBe("child");
        deserialized.Children.Length.ShouldBe(2);
    }
}
