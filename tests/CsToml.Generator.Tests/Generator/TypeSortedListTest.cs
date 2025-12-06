using Shouldly;
using Utf8StringInterpolation;

namespace CsToml.Generator.Tests;

public class TypeSortedListTest
{
    private TypeSortedList typeSortedList;

    public TypeSortedListTest()
    {
        var sortedList = new SortedList<string, string>()
        {
            ["key"] = "value",
            ["key2"] = "value2",
            ["key3"] = "value3",
        };

        typeSortedList = new TypeSortedList() { Value = sortedList };
    }

    [Fact]
    public void Serialize()
    {
        using var bytes = CsTomlSerializer.Serialize(typeSortedList);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = {key = \"value\", key2 = \"value2\", key3 = \"value3\"}");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(typeSortedList, Option.Header);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("[Value]");
        writer.AppendLine("key = \"value\"");
        writer.AppendLine("key2 = \"value2\"");
        writer.AppendLine("key3 = \"value3\"");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithArrayHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(typeSortedList, Option.ArrayHeader);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = {key = \"value\", key2 = \"value2\", key3 = \"value3\"}");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void SerializeWithHeaderAndArrayHeaderOption()
    {
        using var bytes = CsTomlSerializer.Serialize(typeSortedList, Option.HeaderAndArrayHeader);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("[Value]");
        writer.AppendLine("key = \"value\"");
        writer.AppendLine("key2 = \"value2\"");
        writer.AppendLine("key3 = \"value3\"");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void Deserialize()
    {
        var expected = typeSortedList.Value;

        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = {key = \"value\", key2 = \"value2\", key3 = \"value3\" }");
            writer.Flush();

            var type = CsTomlSerializer.Deserialize<TypeSortedList>(buffer.WrittenSpan);
            type.ShouldNotBeNull();
            type.Value.SequenceEqual(expected).ShouldBeTrue();
        }
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("[Value]");
            writer.AppendLine("key = \"value\"");
            writer.AppendLine("key2 = \"value2\"");
            writer.AppendLine("key3 = \"value3\"");
            writer.Flush();

            var type = CsTomlSerializer.Deserialize<TypeSortedList>(buffer.WrittenSpan);
            type.ShouldNotBeNull();
            type.Value.SequenceEqual(expected).ShouldBeTrue();
        }
    }
}
