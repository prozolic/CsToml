using Shouldly;
using System.Collections;
using Utf8StringInterpolation;

namespace CsToml.Generator.Tests;

public class TypeHashtableTest
{
    private TypeHashtable typeHashtable;

    public TypeHashtableTest()
    {
        typeHashtable = new TypeHashtable()
        {
            Value = new Hashtable()
            {
                [123] = "Value",
                [-1] = "Value",
                [123456789] = "Value",
            }
        };
    }

    [Fact]
    public void Serialize()
    {
        using var bytes = CsTomlSerializer.Serialize(typeHashtable);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = {123 = \"Value\", 123456789 = \"Value\", -1 = \"Value\"}");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(typeHashtable, Option.Header);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = {123 = \"Value\", 123456789 = \"Value\", -1 = \"Value\"}");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithArrayHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(typeHashtable, Option.ArrayHeader);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = {123 = \"Value\", 123456789 = \"Value\", -1 = \"Value\"}");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithHeaderAndArrayHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(typeHashtable, Option.HeaderAndArrayHeader);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = {123 = \"Value\", 123456789 = \"Value\", -1 = \"Value\"}");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);

    }

    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = {123 = \"Value\", -1 = \"Value2\", 123456789 = \"Value3\" }");
        writer.Flush();

        var type = CsTomlSerializer.Deserialize<TypeHashtable>(buffer.WrittenSpan);
        string value = (string)type.Value["123"]!;
        value.ShouldBe("Value");
        string value2 = (string)type.Value["-1"]!;
        value2.ShouldBe("Value2");
        string value3 = (string)type.Value["123456789"]!;
        value3.ShouldBe("Value3");
    }
}
