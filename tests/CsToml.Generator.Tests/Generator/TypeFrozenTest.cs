using Shouldly;
using System.Collections.Frozen;
using Utf8StringInterpolation;

namespace CsToml.Generator.Tests;

public class TypeFrozenTest
{
    [Fact]
    public void Serialize()
    {
        var type = new TypeFrozen()
        {
            Value = new HashSet<long>([1, 2, 3, 4, 5]).ToFrozenSet(),
            Value2 = new Dictionary<long, string>()
            {
                [123] = "Value",
                [-1] = "Value2",
                [123456789] = "Value3",
            }.ToFrozenDictionary()
        };

        {
            using var bytes = CsTomlSerializer.Serialize(type);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("Value2 = {-1 = \"Value2\", 123 = \"Value\", 123456789 = \"Value3\"}");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("[Value2]");
            writer.AppendLine("-1 = \"Value2\"");
            writer.AppendLine("123 = \"Value\"");
            writer.AppendLine("123456789 = \"Value3\"");
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
            writer.AppendLine("Value = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("Value2 = {123 = \"Value\", -1 = \"Value2\", 123456789 = \"Value3\"}");
            writer.Flush();

            var type = CsTomlSerializer.Deserialize<TypeFrozen>(buffer.WrittenSpan);
            type.Value.ShouldBe(new HashSet<long>([1, 2, 3, 4, 5]).ToFrozenSet());
            Validate(type.Value2);
        }

        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("[Value2]");
            writer.AppendLine("123 = \"Value\"");
            writer.AppendLine("-1 = \"Value2\"");
            writer.AppendLine("123456789 = \"Value3\"");
            writer.Flush();

            var type = CsTomlSerializer.Deserialize<TypeFrozen>(buffer.WrittenSpan);
            type.Value.ShouldBe(new HashSet<long>([1, 2, 3, 4, 5]).ToFrozenSet());
            Validate(type.Value2);
        }

        static void Validate(FrozenDictionary<long, string> dynamicDict)
        {
            string value = dynamicDict[123];
            value.ShouldBe("Value");
            string value2 = dynamicDict[-1];
            value2.ShouldBe("Value2");
            string value3 = dynamicDict[123456789];
            value3.ShouldBe("Value3");
        }
    }
}
