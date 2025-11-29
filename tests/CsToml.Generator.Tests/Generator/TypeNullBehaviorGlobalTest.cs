using CsToml.Error;
using CsToml.Formatter.Resolver;
using Shouldly;
using Utf8StringInterpolation;

namespace CsToml.Generator.Tests;

public class TypeNullBehaviorGlobalTest
{
    [Fact]
    public void Serialize_GlobalSkip_AllNull_ShouldSkipAll()
    {
        var type = new TypeNullBehaviorAllNullable
        {
            Field1 = null,
            Field2 = null,
            Field3 = null
        };

        using var bytes = CsTomlSerializer.Serialize(type, Option.IgnoreTomlNullHandling);

        bytes.ByteSpan.ToArray().ShouldBe(""u8.ToArray());
    }

    [Fact]
    public void Serialize_GlobalSkip_MixedNull_ShouldSkipOnlyNull()
    {
        var type = new TypeNullBehaviorAllNullable
        {
            Field1 = "Value1",
            Field2 = null,
            Field3 = 123
        };

        using var bytes = CsTomlSerializer.Serialize(type, Option.IgnoreTomlNullHandling);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Field1 = \"Value1\"");
        writer.AppendLine("Field3 = 123");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void Serialize_GlobalDefault_HasNull_ShouldThrow()
    {
        var type = new TypeNullBehaviorAllNullable
        {
            Field1 = "Value",
            Field2 = null,
            Field3 = 123
        };

        Should.Throw<CsTomlSerializeException>(() => CsTomlSerializer.Serialize(type, Option.ErrorTomlNullHandling));
    }

    [Fact]
    public void RoundTrip_GlobalSkip_ShouldWork()
    {
        var original = new TypeNullBehaviorAllNullable
        {
            Field1 = "Test",
            Field2 = null,
            Field3 = 456
        };

        using var serialized = CsTomlSerializer.Serialize(original, Option.IgnoreTomlNullHandling);
        var deserialized = CsTomlSerializer.Deserialize<TypeNullBehaviorAllNullable>(serialized.ByteSpan);

        deserialized.Field1.ShouldBe(original.Field1);
        deserialized.Field2.ShouldBeNull();
        deserialized.Field3.ShouldBe(original.Field3);
    }
}
