using CsToml.Generator.Other;
using Shouldly;
using System.Collections.Frozen;
using Utf8StringInterpolation;

namespace CsToml.Generator.Tests;

public class TypeTomlSerializedObjectListTest
{
    private TypeTomlSerializedObjectList typeTomlSerializedObjectList;

    public TypeTomlSerializedObjectListTest()
    {
        typeTomlSerializedObjectList = new TypeTomlSerializedObjectList()
        {
            Table2 = [
            new TypeTable2() { Table3 = new TypeTable3() { Value = "[1] This is TypeTable3" } },
            new TypeTable2() { Table3 = new TypeTable3() { Value = "[2] This is TypeTable3" } },
            new TypeTable2() { Table3 = new TypeTable3() { Value = "[3] This is TypeTable3" } },
            new TypeTable2() { Table3 = new TypeTable3() { Value = "[4] This is TypeTable3" } },
            new TypeTable2() { Table3 = new TypeTable3() { Value = "[5] This is TypeTable3" } }]
        };
    }

    [Fact]
    public void Serialize()
    {
        using var bytes = CsTomlSerializer.Serialize(typeTomlSerializedObjectList);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Table2 = [ {Table3.Value = \"[1] This is TypeTable3\"}, {Table3.Value = \"[2] This is TypeTable3\"}, {Table3.Value = \"[3] This is TypeTable3\"}, {Table3.Value = \"[4] This is TypeTable3\"}, {Table3.Value = \"[5] This is TypeTable3\"} ]");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(typeTomlSerializedObjectList, Option.Header);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Table2 = [ {Table3.Value = \"[1] This is TypeTable3\"}, {Table3.Value = \"[2] This is TypeTable3\"}, {Table3.Value = \"[3] This is TypeTable3\"}, {Table3.Value = \"[4] This is TypeTable3\"}, {Table3.Value = \"[5] This is TypeTable3\"} ]");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithArrayHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(typeTomlSerializedObjectList, Option.ArrayHeader);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("[[Table2]]");
        writer.AppendLine("Table3.Value = \"[1] This is TypeTable3\"");
        writer.AppendLine();
        writer.AppendLine("[[Table2]]");
        writer.AppendLine("Table3.Value = \"[2] This is TypeTable3\"");
        writer.AppendLine();
        writer.AppendLine("[[Table2]]");
        writer.AppendLine("Table3.Value = \"[3] This is TypeTable3\"");
        writer.AppendLine();
        writer.AppendLine("[[Table2]]");
        writer.AppendLine("Table3.Value = \"[4] This is TypeTable3\"");
        writer.AppendLine();
        writer.AppendLine("[[Table2]]");
        writer.AppendLine("Table3.Value = \"[5] This is TypeTable3\"");
        writer.AppendLine();
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithHeaderAndArrayHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(typeTomlSerializedObjectList, Option.HeaderAndArrayHeader);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("[[Table2]]");
        writer.AppendLine("Table3.Value = \"[1] This is TypeTable3\"");
        writer.AppendLine();
        writer.AppendLine("[[Table2]]");
        writer.AppendLine("Table3.Value = \"[2] This is TypeTable3\"");
        writer.AppendLine();
        writer.AppendLine("[[Table2]]");
        writer.AppendLine("Table3.Value = \"[3] This is TypeTable3\"");
        writer.AppendLine();
        writer.AppendLine("[[Table2]]");
        writer.AppendLine("Table3.Value = \"[4] This is TypeTable3\"");
        writer.AppendLine();
        writer.AppendLine("[[Table2]]");
        writer.AppendLine("Table3.Value = \"[5] This is TypeTable3\"");
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
            writer.AppendLine("Table2 = [ {Table3.Value = \"[1] This is TypeTable3\"}, {Table3.Value = \"[2] This is TypeTable3\"}, {Table3.Value = \"[3] This is TypeTable3\"}, {Table3.Value = \"[4] This is TypeTable3\"}, {Table3.Value = \"[5] This is TypeTable3\"} ]");
            writer.Flush();

            var typeTomlSerializedObjectList = CsTomlSerializer.Deserialize<TypeTomlSerializedObjectList>(buffer.WrittenSpan);
            Validate(typeTomlSerializedObjectList);
        }
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("[[Table2]]");
            writer.AppendLine("Table3.Value = \"[1] This is TypeTable3\"");
            writer.AppendLine();
            writer.AppendLine("[[Table2]]");
            writer.AppendLine("Table3.Value = \"[2] This is TypeTable3\"");
            writer.AppendLine();
            writer.AppendLine("[[Table2]]");
            writer.AppendLine("Table3.Value = \"[3] This is TypeTable3\"");
            writer.AppendLine();
            writer.AppendLine("[[Table2]]");
            writer.AppendLine("Table3.Value = \"[4] This is TypeTable3\"");
            writer.AppendLine();
            writer.AppendLine("[[Table2]]");
            writer.AppendLine("Table3.Value = \"[5] This is TypeTable3\"");
            writer.AppendLine();
            writer.Flush();

            var typeTomlSerializedObjectList = CsTomlSerializer.Deserialize<TypeTomlSerializedObjectList>(buffer.WrittenSpan);
            Validate(typeTomlSerializedObjectList);
        }

        static void Validate(TypeTomlSerializedObjectList typeTomlSerializedObjectList)
        {
            typeTomlSerializedObjectList.Table2.Count.ShouldBe(5);
            typeTomlSerializedObjectList.Table2[0].Table3.Value.ShouldBe("[1] This is TypeTable3");
            typeTomlSerializedObjectList.Table2[1].Table3.Value.ShouldBe("[2] This is TypeTable3");
            typeTomlSerializedObjectList.Table2[2].Table3.Value.ShouldBe("[3] This is TypeTable3");
            typeTomlSerializedObjectList.Table2[3].Table3.Value.ShouldBe("[4] This is TypeTable3");
            typeTomlSerializedObjectList.Table2[4].Table3.Value.ShouldBe("[5] This is TypeTable3");
        }
    }
}
