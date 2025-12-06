#if NET9_0_OR_GREATER

using Shouldly;
using Utf8StringInterpolation;
using System.Collections.ObjectModel;

namespace CsToml.Generator.Tests;

public class TypeReadOnlySetFormatterTest
{
    private TypeReadOnlySetFormatter typeReadOnlySetFormatter;

    public TypeReadOnlySetFormatterTest()
    {
        typeReadOnlySetFormatter = new TypeReadOnlySetFormatter
        {
            Value = new ReadOnlySet<long>(new HashSet<long>([1, 2, 3, 4, 5])),
        };
    }

    [Fact]
    public void Serialize()
    {
        using var bytes = CsTomlSerializer.Serialize(typeReadOnlySetFormatter);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = [ 1, 2, 3, 4, 5 ]");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(typeReadOnlySetFormatter, Option.Header);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = [ 1, 2, 3, 4, 5 ]");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithArrayHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(typeReadOnlySetFormatter, Option.ArrayHeader);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = [ 1, 2, 3, 4, 5 ]");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithHeaderAndArrayHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(typeReadOnlySetFormatter, Option.HeaderAndArrayHeader);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = [ 1, 2, 3, 4, 5 ]");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = [ 1, 2, 3, 4, 5 ]");
        writer.Flush();

        var type = CsTomlSerializer.Deserialize<TypeReadOnlySetFormatter>(buffer.WrittenSpan);
        type.Value.ShouldBe(new ReadOnlySet<long>(new HashSet<long>([1, 2, 3, 4, 5])));
    }
}

#endif
