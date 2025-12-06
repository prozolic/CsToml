using Newtonsoft.Json.Linq;
using Shouldly;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization.Metadata;
using Utf8StringInterpolation;

namespace CsToml.Generator.Tests;

public class TypeCollectionInterfaceTest
{
    public static IEnumerable<object[]> GetTypeCollectionInterfaces()
    {
        yield return new object[] { new TypeCollectionInterface()
        {
            Value = new List<int>([1, 2, 3, 4, 5]),
            Value2 = new List<int>([1, 2, 3, 4, 5]),
            Value3 = new List<int>([1, 2, 3, 4, 5]),
            Value4 = new HashSet<int>([1, 2, 3, 4, 5]),
            Value5 = new List<int>([1, 2, 3, 4, 5]),
            Value6 = new List<int>([1, 2, 3, 4, 5]),
            Value7 = new HashSet<int>([1, 2, 3, 4, 5]),
        }};
        yield return new object[] { new TypeCollectionInterface()
        {
            Value = new ReadOnlyCollection<int>([1, 2, 3, 4, 5]),
            Value2 = new ReadOnlyCollection<int>([1, 2, 3, 4, 5]),
            Value3 = new ReadOnlyCollection<int>([1, 2, 3, 4, 5]),
            Value4 = new SortedSet<int>([1, 2, 3, 4, 5]),
            Value5 = new ReadOnlyCollection<int>([1, 2, 3, 4, 5]),
            Value6 = new ReadOnlyCollection<int>([1, 2, 3, 4, 5]),
            Value7 = new SortedSet<int>([1, 2, 3, 4, 5]),
        }};
    }

    [Theory]
    [MemberData(nameof(GetTypeCollectionInterfaces))]
    public void Serialize(TypeCollectionInterface typeCollectionInterface)
    {
        using var bytes = CsTomlSerializer.Serialize(typeCollectionInterface);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("Value2 = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("Value3 = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("Value4 = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("Value5 = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("Value6 = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("Value7 = [ 1, 2, 3, 4, 5 ]");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Theory]
    [MemberData(nameof(GetTypeCollectionInterfaces))]
    public void SerializeWithHeaderOption(TypeCollectionInterface typeCollectionInterface)
    {
        using var bytes = CsTomlSerializer.Serialize(typeCollectionInterface, Option.Header);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("Value2 = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("Value3 = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("Value4 = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("Value5 = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("Value6 = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("Value7 = [ 1, 2, 3, 4, 5 ]");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Theory]
    [MemberData(nameof(GetTypeCollectionInterfaces))]
    public void SerializeWithArrayHeaderOption(TypeCollectionInterface typeCollectionInterface)
    {
        using var bytes = CsTomlSerializer.Serialize(typeCollectionInterface, Option.ArrayHeader);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("Value2 = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("Value3 = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("Value4 = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("Value5 = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("Value6 = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("Value7 = [ 1, 2, 3, 4, 5 ]");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Theory]
    [MemberData(nameof(GetTypeCollectionInterfaces))]
    public void SerializeWithHeaderAndArrayHeaderOption(TypeCollectionInterface typeCollectionInterface)
    {
        using var bytes = CsTomlSerializer.Serialize(typeCollectionInterface, Option.HeaderAndArrayHeader);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("Value2 = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("Value3 = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("Value4 = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("Value5 = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("Value6 = [ 1, 2, 3, 4, 5 ]");
        writer.AppendLine("Value7 = [ 1, 2, 3, 4, 5 ]");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
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
        writer.Flush();

        var type = CsTomlSerializer.Deserialize<TypeCollectionInterface>(buffer.WrittenSpan);

        int[] expected = [1, 2, 3, 4, 5];
        type.Value.ShouldBe(expected);
        type.Value2.ShouldBe(expected);
        type.Value3.ShouldBe(expected);
        type.Value4.ShouldBe(expected);
        type.Value5.ShouldBe(expected);
        type.Value6.ShouldBe(expected);
        type.Value7.ShouldBe(expected);
    }
}
