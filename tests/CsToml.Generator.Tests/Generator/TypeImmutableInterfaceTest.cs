using CsToml.Formatter.Resolver;
using CsToml.Generator.Other;
using Shouldly;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using Utf8StringInterpolation;

namespace CsToml.Generator.Tests;

public class TypeImmutableInterfaceTest
{
    public static IEnumerable<object[]> GetTypeImmutableInterfaces()
    {
        int[] array = [1, 2, 3, 4, 5];
        var set = new HashSet<int>(array);
        var queue = new Queue<int>(array);
        var immutableQueue = ImmutableQueue<int>.Empty;
        for (var i = queue.Count - 1; i >= 0; i--)
        {
            immutableQueue = immutableQueue.Enqueue(queue.Dequeue());
        }

        var dict = new Dictionary<string, object?>()
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
        };

        yield return new object[] { new TypeImmutableInterface()
        {
            IImmutableList = array.ToImmutableList(),
            IImmutableStack = [5, 4, 3, 2, 1],
            IImmutableSet = set.ToImmutableHashSet(),
            IImmutableQueue = immutableQueue,
            IImmutableDictionary = dict.ToImmutableDictionary(),
        }};
        yield return new object[] { new TypeImmutableInterface()
        {
            IImmutableList = array.ToImmutableArray(),
            IImmutableStack = [5, 4, 3, 2, 1],
            IImmutableSet = set.ToImmutableSortedSet(),
            IImmutableQueue = immutableQueue,
            IImmutableDictionary = dict.ToImmutableSortedDictionary(),
        }};
    }

    [Theory]
    [MemberData(nameof(GetTypeImmutableInterfaces))]
    public void Serialize(TypeImmutableInterface typeImmutableInterface)
    {
        using var bytes = CsTomlSerializer.Serialize(typeImmutableInterface);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("IImmutableList = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("IImmutableStack = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("IImmutableQueue = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("IImmutableSet = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("IImmutableDictionary = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3 ], {key = \"value\"} ]} ]}");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Theory]
    [MemberData(nameof(GetTypeImmutableInterfaces))]
    public void SerializeWithHeaderOption(TypeImmutableInterface typeImmutableInterface)
    {
        using var bytes = CsTomlSerializer.Serialize(typeImmutableInterface, Option.Header);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("IImmutableList = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("IImmutableStack = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("IImmutableQueue = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("IImmutableSet = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("[IImmutableDictionary]");
        writer.AppendLine("key = [ 999, \"Value\", {key = [ [ 1, 2, 3 ], {key = \"value\"} ]} ]");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Theory]
    [MemberData(nameof(GetTypeImmutableInterfaces))]
    public void SerializeWithArrayHeaderOption(TypeImmutableInterface typeImmutableInterface)
    {
        using var bytes = CsTomlSerializer.Serialize(typeImmutableInterface, Option.ArrayHeader);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("IImmutableList = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("IImmutableStack = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("IImmutableQueue = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("IImmutableSet = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("IImmutableDictionary = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3 ], {key = \"value\"} ]} ]}");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Theory]
    [MemberData(nameof(GetTypeImmutableInterfaces))]
    public void SerializeWithHeaderAndArrayHeaderOption(TypeImmutableInterface typeImmutableInterface)
    {
        using var bytes = CsTomlSerializer.Serialize(typeImmutableInterface, Option.HeaderAndArrayHeader);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("IImmutableList = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("IImmutableStack = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("IImmutableQueue = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("IImmutableSet = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("[IImmutableDictionary]");
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
            writer.AppendLine("IImmutableList = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("IImmutableStack = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("IImmutableQueue = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("IImmutableSet = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("IImmutableDictionary = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\" }] }] }");
            writer.Flush();

            var typeImmutableInterface = CsTomlSerializer.Deserialize<TypeImmutableInterface>(buffer.WrittenSpan);
            Validate(typeImmutableInterface);
        }
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("IImmutableList = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("IImmutableStack = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("IImmutableQueue = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("IImmutableSet = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("[IImmutableDictionary]");
            writer.AppendLine("key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\" }] }] ");
            writer.Flush();

            var typeImmutableInterface = CsTomlSerializer.Deserialize<TypeImmutableInterface>(buffer.WrittenSpan);
            Validate(typeImmutableInterface);
        }

        static void Validate(TypeImmutableInterface typeImmutableInterface)
        {
            typeImmutableInterface.IImmutableList.ShouldBe([1, 2, 3, 4, 5]);
            typeImmutableInterface.IImmutableStack.ShouldBe([5, 4, 3, 2, 1]);
            typeImmutableInterface.IImmutableQueue.ShouldBe([1, 2, 3, 4, 5]);
            typeImmutableInterface.IImmutableSet.ShouldBe([1, 2, 3, 4, 5]);

            dynamic dict = typeImmutableInterface.IImmutableDictionary;
            long value = dict["key"][0];
            value.ShouldBe(999);
            string value2 = dict["key"][1];
            value2.ShouldBe("Value");
            object[] value3 = dict["key"][2]["key"][0];
            value3.ShouldBe(new object[] { 1, 2, 3 });
            string value4 = dict["key"][2]["key"][1]["key"];
            value4.ShouldBe("value");
        }
    }
}
