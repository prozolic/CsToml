using Shouldly;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using Utf8StringInterpolation;

namespace CsToml.Generator.Tests;

public class TypeDictionaryTest
{
    private TypeDictionary typeDict;

    public TypeDictionaryTest()
    {
        typeDict = new TypeDictionary()
        {
            Value = new Dictionary<string, object?>()
            {
                ["key"] = new object[]
                {
                    999,
                    "Value",
                    new Dictionary<string, object?>()
                    {
                        ["key"] = new object[]
                        {
                            new long[] {1, 2, 3},
                            new Dictionary<string, object?>()
                            {
                                ["key"] = "value"
                            }
                        }
                    }
                }
            }
        };
    }

    [Fact]
    public void Serialize()
    {
        using var bytes = CsTomlSerializer.Serialize(typeDict);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3 ], {key = \"value\"} ]} ]}");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(typeDict, Option.Header);

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
        using var bytes = CsTomlSerializer.Serialize(typeDict, Option.ArrayHeader);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3 ], {key = \"value\"} ]} ]}");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithHeaderAndArrayHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(typeDict, Option.HeaderAndArrayHeader);

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
            writer.AppendLine("Value = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3 ], {key = \"value\"} ]} ]}");
            writer.Flush();

            var type = CsTomlSerializer.Deserialize<TypeDictionary>(buffer.WrittenSpan);
            dynamic dynamicDict = type.Value;

            long value = dynamicDict["key"][0];
            value.ShouldBe(999);
            string value2 = dynamicDict["key"][1];
            value2.ShouldBe("Value");
            object[] value3 = dynamicDict["key"][2]["key"][0];
            value3.ShouldBe(new object[] { 1, 2, 3 });
            string value4 = dynamicDict["key"][2]["key"][1]["key"];
            value4.ShouldBe("value");
        }
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("[Value]");
            writer.AppendLine("key = [ 999, \"Value\", {key = [ [ 1, 2, 3 ], {key = \"value\"} ]} ]");
            writer.Flush();

            var type = CsTomlSerializer.Deserialize<TypeDictionary>(buffer.WrittenSpan);
            dynamic dynamicDict = type.Value;

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

public class TypeDictionaryTest2
{
    [Fact]
    public void Serialize()
    {
        var dict = new Dictionary<object, object>()
        {
            ["key"] = new object[]
            {
                999,
                "Value",
                new Dictionary<object, object?>()
                {
                    ["key"] = new object[]
                    {
                        new long[] {1, 2, 3},
                        new Dictionary<object, object?>()
                        {
                            ["key"] = "value"
                        }
                    }
                }
            }
        };

        var type = new TypeDictionary2()
        {
            Value = dict,
            Value2 = dict.AsReadOnly(),
            Value3 = new SortedDictionary<object, object>(dict),
            Value4 = new ConcurrentDictionary<object, object>(dict),
            Value5 = dict,
            Value6 = dict,
        };
        {
            using var bytes = CsTomlSerializer.Serialize(type);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3 ], {key = \"value\"} ]} ]}");
            writer.AppendLine("Value2 = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3 ], {key = \"value\"} ]} ]}");
            writer.AppendLine("Value3 = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3 ], {key = \"value\"} ]} ]}");
            writer.AppendLine("Value4 = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3 ], {key = \"value\"} ]} ]}");
            writer.AppendLine("Value5 = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3 ], {key = \"value\"} ]} ]}");
            writer.AppendLine("Value6 = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3 ], {key = \"value\"} ]} ]}");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("[Value]");
            writer.AppendLine("key = [ 999, \"Value\", {key = [ [ 1, 2, 3 ], {key = \"value\"} ]} ]");
            writer.AppendLine("[Value2]");
            writer.AppendLine("key = [ 999, \"Value\", {key = [ [ 1, 2, 3 ], {key = \"value\"} ]} ]");
            writer.AppendLine("[Value3]");
            writer.AppendLine("key = [ 999, \"Value\", {key = [ [ 1, 2, 3 ], {key = \"value\"} ]} ]");
            writer.AppendLine("[Value4]");
            writer.AppendLine("key = [ 999, \"Value\", {key = [ [ 1, 2, 3 ], {key = \"value\"} ]} ]");
            writer.AppendLine("[Value5]");
            writer.AppendLine("key = [ 999, \"Value\", {key = [ [ 1, 2, 3 ], {key = \"value\"} ]} ]");
            writer.AppendLine("[Value6]");
            writer.AppendLine("key = [ 999, \"Value\", {key = [ [ 1, 2, 3 ], {key = \"value\"} ]} ]");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
    }

