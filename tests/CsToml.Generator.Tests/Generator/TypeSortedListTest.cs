using Shouldly;
using Utf8StringInterpolation;

namespace CsToml.Generator.Tests;

public class TypeSortedListTest
{
    [Fact]
    public void Serialize()
    {
        var sortedList = new SortedList<string, string>()
        {
            ["key"] = "value",
            ["key2"] = "value2",
            ["key3"] = "value3",
        };

        var type = new TypeSortedList() { Value = sortedList };
        {
            using var bytes = CsTomlSerializer.Serialize(type);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = {key = \"value\", key2 = \"value2\", key3 = \"value3\"}");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("[Value]");
            writer.AppendLine("key = \"value\"");
            writer.AppendLine("key2 = \"value2\"");
            writer.AppendLine("key3 = \"value3\"");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
    }

    [Fact]
    public void Deserialize()
    {
        var expected = new SortedList<string, string>()
        {
            ["key"] = "value",
            ["key2"] = "value2",
            ["key3"] = "value3",
        };

        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = {key = \"value\", key2 = \"value2\", key3 = \"value3\" }");
            writer.Flush();

            var type = CsTomlSerializer.Deserialize<TypeSortedList>(buffer.WrittenSpan);
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
            type.Value.SequenceEqual(expected).ShouldBeTrue();
        }
    }
}
