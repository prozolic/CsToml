
#if NET9_0_OR_GREATER

using Shouldly;
using Utf8StringInterpolation;

namespace CsToml.Generator.Tests;

public class TypeOrderedDictionaryTest
{
    private TypeDictionary typeDictionary;

    public TypeOrderedDictionaryTest()
    {
        var dict = new OrderedDictionary<string, object?>()
        {
            ["key"] = new object[]
            {
                999,
                "Value",
                new OrderedDictionary<string, object?>()
                {
                    ["key"] = new object[]
                    {
                        new long[] {1, 2, 3},
                        new OrderedDictionary<string, object?>()
                        {
                            ["key"] = "value"
                        }
                    }
                }
            }
        };

        typeDictionary = new TypeDictionary() { Value = dict };
    }

    [Fact]
    public void Serialize()
    {
        using var bytes = CsTomlSerializer.Serialize(typeDictionary);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3 ], {key = \"value\"} ]} ]}");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(typeDictionary, Option.Header);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("[Value]");
        writer.AppendLine("key = [ 999, \"Value\", {key = [ [ 1, 2, 3 ], {key = \"value\"} ]} ]");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithArrayHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(typeDictionary, Option.ArrayHeader);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3 ], {key = \"value\"} ]} ]}");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithHeaderAndArrayHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(typeDictionary, Option.HeaderAndArrayHeader);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("[Value]");
        writer.AppendLine("key = [ 999, \"Value\", {key = [ [ 1, 2, 3 ], {key = \"value\"} ]} ]");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void Deserialize()
    {
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\"}]}]}");
            writer.Flush();

            var type = CsTomlSerializer.Deserialize<TypeOrderedDictionary>(buffer.WrittenSpan);
            Validate(type);
        }
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("[Value]");
            writer.AppendLine("key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\"}]}]");
            writer.Flush();

            var type = CsTomlSerializer.Deserialize<TypeOrderedDictionary>(buffer.WrittenSpan);
            Validate(type);
        }


        static void Validate(TypeOrderedDictionary typeOrderedDictionary)
        {
            dynamic dynamicDict = typeOrderedDictionary.Value;

            long value = dynamicDict["key"][0];
            value.ShouldBe(999);
            string value2 = dynamicDict["key"][1];
            value2.ShouldBe("Value");
            object[] value3 = dynamicDict["key"][2]["key"][0];
            value3.ShouldBe(new object[] { 1, 2, 3 });
            string value4 = dynamicDict["key"][2]["key"][1]["key"];
            value4.ShouldBe("value");

        }
    }
}

#endif
