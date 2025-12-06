using Shouldly;
using Utf8StringInterpolation;

namespace CsToml.Generator.Tests;

public class TypeAliasTest
{
    private TypeAlias type;

    public TypeAliasTest()
    {
        type = new TypeAlias() { Value = "This is TypeAlias" };
    }

    [Fact]
    public void Serialize()
    {
        using var bytes = CsTomlSerializer.Serialize(type);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("alias = \"This is TypeAlias\"");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("alias = \"This is TypeAlias\"");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithArrayHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(type, Option.ArrayHeader);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("alias = \"This is TypeAlias\"");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithHeaderAndArrayHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(type, Option.HeaderAndArrayHeader);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("alias = \"This is TypeAlias\"");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void Deserialize()
    {
        var type = CsTomlSerializer.Deserialize<TypeAlias>("alias = \"This is TypeAlias\""u8);
        type.Value.ShouldBe("This is TypeAlias");
    }
}
