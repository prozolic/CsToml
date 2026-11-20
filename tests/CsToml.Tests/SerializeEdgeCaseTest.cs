using System.Collections;
using Utf8StringInterpolation;

namespace CsToml.Tests;

public class SerializeEdgeCaseTest
{
    [Test]
    public void SubTableHeader_AfterKeyNamedLikeParentTable()
    {
        var document = CsTomlSerializer.Deserialize<TomlDocument>("[a]\na = 1\n[a.b]\nx = 2\n"u8);
        using var serialized = CsTomlSerializer.Serialize(document);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("[a]");
        writer.AppendLine("a = 1");
        writer.AppendLine();
        writer.AppendLine("[a.b]");
        writer.AppendLine("x = 2");
        writer.Flush();

        serialized.ByteSpan.ToArray().ShouldBe(buffer.ToArray());
    }

    [Test]
    public void SubTableUnderDottedKeyTable_KeyValueOrder()
    {
        var document = CsTomlSerializer.Deserialize<TomlDocument>("a.b.c = 1\na.d = 2\ne = 3\n[a.b.f]\ng = 4\n[[a.h]]\ni = 5\n"u8);
        using var serialized = CsTomlSerializer.Serialize(document);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("a.b.c = 1");
        writer.AppendLine("a.d = 2");
        writer.AppendLine("e = 3");
        writer.AppendLine();
        writer.AppendLine("[a.b.f]");
        writer.AppendLine("g = 4");
        writer.AppendLine();
        writer.AppendLine("[[a.h]]");
        writer.AppendLine("i = 5");
        writer.Flush();

        serialized.ByteSpan.ToArray().ShouldBe(buffer.ToArray());
    }

    [Test]
    public void SubTableUnderDottedKeyTable_InTable()
    {
        var document = CsTomlSerializer.Deserialize<TomlDocument>("[t]\na.b = 1\nc = 2\n[t.a.d]\ne = 3\n"u8);
        using var serialized = CsTomlSerializer.Serialize(document);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("[t]");
        writer.AppendLine("a.b = 1");
        writer.AppendLine("c = 2");
        writer.AppendLine();
        writer.AppendLine("[t.a.d]");
        writer.AppendLine("e = 3");
        writer.Flush();

        serialized.ByteSpan.ToArray().ShouldBe(buffer.ToArray());
    }

    [Test]
    public void SubTableDefinedBeforeParentTable_KeepsOrder()
    {
        var document = CsTomlSerializer.Deserialize<TomlDocument>("[a.b]\nc = 1\n[[a.x]]\n[a]\nd = 2\n"u8);
        using var serialized = CsTomlSerializer.Serialize(document);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("[a.b]");
        writer.AppendLine("c = 1");
        writer.AppendLine();
        writer.AppendLine("[a]");
        writer.AppendLine("d = 2");
        writer.AppendLine();
        writer.AppendLine("[[a.x]]");
        writer.Flush();

        serialized.ByteSpan.ToArray().ShouldBe(buffer.ToArray());
    }

    [Test]
    public void Hashtable_DateOnlyKey()
    {
        var value = new Hashtable { [new DateOnly(2024, 1, 2)] = 1, [new DateOnly(2024, 1, 3)] = 2 };
        using var serialized = CsTomlSerializer.SerializeValueType(value);

        serialized.ByteSpan.ToArray().ShouldBe("{\"2024-01-03\" = 2, \"2024-01-02\" = 1}"u8.ToArray());
    }

    [Test]
    public void NestedDictionaryHeader_QuotedKey()
    {
        var value = new Dictionary<string, object?>
        {
            ["a b"] = new Dictionary<string, object?>
            {
                ["c"] = new Dictionary<string, object?> { ["d"] = 1 },
            },
        };
        var options = CsTomlSerializerOptions.Default with { SerializeOptions = new() { TableStyle = TomlTableStyle.Header } };
        using var serialized = CsTomlSerializer.Serialize(value, options);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("[\"a b\"]");
        writer.AppendLine("[\"a b\".c]");
        writer.AppendLine("d = 1");
        writer.Flush();

        serialized.ByteSpan.ToArray().ShouldBe(buffer.ToArray());
    }

    [Test]
    public void ControlCharacter()
    {
        using var keyAndValue = CsTomlSerializer.SerializeValueType(new Dictionary<string, object?> { ["a\bb"] = "a\bb" });
        keyAndValue.ByteSpan.ToArray().ShouldBe(@"{""a\bb"" = ""a\bb""}"u8.ToArray());

        using var multiLine = CsTomlSerializer.SerializeValueType("a\n\bb");
        multiLine.ByteSpan.ToArray().ShouldBe("\"\"\"a\\n\\bb\"\"\""u8.ToArray());
    }

    [Test]
    public void LiteralBackslashUnicodeEscape()
    {
        var value = new Dictionary<string, object?>
        {
            [@"\u0041"] = @"\u0041",
            [@"\U00000041"] = @"\U00000041",
            [@"\u'abc"] = @"\u'abc",
        };
        using var serialized = CsTomlSerializer.SerializeValueType(value);

        serialized.ByteSpan.ToArray().ShouldBe(@"{""\\u0041"" = ""\\u0041"", ""\\U00000041"" = ""\\U00000041"", ""\\u'abc"" = ""\\u'abc""}"u8.ToArray());
    }

    [Test]
    public void QuoteInKey()
    {
        using var serialized = CsTomlSerializer.SerializeValueType(new Dictionary<string, object?> { ["a'b"] = 1, ["a\"b"] = 2 });

        serialized.ByteSpan.ToArray().ShouldBe("{\"a'b\" = 1, 'a\"b' = 2}"u8.ToArray());
    }
}
