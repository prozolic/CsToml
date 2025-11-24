using CsToml.Error;
using CsToml.Formatter.Resolver;
using Shouldly;
using Utf8StringInterpolation;
using CsToml.Generator.Other;

namespace CsToml.Generator.Tests;

public class TypeNullBehaviorFieldLevelTest
{
    [Fact]
    public void Serialize_FieldLevelSkipNull_ShouldSkipNullFields()
    {
        var type = new TypeNullBehaviorFieldLevel
        {
            Name = "Test",
            RequiredField = "Value",
            OptionalField = null,
            OptionalNumber = null
        };

        using var bytes = CsTomlSerializer.Serialize(type);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Name = \"Test\"");
        writer.AppendLine("RequiredField = \"Value\"");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void Serialize_FieldLevelSkipNull_WithValues_ShouldIncludeAll()
    {
        var type = new TypeNullBehaviorFieldLevel
        {
            Name = "Test",
            RequiredField = "Required",
            OptionalField = "Optional",
            OptionalNumber = 123
        };

        using var bytes = CsTomlSerializer.Serialize(type);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Name = \"Test\"");
        writer.AppendLine("RequiredField = \"Required\"");
        writer.AppendLine("OptionalField = \"Optional\"");
        writer.AppendLine("OptionalNumber = 123");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void Serialize_FieldLevelSkipNull_RequiredFieldNull_ShouldThrow()
    {
        var type = new TypeNullBehaviorFieldLevel
        {
            Name = "Test",
            RequiredField = null,
            OptionalField = null,
            OptionalNumber = null
        };

        Should.Throw<CsTomlSerializeException>(() => CsTomlSerializer.Serialize(type));
    }

    [Fact]
    public void Deserialize_MissingOptionalFields_ShouldSucceed()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Name = \"Test\"");
        writer.AppendLine("RequiredField = \"Value\"");
        writer.Flush();

        var type = CsTomlSerializer.Deserialize<TypeNullBehaviorFieldLevel>(buffer.WrittenSpan);
        type.Name.ShouldBe("Test");
        type.RequiredField.ShouldBe("Value");
        type.OptionalField.ShouldBeNull();
        type.OptionalNumber.ShouldBeNull();
    }

    [Fact]
    public void RoundTrip_PartialNull_ShouldMaintainValues()
    {
        var original = new TypeNullBehaviorFieldLevel
        {
            Name = "RoundTrip",
            RequiredField = "Required",
            OptionalField = "HasValue",
            OptionalNumber = null
        };

        using var serialized = CsTomlSerializer.Serialize(original);
        var deserialized = CsTomlSerializer.Deserialize<TypeNullBehaviorFieldLevel>(serialized.ByteSpan);

        deserialized.Name.ShouldBe(original.Name);
        deserialized.RequiredField.ShouldBe(original.RequiredField);
        deserialized.OptionalField.ShouldBe(original.OptionalField);
        deserialized.OptionalNumber.ShouldBeNull();
    }
}
