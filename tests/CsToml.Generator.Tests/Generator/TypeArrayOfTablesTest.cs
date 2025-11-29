using Shouldly;
using System.Collections.Immutable;
using Utf8StringInterpolation;
using CsToml.Generator.Other;

namespace CsToml.Generator.Tests;

public class TypeArrayOfTablesTest
{
    private TypeArrayOfTable type;

    public TypeArrayOfTablesTest()
    {
        type = new TypeArrayOfTable()
        {
            Header = new TypeTomlSerializedObjectList()
            {
                Table2 = [
                    new TypeTable2(){ Table3 = new TypeTable3() { Value = "[1] This is TypeTable3" } },
                    new TypeTable2(){ Table3 = new TypeTable3() { Value = "[2] This is TypeTable3" } },
                    new TypeTable2(){ Table3 = new TypeTable3() { Value = "[3] This is TypeTable3" } }
                ],
            }
        };
    }

    [Fact]
    public void Serialize()
    {
        using var bytes = CsTomlSerializer.Serialize(type);
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Header.Table2 = [ {Table3.Value = \"[1] This is TypeTable3\"}, {Table3.Value = \"[2] This is TypeTable3\"}, {Table3.Value = \"[3] This is TypeTable3\"} ]");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(type, Option.Header);
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("[Header]");
        writer.AppendLine("Table2 = [ {Table3.Value = \"[1] This is TypeTable3\"}, {Table3.Value = \"[2] This is TypeTable3\"}, {Table3.Value = \"[3] This is TypeTable3\"} ]");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithArrayHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(type, Option.ArrayHeader);
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("[[Header.Table2]]");
        writer.AppendLine("Table3.Value = \"[1] This is TypeTable3\"");
        writer.AppendLine();
        writer.AppendLine("[[Header.Table2]]");
        writer.AppendLine("Table3.Value = \"[2] This is TypeTable3\"");
        writer.AppendLine();
        writer.AppendLine("[[Header.Table2]]");
        writer.AppendLine("Table3.Value = \"[3] This is TypeTable3\"");
        writer.AppendLine();
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithHeaderAndArrayHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(type, Option.HeaderAndArrayHeader);
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("[Header]");
        writer.AppendLine("Table2 = [ {Table3.Value = \"[1] This is TypeTable3\"}, {Table3.Value = \"[2] This is TypeTable3\"}, {Table3.Value = \"[3] This is TypeTable3\"} ]");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void Deserialize()
    {
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("[[Header.Table2]]");
            writer.AppendLine("Table3.Value = \"[1] This is TypeTable3\"");
            writer.AppendLine();
            writer.AppendLine("[[Header.Table2]]");
            writer.AppendLine("Table3.Value = \"[2] This is TypeTable3\"");
            writer.AppendLine();
            writer.AppendLine("[[Header.Table2]]");
            writer.AppendLine("Table3.Value = \"[3] This is TypeTable3\"");
            writer.Flush();

            var typeArrayOfTable = CsTomlSerializer.Deserialize<TypeArrayOfTable>(buffer.WrittenSpan);
            Validate(typeArrayOfTable);
        }
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("[[Header.Table2]]");
            writer.AppendLine("Table3.Value = \"[1] This is TypeTable3\"");
            writer.AppendLine();
            writer.AppendLine("[[Header.Table2]]");
            writer.AppendLine("Table3.Value = \"[2] This is TypeTable3\"");
            writer.AppendLine();
            writer.AppendLine("[[Header.Table2]]");
            writer.AppendLine("Table3.Value = \"[3] This is TypeTable3\"");
            writer.AppendLine();
            writer.Flush();

            var typeArrayOfTable = CsTomlSerializer.Deserialize<TypeArrayOfTable>(buffer.WrittenSpan);
            Validate(typeArrayOfTable);
        }
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Header.Table2 = [ {Table3.Value = \"[1] This is TypeTable3\"}, {Table3.Value = \"[2] This is TypeTable3\"}, {Table3.Value = \"[3] This is TypeTable3\"} ]");
            writer.Flush();

            var typeArrayOfTable = CsTomlSerializer.Deserialize<TypeArrayOfTable>(buffer.WrittenSpan);
            Validate(typeArrayOfTable);
        }
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("[Header]");
            writer.AppendLine("Table2 = [ {Table3.Value = \"[1] This is TypeTable3\"}, {Table3.Value = \"[2] This is TypeTable3\"}, {Table3.Value = \"[3] This is TypeTable3\"} ]");
            writer.Flush();

            var typeArrayOfTable = CsTomlSerializer.Deserialize<TypeArrayOfTable>(buffer.WrittenSpan);
            Validate(typeArrayOfTable);
        }

