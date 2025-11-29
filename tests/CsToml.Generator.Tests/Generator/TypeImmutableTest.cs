using CsToml.Formatter.Resolver;
using Shouldly;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Utf8StringInterpolation;
using CsToml.Generator.Other;

namespace CsToml.Generator.Tests;

public class TypeImmutableTest
{
    private TypeImmutable typeImmutable;

    public TypeImmutableTest()
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

        typeImmutable = new TypeImmutable()
        {
            ImmutableArray = ImmutableCollectionsMarshal.AsImmutableArray(array),
            ImmutableList = array.ToImmutableList(),
            ImmutableStack = [5, 4, 3, 2, 1],
            ImmutableHashSet = set.ToImmutableHashSet(),
            ImmutableSortedSet = set.ToImmutableSortedSet(),
            ImmutableQueue = immutableQueue,
            ImmutableDictionary = dict.ToImmutableDictionary(),
            ImmutableSortedDictionary = dict.ToImmutableSortedDictionary(),
        };
    }

    [Fact]
    public void Serialize()
    {
        using var bytes = CsTomlSerializer.Serialize(typeImmutable);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("ImmutableArray = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("ImmutableList = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("ImmutableStack = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("ImmutableHashSet = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("ImmutableSortedSet = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("ImmutableQueue = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("ImmutableDictionary = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3 ], {key = \"value\"} ]} ]}");
        writer.AppendLine("ImmutableSortedDictionary = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3 ], {key = \"value\"} ]} ]}");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(typeImmutable, Option.Header);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("ImmutableArray = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("ImmutableList = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("ImmutableStack = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("ImmutableHashSet = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("ImmutableSortedSet = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("ImmutableQueue = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("[ImmutableDictionary]");
        writer.AppendLine("key = [ 999, \"Value\", {key = [ [ 1, 2, 3 ], {key = \"value\"} ]} ]");
        writer.AppendLine("[ImmutableSortedDictionary]");
        writer.AppendLine("key = [ 999, \"Value\", {key = [ [ 1, 2, 3 ], {key = \"value\"} ]} ]");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithArrayHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(typeImmutable, Option.ArrayHeader);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("ImmutableArray = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("ImmutableList = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("ImmutableStack = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("ImmutableHashSet = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("ImmutableSortedSet = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("ImmutableQueue = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("ImmutableDictionary = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3 ], {key = \"value\"} ]} ]}");
        writer.AppendLine("ImmutableSortedDictionary = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3 ], {key = \"value\"} ]} ]}");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithHeaderAndArrayHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(typeImmutable, Option.HeaderAndArrayHeader);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("ImmutableArray = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("ImmutableList = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("ImmutableStack = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("ImmutableHashSet = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("ImmutableSortedSet = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("ImmutableQueue = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("[ImmutableDictionary]");
        writer.AppendLine("key = [ 999, \"Value\", {key = [ [ 1, 2, 3 ], {key = \"value\"} ]} ]");
        writer.AppendLine("[ImmutableSortedDictionary]");
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
            writer.AppendLine("ImmutableArray = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("ImmutableList = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("ImmutableStack = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("ImmutableHashSet = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("ImmutableSortedSet = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("ImmutableQueue = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("ImmutableDictionary = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\" }] }] }");
            writer.AppendLine("ImmutableSortedDictionary = {key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\" }] }] }");
            writer.Flush();

            var typeImmutable = CsTomlSerializer.Deserialize<TypeImmutable>(buffer.WrittenSpan);
            Validate(typeImmutable);
        }
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("ImmutableArray = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("ImmutableList = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("ImmutableStack = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("ImmutableHashSet = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("ImmutableSortedSet = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("ImmutableQueue = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("[ImmutableDictionary]");
            writer.AppendLine("key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\" }] }] ");
            writer.AppendLine("[ImmutableSortedDictionary]");
            writer.AppendLine("key = [ 999, \"Value\", {key = [ [ 1, 2, 3], {key = \"value\" }] }] ");
            writer.Flush();

            var typeImmutable = CsTomlSerializer.Deserialize<TypeImmutable>(buffer.WrittenSpan);
            Validate(typeImmutable);
        }

        static void Validate(TypeImmutable typeImmutable)
        {
            typeImmutable.ImmutableArray.ShouldBe([1, 2, 3, 4, 5]);
            typeImmutable.ImmutableList.ShouldBe([1, 2, 3, 4, 5]);
            typeImmutable.ImmutableStack.ShouldBe([5, 4, 3, 2, 1]);
            typeImmutable.ImmutableHashSet.ShouldBe([1, 2, 3, 4, 5]);
            typeImmutable.ImmutableSortedSet.ShouldBe([1, 2, 3, 4, 5]);

            {
                dynamic dict = typeImmutable.ImmutableDictionary;
                long value = dict["key"][0];
                value.ShouldBe(999);
                string value2 = dict["key"][1];
                value2.ShouldBe("Value");
                object[] value3 = dict["key"][2]["key"][0];
                value3.ShouldBe(new object[] { 1, 2, 3 });
                string value4 = dict["key"][2]["key"][1]["key"];
                value4.ShouldBe("value");
            }
            {
                dynamic dict = typeImmutable.ImmutableSortedDictionary;
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
}

public class TypeImmutableTest2
{
    [Fact]
    public void Serialize()
    {
        var type = new TypeImmutable2()
        {
            ImmutableArray = [new TypeTable3() { Value = "[1] This is TypeTable3 in ImmutableArray" },
                              new TypeTable3() { Value = "[2] This is TypeTable3 in ImmutableArray" },
                              new TypeTable3() { Value = "[3] This is TypeTable3 in ImmutableArray" }],
            NullableImmutableArray = [new TypeTable3() { Value = "[1] This is TypeTable3 in NullableImmutableArray" },
                                      new TypeTable3() { Value = "[2] This is TypeTable3 in NullableImmutableArray" },
                                      new TypeTable3() { Value = "[3] This is TypeTable3 in NullableImmutableArray" }],
            ImmutableList = [new TypeTable3() { Value = "[1] This is TypeTable3 in ImmutableList" },
                              new TypeTable3() { Value = "[2] This is TypeTable3 in ImmutableList" },
                              new TypeTable3() { Value = "[3] This is TypeTable3 in ImmutableList" }],
            IImmutableList = [new TypeTable3() { Value = "[1] This is TypeTable3 in IImmutableList" },
                              new TypeTable3() { Value = "[2] This is TypeTable3 in IImmutableList" },
                              new TypeTable3() { Value = "[3] This is TypeTable3 in IImmutableList" }],
        };

        {
            using var bytes = CsTomlSerializer.Serialize(type);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("ImmutableArray = [ {Value = \"[1] This is TypeTable3 in ImmutableArray\"}, {Value = \"[2] This is TypeTable3 in ImmutableArray\"}, {Value = \"[3] This is TypeTable3 in ImmutableArray\"} ]");
            writer.AppendLine("NullableImmutableArray = [ {Value = \"[1] This is TypeTable3 in NullableImmutableArray\"}, {Value = \"[2] This is TypeTable3 in NullableImmutableArray\"}, {Value = \"[3] This is TypeTable3 in NullableImmutableArray\"} ]");
            writer.AppendLine("ImmutableList = [ {Value = \"[1] This is TypeTable3 in ImmutableList\"}, {Value = \"[2] This is TypeTable3 in ImmutableList\"}, {Value = \"[3] This is TypeTable3 in ImmutableList\"} ]");
            writer.AppendLine("IImmutableList = [ {Value = \"[1] This is TypeTable3 in IImmutableList\"}, {Value = \"[2] This is TypeTable3 in IImmutableList\"}, {Value = \"[3] This is TypeTable3 in IImmutableList\"} ]");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("ImmutableArray = [ {Value = \"[1] This is TypeTable3 in ImmutableArray\"}, {Value = \"[2] This is TypeTable3 in ImmutableArray\"}, {Value = \"[3] This is TypeTable3 in ImmutableArray\"} ]");
            writer.AppendLine("NullableImmutableArray = [ {Value = \"[1] This is TypeTable3 in NullableImmutableArray\"}, {Value = \"[2] This is TypeTable3 in NullableImmutableArray\"}, {Value = \"[3] This is TypeTable3 in NullableImmutableArray\"} ]");
            writer.AppendLine("ImmutableList = [ {Value = \"[1] This is TypeTable3 in ImmutableList\"}, {Value = \"[2] This is TypeTable3 in ImmutableList\"}, {Value = \"[3] This is TypeTable3 in ImmutableList\"} ]");
            writer.AppendLine("IImmutableList = [ {Value = \"[1] This is TypeTable3 in IImmutableList\"}, {Value = \"[2] This is TypeTable3 in IImmutableList\"}, {Value = \"[3] This is TypeTable3 in IImmutableList\"} ]");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
    }

    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("ImmutableArray = [ {Value = \"[1] This is TypeTable3 in ImmutableArray\"}, {Value = \"[2] This is TypeTable3 in ImmutableArray\"}, {Value = \"[3] This is TypeTable3 in ImmutableArray\"} ]");
        writer.AppendLine("ImmutableList = [ {Value = \"[1] This is TypeTable3 in ImmutableList\"}, {Value = \"[2] This is TypeTable3 in ImmutableList\"}, {Value = \"[3] This is TypeTable3 in ImmutableList\"} ]");
        writer.AppendLine("IImmutableList = [ {Value = \"[1] This is TypeTable3 in IImmutableList\"}, {Value = \"[2] This is TypeTable3 in IImmutableList\"}, {Value = \"[3] This is TypeTable3 in IImmutableList\"} ]");
        writer.AppendLine("NullableImmutableArray = [ {Value = \"[1] This is TypeTable3 in NullableImmutableArray\"}, {Value = \"[2] This is TypeTable3 in NullableImmutableArray\"}, {Value = \"[3] This is TypeTable3 in NullableImmutableArray\"} ]");
        writer.Flush();

        var typeImmutable2 = CsTomlSerializer.Deserialize<TypeImmutable2>(buffer.WrittenSpan);

        typeImmutable2.IImmutableList.Count.ShouldBe(3);
        typeImmutable2.IImmutableList[0].Value.ShouldBe("[1] This is TypeTable3 in IImmutableList");
        typeImmutable2.IImmutableList[1].Value.ShouldBe("[2] This is TypeTable3 in IImmutableList");
        typeImmutable2.IImmutableList[2].Value.ShouldBe("[3] This is TypeTable3 in IImmutableList");

        typeImmutable2.ImmutableList.Count.ShouldBe(3);
        typeImmutable2.ImmutableList[0].Value.ShouldBe("[1] This is TypeTable3 in ImmutableList");
        typeImmutable2.ImmutableList[1].Value.ShouldBe("[2] This is TypeTable3 in ImmutableList");
        typeImmutable2.ImmutableList[2].Value.ShouldBe("[3] This is TypeTable3 in ImmutableList");

        typeImmutable2.ImmutableArray.Length.ShouldBe(3);
        typeImmutable2.ImmutableArray[0].Value.ShouldBe("[1] This is TypeTable3 in ImmutableArray");
        typeImmutable2.ImmutableArray[1].Value.ShouldBe("[2] This is TypeTable3 in ImmutableArray");
        typeImmutable2.ImmutableArray[2].Value.ShouldBe("[3] This is TypeTable3 in ImmutableArray");

        typeImmutable2.NullableImmutableArray!.Value.Length.ShouldBe(3);
        typeImmutable2.NullableImmutableArray.Value[0].Value.ShouldBe("[1] This is TypeTable3 in NullableImmutableArray");
        typeImmutable2.NullableImmutableArray.Value[1].Value.ShouldBe("[2] This is TypeTable3 in NullableImmutableArray");
        typeImmutable2.NullableImmutableArray.Value[2].Value.ShouldBe("[3] This is TypeTable3 in NullableImmutableArray");
    }
}
