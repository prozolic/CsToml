using CsToml.Error;
using CsToml.Formatter.Resolver;
using Shouldly;
using Utf8StringInterpolation;

namespace CsToml.Generator.Tests;

public class TypeNullBehaviorMixedTest
{
    [Fact]
    public void Serialize_FieldOverridesGlobal_SkipNullField()
    {
        var type = new TypeNullBehaviorMixed
        {
            Name = "Mixed",
            SkippableField = null,
            NonSkippableField = null
        };

        // SkippableField has SkipNull = true, so it should be skipped
        // NonSkippableField has no SkipNull, so it uses global Default and should throw
        Should.Throw<CsTomlSerializeException>(() => CsTomlSerializer.Serialize(type, Option.ErrorTomlNullHandling));
    }

    [Fact]
    public void Serialize_FieldOverridesGlobal_WithGlobalSkip()
    {
        var type = new TypeNullBehaviorMixed
        {
            Name = "Mixed",
            SkippableField = null,
            NonSkippableField = null
        };

        using var bytes = CsTomlSerializer.Serialize(type, Option.IgnoreTomlNullHandling);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Name = \"Mixed\"");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void Serialize_MixedValues_ShouldHandleCorrectly()
    {
        var type = new TypeNullBehaviorMixed
        {
            Name = "Test",
            SkippableField = "Skippable",
            NonSkippableField = "NonSkippable"
        };

        using var bytes = CsTomlSerializer.Serialize(type);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Name = \"Test\"");
        writer.AppendLine("SkippableField = \"Skippable\"");
        writer.AppendLine("NonSkippableField = \"NonSkippable\"");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Theory]
    [InlineData(null, "Value")]
    [InlineData("Value", null)]
    [InlineData("Value1", "Value2")]
    public void Serialize_VariousCombinations_WithGlobalSkip(string? skippable, string? nonSkippable)
    {
        var type = new TypeNullBehaviorMixed
        {
            Name = "Theory",
            SkippableField = skippable,
            NonSkippableField = nonSkippable
        };

        using var bytes = CsTomlSerializer.Serialize(type, Option.IgnoreTomlNullHandling);
        var deserialized = CsTomlSerializer.Deserialize<TypeNullBehaviorMixed>(bytes.ByteSpan);

        deserialized.Name.ShouldBe("Theory");
        deserialized.SkippableField.ShouldBe(skippable);
        deserialized.NonSkippableField.ShouldBe(nonSkippable);
    }
}