    [Fact]
    public void Deserialize()
    {
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\"}]}]}");
            writer.AppendLine("Value2 = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\"}]}]}");
            writer.AppendLine("Value3 = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\"}]}]}");
            writer.AppendLine("Value4 = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\"}]}]}");
            writer.AppendLine("Value5 = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\"}]}]}");
            writer.AppendLine("Value6 = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\"}]}]}");
            writer.Flush();

            var type = CsTomlSerializer.Deserialize<TypeDictionary2>(buffer.WrittenSpan);

            Validate(type.Value);
            Validate(type.Value2);
            Validate(type.Value3);
            Validate(type.Value4);
            Validate(type.Value5);
            Validate(type.Value6);
        }

        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("[Value]");
            writer.AppendLine("key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\"}]}]");
            writer.AppendLine("[Value2]");
            writer.AppendLine("key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\"}]}]");
            writer.AppendLine("[Value3]");
            writer.AppendLine("key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\"}]}]");
            writer.AppendLine("[Value4]");
            writer.AppendLine("key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\"}]}]");
            writer.AppendLine("[Value5]");
            writer.AppendLine("key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\"}]}]");
            writer.AppendLine("[Value6]");
            writer.AppendLine("key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\"}]}]");
            writer.Flush();

            var type = CsTomlSerializer.Deserialize<TypeDictionary2>(buffer.WrittenSpan);

            Validate(type.Value);
            Validate(type.Value2);
            Validate(type.Value3);
            Validate(type.Value4);
            Validate(type.Value5);
            Validate(type.Value6);
        }

        static void Validate(dynamic dynamicDict)
        {
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

public class TypeDictionaryTest3
{
    [Fact]
    public void Serialize()
    {
        var type = new TypeDictionary3()
        {
            Value = new Dictionary<long, string>()
            {
                [123] = "Value",
                [-1] = "Value2",
                [123456789] = "Value3",
            }
        };

        {
            using var bytes = CsTomlSerializer.Serialize(type);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = {123 = \"Value\", -1 = \"Value2\", 123456789 = \"Value3\"}");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("[Value]");
            writer.AppendLine("123 = \"Value\"");
            writer.AppendLine("-1 = \"Value2\"");
            writer.AppendLine("123456789 = \"Value3\"");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
    }

    [Fact]
    public void Deserialize()
    {
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = {123 = \"Value\", -1 = \"Value2\", 123456789 = \"Value3\" }");
            writer.Flush();

            var type = CsTomlSerializer.Deserialize<TypeDictionary3>(buffer.WrittenSpan);
            Validate(type.Value);
        }

        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("[Value]");
            writer.AppendLine("123 = \"Value\"");
            writer.AppendLine("-1 = \"Value2\"");
            writer.AppendLine("123456789 = \"Value3\"");
            writer.Flush();

            var type = CsTomlSerializer.Deserialize<TypeDictionary3>(buffer.WrittenSpan);
            Validate(type.Value);
        }

        static void Validate(dynamic dynamicDict)
        {
            string value = dynamicDict[123];
            value.ShouldBe("Value");
            string value2 = dynamicDict[-1];
            value2.ShouldBe("Value2");
            string value3 = dynamicDict[123456789];
            value3.ShouldBe("Value3");
        }
    }
}
