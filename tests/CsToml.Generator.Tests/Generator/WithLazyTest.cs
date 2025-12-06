using Shouldly;
using Utf8StringInterpolation;

namespace CsToml.Generator.Tests;

public class WithLazyTest
{
    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Int = 123");
        writer.AppendLine("NullableInt = 123");
        writer.AppendLine("Str = \"Lazy<string>\"");
        writer.AppendLine("IntList = [ 1, 2, 3 ]");
        writer.Flush();

        var withLazy = CsTomlSerializer.Deserialize<WithLazy>(buffer.WrittenSpan);
        withLazy.Int.Value.ShouldBe(123);
        withLazy.NullableInt.Value.ShouldBe(123);
        withLazy.Str.Value.ShouldBe("Lazy<string>");
        withLazy.IntList.Value.ShouldBe([1, 2, 3]);
    }

    [Fact]
    public void Serialize()
    {
        var type = new WithLazy()
        {
            Int = new Lazy<int>(() => 123),
            NullableInt = new Lazy<int?>(() => 123),
            Str = new Lazy<string>(() => "Lazy<string>"),
            IntList = new Lazy<List<int>>(() => [1, 2, 3]),
        };
        using var bytes = CsTomlSerializer.Serialize(type);
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Int = 123");
        writer.AppendLine("NullableInt = 123");
        writer.AppendLine("Str = \"Lazy<string>\"");
        writer.AppendLine("IntList = [ 1, 2, 3 ]");
        writer.Flush();
        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }
}
