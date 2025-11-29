using Shouldly;
using Utf8StringInterpolation;
using CsToml.Generator.Other;

namespace CsToml.Generator.Tests;

public class TypeTableTest
{
    private TypeTable table;

    public TypeTableTest()
    {
        table = new TypeTable();
        table.Table2 = new TypeTable2();
        table.Table2.Table3 = new TypeTable3() { Value = "This is TypeTable3" };

    }

    [Fact]
    public void Serialize()
    {
        using var bytes = CsTomlSerializer.Serialize(table);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Table2.Table3.Value = \"This is TypeTable3\"");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(table, Option.Header);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("[Table2]");
        writer.AppendLine("[Table2.Table3]");
        writer.AppendLine("Value = \"This is TypeTable3\"");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithArrayHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(table, Option.ArrayHeader);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Table2.Table3.Value = \"This is TypeTable3\"");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithHeaderAndArrayHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(table, Option.HeaderAndArrayHeader);
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("[Table2]");
        writer.AppendLine("[Table2.Table3]");
        writer.AppendLine("Value = \"This is TypeTable3\"");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void Deserialize()
    {
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Table2.Table3.Value = \"This is TypeTable3\"");
            writer.Flush();

            var type = CsTomlSerializer.Deserialize<TypeTable>(buffer.WrittenSpan);
            type.Table2.Table3.Value.ShouldBe("This is TypeTable3");
        }
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("[Table2]");
            writer.AppendLine("[Table2.Table3]");
            writer.AppendLine("Value = \"This is TypeTable3\"");
            writer.Flush();

            var type = CsTomlSerializer.Deserialize<TypeTable>(buffer.WrittenSpan);
            type.Table2.Table3.Value.ShouldBe("This is TypeTable3");
        }
    }
}
