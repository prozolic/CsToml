using Shouldly;
using Utf8StringInterpolation;

namespace CsToml.Generator.Tests;

public class WithNullableCollectionTest
{
    [Fact]
    public void Serialize()
    {
        var type = new WithNullableCollection
        {
            Value = [1, 2, 3, 4, 5]
        };

        {
            using var bytes = CsTomlSerializer.Serialize(type);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = [ 1, 2, 3, 4, 5 ]");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = [ 1, 2, 3, 4, 5 ]");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
    }

    [Fact]
    public void Deserialize()
    {
        var nullableValue = CsTomlSerializer.Deserialize<WithNullableCollection>(""u8);
        nullableValue.Value.ShouldBeNull();

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = [1,3,5]");
        writer.Flush();

        var type = CsTomlSerializer.Deserialize<WithNullableCollection>(buffer.WrittenSpan);
        type.Value.ShouldBe([1, 3, 5]);
    }
}
