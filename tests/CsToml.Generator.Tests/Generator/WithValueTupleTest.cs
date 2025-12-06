using Shouldly;
using Utf8StringInterpolation;

namespace CsToml.Generator.Tests;

public class WithValueTupleTest
{
    [Fact]
    public void Serialize()
    {
        var value = new WithValueTuple()
        {
            One = new ValueTuple<int>(1),
            Two = new ValueTuple<int, int>(1, 2),
            Three = new ValueTuple<int, int, int>(1, 2, 3),
            Four = new ValueTuple<int, int, int, int>(1, 2, 3, 4),
            Five = new ValueTuple<int, int, int, int, int>(1, 2, 3, 4, 5),
            Six = new ValueTuple<int, int, int, int, int, int>(1, 2, 3, 4, 5, 6),
            Seven = new ValueTuple<int, int, int, int, int, int, int>(1, 2, 3, 4, 5, 6, 7),
            Eight = new ValueTuple<int, int, int, int, int, int, int, ValueTuple<int>>(1, 2, 3, 4, 5, 6, 7, new ValueTuple<int>(8))
        };
        {
            using var bytes = CsTomlSerializer.Serialize(value);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("One = 1");
            writer.AppendLine("Two = [ 1, 2 ]");
            writer.AppendLine("Three = [ 1, 2, 3 ]");
            writer.AppendLine("Four = [ 1, 2, 3, 4 ]");
            writer.AppendLine("Five = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("Six = [ 1, 2, 3, 4, 5, 6 ]");
            writer.AppendLine("Seven = [ 1, 2, 3, 4, 5, 6, 7 ]");
            writer.AppendLine("Eight = [ 1, 2, 3, 4, 5, 6, 7, 8 ]");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(value, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("One = 1");
            writer.AppendLine("Two = [ 1, 2 ]");
            writer.AppendLine("Three = [ 1, 2, 3 ]");
            writer.AppendLine("Four = [ 1, 2, 3, 4 ]");
            writer.AppendLine("Five = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("Six = [ 1, 2, 3, 4, 5, 6 ]");
            writer.AppendLine("Seven = [ 1, 2, 3, 4, 5, 6, 7 ]");
            writer.AppendLine("Eight = [ 1, 2, 3, 4, 5, 6, 7, 8 ]");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
    }

    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("One = 1");
        writer.AppendLine("Two = [ 1, 2 ]");
        writer.AppendLine("Three = [ 1, 2, 3 ]");
        writer.AppendLine("Four = [ 1, 2, 3, 4 ]");
        writer.AppendLine("Five = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("Six = [ 1, 2, 3, 4, 5, 6 ]");
        writer.AppendLine("Seven = [ 1, 2, 3, 4, 5, 6, 7 ]");
        writer.AppendLine("Eight = [ 1, 2, 3, 4, 5, 6, 7, 8 ]");
        writer.Flush();

        var type = CsTomlSerializer.Deserialize<WithValueTuple>(buffer.WrittenSpan);
        type.One.ShouldBe(new ValueTuple<int>(1));
        type.Two.ShouldBe(new ValueTuple<int, int>(1, 2));
        type.Three.ShouldBe(new ValueTuple<int, int, int>(1, 2, 3));
        type.Four.ShouldBe(new ValueTuple<int, int, int, int>(1, 2, 3, 4));
        type.Five.ShouldBe(new ValueTuple<int, int, int, int, int>(1, 2, 3, 4, 5));
        type.Six.ShouldBe(new ValueTuple<int, int, int, int, int, int>(1, 2, 3, 4, 5, 6));
        type.Seven.ShouldBe(new ValueTuple<int, int, int, int, int, int, int>(1, 2, 3, 4, 5, 6, 7));
        type.Eight.ShouldBe(new ValueTuple<int, int, int, int, int, int, int, ValueTuple<int>>(1, 2, 3, 4, 5, 6, 7, new ValueTuple<int>(8)));
    }
}
