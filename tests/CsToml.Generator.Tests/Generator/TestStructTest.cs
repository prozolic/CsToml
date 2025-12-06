using Shouldly;
using Utf8StringInterpolation;
using CsToml.Generator.Other;

namespace CsToml.Generator.Tests;

public class TestStructTest
{
    private TestStruct type;

    public TestStructTest()
    {
        type = new TestStruct
        {
            Value = 999,
            Str = "Test"
        };
    }

    [Fact]
    public void Serialize()
    {
        using var bytes = CsTomlSerializer.Serialize(type);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = 999");
        writer.AppendLine("Str = \"Test\"");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = 999");
        writer.AppendLine("Str = \"Test\"");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithArrayHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(type, Option.ArrayHeader);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = 999");
        writer.AppendLine("Str = \"Test\"");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithHeaderAndArrayHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(type, Option.HeaderAndArrayHeader);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = 999");
        writer.AppendLine("Str = \"Test\"");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = 999");
        writer.AppendLine("Str = \"Test\"");
        writer.Flush();

        var type = CsTomlSerializer.Deserialize<TestStruct>(buffer.WrittenSpan);
        type.Value.ShouldBe(999);
        type.Str.ShouldBe("Test");
    }
}
