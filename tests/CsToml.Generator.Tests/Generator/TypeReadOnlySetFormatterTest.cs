using CsToml.Formatter.Resolver;
using Shouldly;
using System.Collections.Frozen;
using Utf8StringInterpolation;
using CsToml.Generator.Other;
using System.Collections.ObjectModel;

namespace CsToml.Generator.Tests;

#if NET9_0_OR_GREATER

public class TypeReadOnlySetFormatterTest
{
    [Fact]
    public void Serialize()
    {
        var type = new TypeReadOnlySetFormatter
        {
            Value = new ReadOnlySet<long>(new HashSet<long>([1, 2, 3, 4, 5])),
        };

        {
            using var bytes = CsTomlSerializer.Serialize(type);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = [ 1, 2, 3, 4, 5 ]");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = [ 1, 2, 3, 4, 5 ]");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
    }

    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = [1,3,5]");
        writer.Flush();

        var type = CsTomlSerializer.Deserialize<TypeReadOnlySetFormatter>(buffer.WrittenSpan);
        type.Value.ShouldBe(new ReadOnlySet<long>(new HashSet<long>([1, 3, 5])));
    }
}

#endif
