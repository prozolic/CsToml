using CsToml.Error;
using CsToml.Generator.Other;
using Shouldly;
using Utf8StringInterpolation;

namespace CsToml.Generator.Tests;

public class NullableTypeTest
{
    [Test]
    public void Serialize()
    {
        var type = new NullableType()
        {
            Value = 999
        };

        {
            using var bytes = CsTomlSerializer.Serialize(type);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = 999");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = 999");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
    }

    [Test]
    public void SerializeNull()
    {
        var throwAction = () => { using var bytes = CsTomlSerializer.Serialize(new NullableType()); };
        throwAction.ShouldThrow<CsTomlException>();
    }

    [Test]
    public void Deserialize()
    {
        var type = CsTomlSerializer.Deserialize<NullableType>(@"Value = 999"u8);
        type.Value.ShouldBe(999);
    }

    [Test]
    public void DeserializeNull()
    {
        var type = CsTomlSerializer.Deserialize<NullableType>(@""u8);
        type.Value.ShouldBeNull();
    }

    [Test]
    public void SerializeStruct()
    {
        var type = new NullableStructType()
        {
            Value = new TestStruct { Value = 1, Str = "Test" }
        };

        {
            using var bytes = CsTomlSerializer.Serialize(type);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value.Value = 1");
            writer.AppendLine("Value.Str = \"Test\"");
            writer.Flush();

            bytes.ByteSpan.ToArray().ShouldBe(buffer.ToArray());
            CsTomlSerializer.Deserialize<NullableStructType>(bytes.ByteSpan).Value.ShouldBe(type.Value);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("[Value]");
            writer.AppendLine("Value = 1");
            writer.AppendLine("Str = \"Test\"");
            writer.Flush();

            bytes.ByteSpan.ToArray().ShouldBe(buffer.ToArray());
            CsTomlSerializer.Deserialize<NullableStructType>(bytes.ByteSpan).Value.ShouldBe(type.Value);
        }
    }
}
