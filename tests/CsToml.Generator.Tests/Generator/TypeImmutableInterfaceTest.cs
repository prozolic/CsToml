using CsToml.Formatter.Resolver;
using Shouldly;
using System.Collections.Immutable;
using Utf8StringInterpolation;
using CsToml.Generator.Other;

namespace CsToml.Generator.Tests;

public class TypeImmutableInterfaceTest
{

    [Fact]
    public void Serialize()
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

        var type = new TypeImmutableInterface()
        {
            IImmutableList = array.ToImmutableList(),
            IImmutableStack = [5, 4, 3, 2, 1],
            IImmutableSet = set.ToImmutableHashSet(),
            IImmutableQueue = immutableQueue,
            IImmutableDictionary = dict.ToImmutableDictionary(),
        };

        {
            using var bytes = CsTomlSerializer.Serialize(type);

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
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

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
    }

    [Fact]
    public void Serialize2()
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

        var type = new TypeImmutableInterface()
        {
            IImmutableList = array.ToImmutableArray(),
            IImmutableStack = [5, 4, 3, 2, 1],
            IImmutableSet = set.ToImmutableSortedSet(),
            IImmutableQueue = immutableQueue,
            IImmutableDictionary = dict.ToImmutableSortedDictionary(),
        };

        {
            using var bytes = CsTomlSerializer.Serialize(type);

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
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

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
            typeImmutableInterface.IImmutableList.ShouldBe([1, 2, 3, 4, 5]);
            typeImmutableInterface.IImmutableStack.ShouldBe([5, 4, 3, 2, 1]);
            typeImmutableInterface.IImmutableQueue.ShouldBe([1, 2, 3, 4, 5]);
            typeImmutableInterface.IImmutableSet.ShouldBe([1, 2, 3, 4, 5]);
            Validate(typeImmutableInterface.IImmutableDictionary);
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
            typeImmutableInterface.IImmutableList.ShouldBe([1, 2, 3, 4, 5]);
            typeImmutableInterface.IImmutableStack.ShouldBe([5, 4, 3, 2, 1]);
            typeImmutableInterface.IImmutableQueue.ShouldBe([1, 2, 3, 4, 5]);
            typeImmutableInterface.IImmutableSet.ShouldBe([1, 2, 3, 4, 5]);
            Validate(typeImmutableInterface.IImmutableDictionary);

        }

        static void Validate(dynamic dict)
        {
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