        static void Validate(TypeArrayOfTable typeArrayOfTable)
        {
            typeArrayOfTable.ShouldNotBeNull();
            typeArrayOfTable.Header.Table2.Count.ShouldBe(3);
            typeArrayOfTable.Header.Table2[0].Table3.Value.ShouldBe("[1] This is TypeTable3");
            typeArrayOfTable.Header.Table2[1].Table3.Value.ShouldBe("[2] This is TypeTable3");
            typeArrayOfTable.Header.Table2[2].Table3.Value.ShouldBe("[3] This is TypeTable3");
        }
    }
}

public class TypeArrayOfTables2Test
{
    private TypeArrayOfTable2 type;

    public TypeArrayOfTables2Test()
    {
        type = new TypeArrayOfTable2()
        {
            Dict = new Dictionary<long, string>()
            {
                [123] = "Value",
                [-1] = "Value",
                [123456789] = "Value",
            },
            TestStructArray = new List<TestStruct?>()
            {
                 new TestStruct() { Value = 1234, Str = "Test1234" },
                 new TestStruct() { Value = 5678, Str = "Test5678" },
            }.ToImmutableArray(),
        };
    }

    [Fact]
    public void Serialize()
    {
        using var bytes = CsTomlSerializer.Serialize(type);
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("TestStructArray = [ {Value = 1234, Str = \"Test1234\"}, {Value = 5678, Str = \"Test5678\"} ]");
        writer.AppendLine("Dict = {123 = \"Value\", -1 = \"Value\", 123456789 = \"Value\"}");
        writer.Flush();
        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(type, Option.Header);
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("TestStructArray = [ {Value = 1234, Str = \"Test1234\"}, {Value = 5678, Str = \"Test5678\"} ]");
        writer.AppendLine("[Dict]");
        writer.AppendLine("123 = \"Value\"");
        writer.AppendLine("-1 = \"Value\"");
        writer.AppendLine("123456789 = \"Value\"");
        writer.Flush();
        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithArrayHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(type, Option.ArrayHeader);
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Dict = {123 = \"Value\", -1 = \"Value\", 123456789 = \"Value\"}");
        writer.AppendLine("[[TestStructArray]]");
        writer.AppendLine("Value = 1234");
        writer.AppendLine("Str = \"Test1234\"");
        writer.AppendLine();
        writer.AppendLine("[[TestStructArray]]");
        writer.AppendLine("Value = 5678");
        writer.AppendLine("Str = \"Test5678\"");
        writer.AppendLine();
        writer.Flush();
        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithHeaderAndArrayHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(type, Option.HeaderAndArrayHeader);
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("[[TestStructArray]]");
        writer.AppendLine("Value = 1234");
        writer.AppendLine("Str = \"Test1234\"");
        writer.AppendLine();
        writer.AppendLine("[[TestStructArray]]");
        writer.AppendLine("Value = 5678");
        writer.AppendLine("Str = \"Test5678\"");
        writer.AppendLine();
        writer.AppendLine("[Dict]");
        writer.AppendLine("123 = \"Value\"");
        writer.AppendLine("-1 = \"Value\"");
        writer.AppendLine("123456789 = \"Value\"");
        writer.Flush();
        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void Deserialize()
    {
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("[Dict]");
            writer.AppendLine("123 = \"Value\"");
            writer.AppendLine("-1 = \"Value\"");
            writer.AppendLine("123456789 = \"Value\"");
            writer.AppendLine();
            writer.AppendLine("[[TestStructArray]]");
            writer.AppendLine("Value = 1234");
            writer.AppendLine("Str = \"Test1234\"");
            writer.AppendLine();
            writer.AppendLine("[[TestStructArray]]");
            writer.AppendLine("Value = 5678");
            writer.AppendLine("Str = \"Test5678\"");
            writer.Flush();

            var typeArrayOfTable2 = CsTomlSerializer.Deserialize<TypeArrayOfTable2>(buffer.WrittenSpan);
            Validate(typeArrayOfTable2);
        }
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("TestStructArray = [ {Value = 1234, Str = \"Test1234\"}, {Value = 5678, Str = \"Test5678\"} ]");
            writer.AppendLine("Dict = {123 = \"Value\", -1 = \"Value\", 123456789 = \"Value\"}");
            writer.Flush();

            var typeArrayOfTable2 = CsTomlSerializer.Deserialize<TypeArrayOfTable2>(buffer.WrittenSpan);
            Validate(typeArrayOfTable2);
        }
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("TestStructArray = [ {Value = 1234, Str = \"Test1234\"}, {Value = 5678, Str = \"Test5678\"} ]");
            writer.AppendLine("[Dict]");
            writer.AppendLine("123 = \"Value\"");
            writer.AppendLine("-1 = \"Value\"");
            writer.AppendLine("123456789 = \"Value\"");
            writer.Flush();

