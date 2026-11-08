using CsToml.Formatter;
using CsToml.Formatter.Resolver;
using Shouldly;

namespace CsToml.Generator.Tests;

public class FormatterResolverTest
{
    [Test]
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

    [Test]
    public void TypeIsNotPublishedBeforeItsMemberFormatters()
    {
        // Thread A resolves TypeA1Outer for the first time and is held inside TypeA1Slow's static constructor,
        // i.e. halfway through the generated Register(). Thread B must wait for that registration instead of
        // getting a TypeA1Outer whose member formatters are not registered yet.
        var toml = "Id = 1\n"u8.ToArray();
        TypeA1Outer? resultA = null;
        TypeA1Outer? resultB = null;
        var a = new Thread(() => resultA = CsTomlSerializer.Deserialize<TypeA1Outer>(toml)) { IsBackground = true };
        var b = new Thread(() => resultB = CsTomlSerializer.Deserialize<TypeA1Outer>(toml)) { IsBackground = true };
        try
        {
            a.Start();
            TypeA1Gate.EnteredSlowConstructor.Wait(TimeSpan.FromSeconds(30)).ShouldBeTrue();

            b.Start();
            b.Join(TimeSpan.FromMilliseconds(500)).ShouldBeFalse();
        }
        finally
        {
            TypeA1Gate.ReleaseSlowConstructor.Set();
        }

        a.Join(TimeSpan.FromSeconds(30)).ShouldBeTrue();
        b.Join(TimeSpan.FromSeconds(30)).ShouldBeTrue();
        resultA!.Id.ShouldBe(1);
        resultB!.Id.ShouldBe(1);
        TomlValueFormatterResolver.IsRegistered<List<TypeA1Inner>>().ShouldBeTrue();
    }

    [Test]
    public void SelfReferencingTypeRegistersAndRoundTrips()
    {
        var toml = "Value = 1\nChildren = [{ Value = 2, Children = [] }, { Value = 3, Children = [{ Value = 4, Children = [] }] }]\n"u8;
        var node = CsTomlSerializer.Deserialize<TypeCycleNode>(toml);
        node.Value.ShouldBe(1);
        node.Children.Count.ShouldBe(2);
        node.Children[1].Children[0].Value.ShouldBe(4);
        TomlValueFormatterResolver.IsRegistered<TypeCycleNode>().ShouldBeTrue();
        TomlValueFormatterResolver.IsRegistered<List<TypeCycleNode>>().ShouldBeTrue();

        using var bytes = CsTomlSerializer.Serialize(node);
        var again = CsTomlSerializer.Deserialize<TypeCycleNode>(bytes.ByteSpan);
        again.Children[1].Children[0].Value.ShouldBe(4);
    }
}
