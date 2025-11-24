using Shouldly;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using Utf8StringInterpolation;

namespace CsToml.Generator.Tests;

public class TypeCollectionTest
{
    [Fact]
    public void Serialize()
    {
        var type = new TypeCollection()
        {
            Value = [1, 2, 3, 4, 5],
            Value2 = new Stack<int>([5, 4, 3, 2, 1]),
            Value3 = new HashSet<int>([1, 2, 3, 4, 5]),
            Value4 = new SortedSet<int>([1, 2, 3, 4, 5]),
            Value5 = new Queue<int>([1, 2, 3, 4, 5]),
            Value6 = new LinkedList<int>([1, 2, 3, 4, 5]),
            Value7 = new ConcurrentQueue<int>([1, 2, 3, 4, 5]),
            Value8 = new ConcurrentStack<int>([5, 4, 3, 2, 1]),
            Value9 = new ConcurrentBag<int>([5, 4, 3, 2, 1]),
            Value10 = new ReadOnlyCollection<int>([1, 2, 3, 4, 5])
        };

        {
            using var bytes = CsTomlSerializer.Serialize(type);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("Value2 = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("Value3 = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("Value4 = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("Value5 = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("Value6 = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("Value7 = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("Value8 = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("Value9 = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("Value10 = [ 1, 2, 3, 4, 5 ]");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
        {
            using var bytes = CsTomlSerializer.Serialize(type, Option.Header);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("Value2 = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("Value3 = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("Value4 = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("Value5 = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("Value6 = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("Value7 = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("Value8 = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("Value9 = [ 1, 2, 3, 4, 5 ]");
            writer.AppendLine("Value10 = [ 1, 2, 3, 4, 5 ]");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
    }

    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("Value2 = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("Value3 = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("Value4 = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("Value5 = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("Value6 = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("Value7 = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("Value8 = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("Value9 = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("Value10 = [ 1, 2, 3, 4, 5 ]");
        writer.Flush();

        var type = CsTomlSerializer.Deserialize<TypeCollection>(buffer.WrittenSpan);

        int[] expected = [1, 2, 3, 4, 5];
        type.Value.ShouldBe(expected);
        type.Value2.ShouldBe(expected);
        type.Value3.ShouldBe(expected);
        type.Value4.ShouldBe(expected);
        type.Value5.ShouldBe(expected);
        type.Value6.ShouldBe(expected);
        type.Value7.ShouldBe(expected);
        type.Value8.ShouldBe(expected);
        type.Value9.ShouldBe(expected);
        type.Value10.ShouldBe(expected);
    }
}
