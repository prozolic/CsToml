using CsToml.Formatter.Resolver;
using Shouldly;
using Utf8StringInterpolation;
using CsToml.Generator.Other;

namespace CsToml.Generator.Tests;

public class DictionaryTest
{
    [Fact]
    public void Serialize()
    {
        var dict = new Dictionary<object, object?>()
        {
            ["key"] = new object[]
            {
                999,
                "Value",
                Color.Red,
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
            },
            ["Table"] = new Dictionary<object, object>()
            {
                [1] = "2",
                [3] = "4"
            },
            ["Array"] = new object[] { 123, 456.0f, "789" },
            ["TableParent"] = new Dictionary<object, object>()
            {
                ["Table3"] = new Dictionary<object, object>()
                {
                    [1] = new Dictionary<string, object?>()
                    {
                        ["key"] = new object[]
                        {
                            new long[] {1, 2, 3},
                            new Dictionary<string, object?>()
                            {
                                ["key"] = "value"
                            }
                        }
                    },
                    [2] = new Dictionary<string, object?>()
                    {
                        ["key"] = "value"
                    }

                }
            }
        };

        {
            using var bytes = CsTomlSerializer.Serialize(dict);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("key = [ 999, \"Value\", \"Red\", {key = [ [ 1, 2, 3 ], {key = \"value\"} ]} ]");
            writer.AppendLine("Table = {1 = \"2\", 3 = \"4\"}");
            writer.AppendLine("Array = [ 123, 456.0, \"789\" ]");
            writer.AppendLine("TableParent = {Table3 = {1 = {key = [ [ 1, 2, 3 ], {key = \"value\"} ]}, 2 = {key = \"value\"}}}");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(dict, options: Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("key = [ 999, \"Value\", \"Red\", {key = [ [ 1, 2, 3 ], {key = \"value\"} ]} ]");
            writer.AppendLine("Array = [ 123, 456.0, \"789\" ]");
            writer.AppendLine("[Table]");
            writer.AppendLine("1 = \"2\"");
            writer.AppendLine("3 = \"4\"");
            writer.AppendLine("[TableParent]");
            writer.AppendLine("[TableParent.Table3]");
            writer.AppendLine("[TableParent.Table3.1]");
            writer.AppendLine("key = [ [ 1, 2, 3 ], {key = \"value\"} ]");
            writer.AppendLine("[TableParent.Table3.2]");
            writer.AppendLine("key = \"value\"");
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
            writer.AppendLine("key = [ 999, \"Value\", \"Red\", {key = [ [ 1, 2, 3 ], {key = \"value\"} ]} ]");
            writer.AppendLine("Table = {1 = \"2\", 3 = \"4\"}");
            writer.AppendLine("Array = [ 123, 456.0, \"789\" ]");
            writer.AppendLine("TableParent = {Table3 = {1 = {key = [ [ 1, 2, 3 ], {key = \"value\"} ]}, 2 = {key = \"value\"}}}");
            writer.Flush();

            var dict = CsTomlSerializer.Deserialize<IDictionary<object, object?>>(buffer.WrittenSpan);
            Validate(dict);
        }
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("key = [ 999, \"Value\", \"Red\", {key = [ [ 1, 2, 3 ], {key = \"value\"} ]} ]");
            writer.AppendLine("Array = [ 123, 456.0, \"789\" ]");
            writer.AppendLine("[Table]");
            writer.AppendLine("1 = \"2\"");
            writer.AppendLine("3 = \"4\"");
            writer.AppendLine("[TableParent]");
            writer.AppendLine("[TableParent.Table3]");
            writer.AppendLine("[TableParent.Table3.1]");
            writer.AppendLine("key = [ [ 1, 2, 3 ], {key = \"value\"} ]");
            writer.AppendLine("[TableParent.Table3.2]");
            writer.AppendLine("key = \"value\"");
            writer.Flush();

            var dict = CsTomlSerializer.Deserialize<IDictionary<object, object?>>(buffer.WrittenSpan);
            Validate(dict);
        }

        static void Validate(IDictionary<object, object?> typeOrderedDictionary)
        {
            dynamic dynamicDict = typeOrderedDictionary;
            long value = dynamicDict["key"][0];
            value.ShouldBe(999);
            string value2 = dynamicDict["key"][1];
            value2.ShouldBe("Value");
            string value3 = dynamicDict["key"][2];
            value3.ShouldBe("Red");
            object[] value4 = dynamicDict["key"][3]["key"][0];
            value4.ShouldBe(new object[] { 1, 2, 3 });
            string value5 = dynamicDict["key"][3]["key"][1]["key"];
            value5.ShouldBe("value");
            string value6 = dynamicDict["Table"]["1"];
            value6.ShouldBe("2");
            string value7 = dynamicDict["Table"]["3"];
            value7.ShouldBe("4");
            long value8 = dynamicDict["Array"][0];
            value8.ShouldBe(123);
            double value9 = dynamicDict["Array"][1];
            value9.ShouldBe(456.0f);
            string value10 = dynamicDict["Array"][2];
            value10.ShouldBe("789");
            object[] value11 = dynamicDict["TableParent"]["Table3"]["1"]["key"][0];
            value11.ShouldBe(new object[] { 1, 2, 3 });
            string value12 = dynamicDict["TableParent"]["Table3"]["1"]["key"][1]["key"];
            value12.ShouldBe("value");
            string value13 = dynamicDict["TableParent"]["Table3"]["2"]["key"];
            value13.ShouldBe("value");
        }
    }
}
