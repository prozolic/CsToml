using CsToml.Error;
using Shouldly;
using System.Text;
using Utf8StringInterpolation;

namespace CsToml.Generator.Tests;

// union declaration syntax tests (net11.0 only).
public class UnionNet11Test
{
    [Test]
    public void SerializeUnionDeclarationMember()
    {
        var target = new Net11UnionHolder() { Name = "cfg", Num = 123L };
        using var bytes = CsTomlSerializer.Serialize(target);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Name = \"cfg\"");
        writer.AppendLine("Num = 123");
        writer.Flush();

        bytes.ByteSpan.ToArray().ShouldBe(buffer.ToArray());
    }

    [Test]
    public void RoundTripUnionDeclarationMember()
    {
        var target = new Net11UnionHolder() { Name = "cfg", Num = 123L };
        using var bytes = CsTomlSerializer.Serialize(target);
        var deserialized = CsTomlSerializer.Deserialize<Net11UnionHolder>(bytes.ByteSpan);

        deserialized!.Num.Value.ShouldBe(123L);
    }

    [Test]
    public void DeserializeMissingUnionDeclarationKeyAsDefault()
    {
        var deserialized = CsTomlSerializer.Deserialize<Net11UnionHolder>("Name = \"x\""u8);
        deserialized!.Num.Value.ShouldBeNull();
    }

    [Test]
    public void SerializeDefaultUnionDeclarationThrows()
    {
        var target = new Net11UnionHolder() { Name = "x", Num = default };
        Should.Throw<CsTomlException>(() =>
        {
            using var bytes = CsTomlSerializer.Serialize(target);
        });
    }

    [Test]
    public void SerializeUnionDeclarationWithMismatchedCaseThrows()
    {
        var target = new Net11UnionHolder() { Name = "x", Num = "text" };
        Should.Throw<CsTomlException>(() =>
        {
            using var bytes = CsTomlSerializer.Serialize(target);
        });
    }

    [Test]
    public void RoundTripStandaloneUnionDeclarationWithManualRegistration()
    {
        global::CsToml.Generated.CsToml.Generator.Tests.StandaloneIntOrTextFormatter.Register();

        StandaloneIntOrText value = 42L;
        using var bytes = CsTomlSerializer.SerializeValueType(value);
        Encoding.UTF8.GetString(bytes.ByteSpan).ShouldBe("42");

        var deserialized = CsTomlSerializer.DeserializeValueType<StandaloneIntOrText>(bytes.ByteSpan);
        deserialized.Value.ShouldBe(42L);
    }

    [Test]
    public void RoundTripGenericUnionDeclarationMember()
    {
        var target = new Net11GenericUnionHolder() { Value = new List<long> { 1, 2 } };
        using var bytes = CsTomlSerializer.Serialize(target);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = [ 1, 2 ]");
        writer.Flush();
        bytes.ByteSpan.ToArray().ShouldBe(buffer.ToArray());

        var deserialized = CsTomlSerializer.Deserialize<Net11GenericUnionHolder>(bytes.ByteSpan);
        ((List<long>)deserialized!.Value.Value!).ShouldBe(new List<long> { 1, 2 });
    }
}
