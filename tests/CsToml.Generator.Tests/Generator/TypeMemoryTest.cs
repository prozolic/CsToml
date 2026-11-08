using Shouldly;
using Utf8StringInterpolation;

namespace CsToml.Generator.Tests;

public class TypeMemoryTest
{
    [Test]
    public void Serialize()
    {
        var type = new TypeMemory() { Memory = new byte[] { 1, 2, 3 } };

        using var bytes = CsTomlSerializer.Serialize(type);
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Memory = [ 1, 2, 3 ]");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Test]
    public void SerializeSlice()
    {
        var type = new TypeMemory() { Memory = new byte[] { 1, 2, 3, 4, 5 }.AsMemory(1, 3) };

        using var bytes = CsTomlSerializer.Serialize(type);
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Memory = [ 2, 3, 4 ]");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Test]
    public void Deserialize()
    {
        var type = CsTomlSerializer.Deserialize<TypeMemory>("Memory = [ 1, 2, 3 ]"u8);
        type.Memory.ToArray().ShouldBe(new byte[] { 1, 2, 3 });
    }
}

public class TypeNestedArrayTest
{
    private TypeNestedArray type;

    public TypeNestedArrayTest()
    {
        TypeNestedArrayItem[] items = [new() { Name = "a" }, new() { Name = "b" }];
        type = new TypeNestedArray()
        {
            Items = items,
            MemoryItems = items,
            Child = new TypeNestedArrayChild() { Items = items, MemoryItems = items },
        };
    }

    [Test]
    public void Serialize()
    {
        using var bytes = CsTomlSerializer.Serialize(type);
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Items = [ {Name = \"a\"}, {Name = \"b\"} ]");
        writer.AppendLine("MemoryItems = [ {Name = \"a\"}, {Name = \"b\"} ]");
        writer.AppendLine("Child = {Items = [ {Name = \"a\"}, {Name = \"b\"} ], MemoryItems = [ {Name = \"a\"}, {Name = \"b\"} ]}");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Test]
    public void SerializeWithArrayHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(type, Option.ArrayHeader);
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Child = {Items = [ {Name = \"a\"}, {Name = \"b\"} ], MemoryItems = [ {Name = \"a\"}, {Name = \"b\"} ]}");
        writer.AppendLine("[[Items]]");
        writer.AppendLine("Name = \"a\"");
        writer.AppendLine();
        writer.AppendLine("[[Items]]");
        writer.AppendLine("Name = \"b\"");
        writer.AppendLine();
        writer.AppendLine("[[MemoryItems]]");
        writer.AppendLine("Name = \"a\"");
        writer.AppendLine();
        writer.AppendLine("[[MemoryItems]]");
        writer.AppendLine("Name = \"b\"");
        writer.AppendLine();
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Test]
    public void SerializeWithHeaderAndArrayHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(type, Option.HeaderAndArrayHeader);
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("[[Items]]");
        writer.AppendLine("Name = \"a\"");
        writer.AppendLine();
        writer.AppendLine("[[Items]]");
        writer.AppendLine("Name = \"b\"");
        writer.AppendLine();
        writer.AppendLine("[[MemoryItems]]");
        writer.AppendLine("Name = \"a\"");
        writer.AppendLine();
        writer.AppendLine("[[MemoryItems]]");
        writer.AppendLine("Name = \"b\"");
        writer.AppendLine();
        writer.AppendLine("[Child]");
        writer.AppendLine("Items = [ {Name = \"a\"}, {Name = \"b\"} ]");
        writer.AppendLine("MemoryItems = [ {Name = \"a\"}, {Name = \"b\"} ]");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }
}
