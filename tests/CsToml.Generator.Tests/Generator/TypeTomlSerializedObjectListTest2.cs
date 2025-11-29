using Shouldly;
using Utf8StringInterpolation;
using CsToml.Generator.Other;

namespace CsToml.Generator.Tests;

public class TypeTomlSerializedObjectListTest2
{
    private TypeTomlSerializedObjectList2 typeTomlSerializedObjectList2;

    public TypeTomlSerializedObjectListTest2()
    {
        typeTomlSerializedObjectList2 = new TypeTomlSerializedObjectList2()
        {
            Value = 999,
            Table = [
            new TypeTable(){ Table2 = new TypeTable2() { Table3 = new TypeTable3() { Value = "[1] This is TypeTable3" } } },
            new TypeTable(){ Table2 = new TypeTable2() { Table3 = new TypeTable3() { Value = "[2] This is TypeTable3" } } },
            new TypeTable(){ Table2 = new TypeTable2() { Table3 = new TypeTable3() { Value = "[3] This is TypeTable3" } } },
            new TypeTable(){ Table2 = new TypeTable2() { Table3 = new TypeTable3() { Value = "[4] This is TypeTable3" } } },
            new TypeTable(){ Table2 = new TypeTable2() { Table3 = new TypeTable3() { Value = "[5] This is TypeTable3" } } }]
        };
    }

    [Fact]
    public void Serialize()
    {
        using var bytes = CsTomlSerializer.Serialize(typeTomlSerializedObjectList2);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = 999");
        writer.AppendLine("Table = [ {Table2.Table3.Value = \"[1] This is TypeTable3\"}, {Table2.Table3.Value = \"[2] This is TypeTable3\"}, {Table2.Table3.Value = \"[3] This is TypeTable3\"}, {Table2.Table3.Value = \"[4] This is TypeTable3\"}, {Table2.Table3.Value = \"[5] This is TypeTable3\"} ]");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(typeTomlSerializedObjectList2, Option.Header);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = 999");
        writer.AppendLine("Table = [ {Table2.Table3.Value = \"[1] This is TypeTable3\"}, {Table2.Table3.Value = \"[2] This is TypeTable3\"}, {Table2.Table3.Value = \"[3] This is TypeTable3\"}, {Table2.Table3.Value = \"[4] This is TypeTable3\"}, {Table2.Table3.Value = \"[5] This is TypeTable3\"} ]");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithArrayHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(typeTomlSerializedObjectList2, Option.ArrayHeader);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = 999");
        writer.AppendLine("[[Table]]");
        writer.AppendLine("Table2.Table3.Value = \"[1] This is TypeTable3\"");
        writer.AppendLine();
        writer.AppendLine("[[Table]]");
        writer.AppendLine("Table2.Table3.Value = \"[2] This is TypeTable3\"");
        writer.AppendLine();
        writer.AppendLine("[[Table]]");
        writer.AppendLine("Table2.Table3.Value = \"[3] This is TypeTable3\"");
        writer.AppendLine();
        writer.AppendLine("[[Table]]");
        writer.AppendLine("Table2.Table3.Value = \"[4] This is TypeTable3\"");
        writer.AppendLine();
        writer.AppendLine("[[Table]]");
        writer.AppendLine("Table2.Table3.Value = \"[5] This is TypeTable3\"");
        writer.AppendLine();
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithHeaderAndArrayHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(typeTomlSerializedObjectList2, Option.HeaderAndArrayHeader);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = 999");
        writer.AppendLine("[[Table]]");
        writer.AppendLine("Table2.Table3.Value = \"[1] This is TypeTable3\"");
        writer.AppendLine();
        writer.AppendLine("[[Table]]");
        writer.AppendLine("Table2.Table3.Value = \"[2] This is TypeTable3\"");
        writer.AppendLine();
        writer.AppendLine("[[Table]]");
        writer.AppendLine("Table2.Table3.Value = \"[3] This is TypeTable3\"");
        writer.AppendLine();
        writer.AppendLine("[[Table]]");
        writer.AppendLine("Table2.Table3.Value = \"[4] This is TypeTable3\"");
        writer.AppendLine();
        writer.AppendLine("[[Table]]");
        writer.AppendLine("Table2.Table3.Value = \"[5] This is TypeTable3\"");
        writer.AppendLine();
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void Deserialize()
    {
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = 999");
            writer.AppendLine("Table = [ {Table2.Table3.Value = \"[1] This is TypeTable3\"}, {Table2.Table3.Value = \"[2] This is TypeTable3\"}, {Table2.Table3.Value = \"[3] This is TypeTable3\"}, {Table2.Table3.Value = \"[4] This is TypeTable3\"}, {Table2.Table3.Value = \"[5] This is TypeTable3\"} ]");
            writer.Flush();
            var typeTomlSerializedObjectList2 = CsTomlSerializer.Deserialize<TypeTomlSerializedObjectList2>(buffer.WrittenSpan);
            Validate(typeTomlSerializedObjectList2);
        }
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = 999");
            writer.AppendLine("[[Table]]");
            writer.AppendLine("Table2.Table3.Value = \"[1] This is TypeTable3\"");
            writer.AppendLine();
            writer.AppendLine("[[Table]]");
            writer.AppendLine("Table2.Table3.Value = \"[2] This is TypeTable3\"");
            writer.AppendLine();
            writer.AppendLine("[[Table]]");
            writer.AppendLine("Table2.Table3.Value = \"[3] This is TypeTable3\"");
            writer.AppendLine();
            writer.AppendLine("[[Table]]");
            writer.AppendLine("Table2.Table3.Value = \"[4] This is TypeTable3\"");
            writer.AppendLine();
            writer.AppendLine("[[Table]]");
            writer.AppendLine("Table2.Table3.Value = \"[5] This is TypeTable3\"");
            writer.AppendLine();
            writer.Flush();
            var typeTomlSerializedObjectList2 = CsTomlSerializer.Deserialize<TypeTomlSerializedObjectList2>(buffer.WrittenSpan);
            Validate(typeTomlSerializedObjectList2);
        }

        static void Validate(TypeTomlSerializedObjectList2 typeTomlSerializedObjectList2)
        {
            typeTomlSerializedObjectList2.Table.Count.ShouldBe(5);
            typeTomlSerializedObjectList2.Table[0].Table2.Table3.Value.ShouldBe("[1] This is TypeTable3");
            typeTomlSerializedObjectList2.Table[1].Table2.Table3.Value.ShouldBe("[2] This is TypeTable3");
            typeTomlSerializedObjectList2.Table[2].Table2.Table3.Value.ShouldBe("[3] This is TypeTable3");
            typeTomlSerializedObjectList2.Table[3].Table2.Table3.Value.ShouldBe("[4] This is TypeTable3");
            typeTomlSerializedObjectList2.Table[4].Table2.Table3.Value.ShouldBe("[5] This is TypeTable3");
        }
    }
}
