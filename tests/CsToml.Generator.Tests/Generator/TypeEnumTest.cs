using Shouldly;
using Utf8StringInterpolation;

namespace CsToml.Generator.Tests;

public class TypeEnumTest
{
    [Test]
    public void Deserialize()
    {
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Color = \"Red\"");
            writer.Flush();

            var type = CsTomlSerializer.Deserialize<TypeEnum>(buffer.WrittenSpan);
            type.Color.ShouldBe(Color.Red);
        }
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Color = \"Green\"");
            writer.Flush();

            var type = CsTomlSerializer.Deserialize<TypeEnum>(buffer.WrittenSpan);
            type.Color.ShouldBe(Color.Green);
        }
    }

    [Test]
    public void Serialize()
    {
        var type = new TypeEnum()
        {
            Color = Color.Red,
        };
        {
            using var bytes = CsTomlSerializer.Serialize(type);
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Color = \"Red\"");
            writer.Flush();
            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Color = \"Red\"");
            writer.Flush();
            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
    }
}