            var typeArrayOfTable2 = CsTomlSerializer.Deserialize<TypeArrayOfTable2>(buffer.WrittenSpan);
            Validate(typeArrayOfTable2);
        }
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Dict = {123 = \"Value\", -1 = \"Value\", 123456789 = \"Value\"}");
            writer.AppendLine("[[TestStructArray]]");
            writer.AppendLine("Value = 1234");
            writer.AppendLine("Str = \"Test1234\"");
            writer.AppendLine();
            writer.AppendLine("[[TestStructArray]]");
            writer.AppendLine("Value = 5678");
            writer.AppendLine("Str = \"Test5678\"");
            writer.AppendLine();
            writer.Flush();

            var typeArrayOfTable2 = CsTomlSerializer.Deserialize<TypeArrayOfTable2>(buffer.WrittenSpan);
            Validate(typeArrayOfTable2);
        }
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("[[TestStructArray]]");
            writer.AppendLine("Value = 1234");
            writer.AppendLine("Str = \"Test1234\"");
            writer.AppendLine();
            writer.AppendLine("[[TestStructArray]]");
            writer.AppendLine("Value = 5678");
            writer.AppendLine("Str = \"Test5678\"");
            writer.AppendLine();
            writer.AppendLine("[Dict]");
            writer.AppendLine("123 = \"Value\"");
            writer.AppendLine("-1 = \"Value\"");
            writer.AppendLine("123456789 = \"Value\"");
            writer.Flush();

            var typeArrayOfTable2 = CsTomlSerializer.Deserialize<TypeArrayOfTable2>(buffer.WrittenSpan);
            Validate(typeArrayOfTable2);
        }

        static void Validate(TypeArrayOfTable2 typeArrayOfTable2)
        {
            typeArrayOfTable2.ShouldNotBeNull();
            typeArrayOfTable2.Dict.ShouldNotBeNull();
            typeArrayOfTable2.Dict[123].ShouldBe("Value");
            typeArrayOfTable2.Dict[-1].ShouldBe("Value");
            typeArrayOfTable2.Dict[123456789].ShouldBe("Value");
            typeArrayOfTable2.TestStructArray.Length.ShouldBe(2);
            typeArrayOfTable2.TestStructArray[0]!.Value.Str.ShouldBe("Test1234");
            typeArrayOfTable2.TestStructArray[0]!.Value.Value.ShouldBe(1234);
            typeArrayOfTable2.TestStructArray[1]!.Value.Str.ShouldBe("Test5678");
            typeArrayOfTable2.TestStructArray[1]!.Value.Value.ShouldBe(5678);
        }
    }
}
