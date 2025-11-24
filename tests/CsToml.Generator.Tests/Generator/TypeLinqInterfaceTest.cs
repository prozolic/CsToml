using CsToml.Generator.Other;
using Shouldly;
using Utf8StringInterpolation;

namespace CsToml.Generator.Tests;

public class TypeLinqInterfaceTest
{

    [Fact]
    public void Serialize()
    {
        var dict = new Dictionary<int, string>()
        {
            [1] = "1",
            [2] = "1",
            [3] = "3",
        };
        var dict2 = new Dictionary<int, TestStruct>()
        {
            [1] = new() { Value = 123, Str = "123" },
            [2] = new() { Value = 123, Str = "123" },
            [3] = new() { Value = 789, Str = "789" },
        };
        var typeLinqInterface = new TypeLinqInterface()
        {
            Lookup = dict.ToLookup(p => p.Value),
            Lookup2 = dict2.ToLookup(p => p.Value.Str),
            Grouping = dict.GroupBy(p => p.Value).First(),
            Grouping2 = dict2.ToLookup(p => p.Value.Str).First(),
        };

        using var bytes = CsTomlSerializer.Serialize(typeLinqInterface);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Lookup = [ {\"1\" = [ [ 1, \"1\" ], [ 2, \"1\" ] ]}, {\"3\" = [ [ 3, \"3\" ] ]} ]");
        writer.AppendLine("Lookup2 = [ {\"123\" = [ [ 1, {Value = 123, Str = \"123\"} ], [ 2, {Value = 123, Str = \"123\"} ] ]}, {\"789\" = [ [ 3, {Value = 789, Str = \"789\"} ] ]} ]");
        writer.AppendLine("Grouping = {\"1\" = [ [ 1, \"1\" ], [ 2, \"1\" ] ]}");
        writer.AppendLine("Grouping2 = {\"123\" = [ [ 1, {Value = 123, Str = \"123\"} ], [ 2, {Value = 123, Str = \"123\"} ] ]}");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Lookup = [ {\"1\" = [ [ 1, \"1\" ], [ 2, \"1\" ] ]}, {\"3\" = [ [ 3, \"3\" ] ]} ]");
        writer.AppendLine("Lookup2 = [ {\"123\" = [ [ 1, {Value = 123, Str = \"123\"} ], [ 2, {Value = 123, Str = \"123\"} ] ]}, {\"789\" = [ [ 3, {Value = 789, Str = \"789\"} ] ]} ]");
        writer.AppendLine("Grouping = {\"1\" = [ [ 1, \"1\" ], [ 2, \"1\" ] ]}");
        writer.AppendLine("Grouping2 = {\"123\" = [ [ 1, {Value = 123, Str = \"123\"} ], [ 2, {Value = 123, Str = \"123\"} ] ]}");
        writer.Flush();

        var dict = new Dictionary<int, string>()
        {
            [1] = "1",
            [2] = "1",
            [3] = "3",
        };
        var dict2 = new Dictionary<int, TestStruct>()
        {
            [1] = new() { Value = 123, Str = "123" },
            [2] = new() { Value = 123, Str = "123" },
            [3] = new() { Value = 789, Str = "789" },
        };

        var type = CsTomlSerializer.Deserialize<TypeLinqInterface>(buffer.WrittenSpan);
        type.Lookup.ShouldBe(dict.ToLookup(p => p.Value));
        type.Lookup2.ShouldBe(dict2.ToLookup(p => p.Value.Str));
        type.Grouping.ShouldBe(dict.GroupBy(p => p.Value).First());
        type.Grouping2.ShouldBe(dict2.ToLookup(p => p.Value.Str).First());
    }
}
