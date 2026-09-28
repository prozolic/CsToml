using CsToml.Error;
using CsToml.Formatter;
using CsToml.Formatter.Resolver;
using Shouldly;
using System.Buffers;
using System.Text;
using Utf8StringInterpolation;

namespace CsToml.Generator.Tests;

public class UnionTest
{
    [Fact]
    public void SerializeUnionMember()
    {
        var target = new UnionHolder() { Name = "cfg", Value = new StringOrInt(123) };
        using var bytes = CsTomlSerializer.Serialize(target);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Name = \"cfg\"");
        writer.AppendLine("Value = 123");
        writer.Flush();

        bytes.ByteSpan.ToArray().ShouldBe(buffer.ToArray());
    }

    [Fact]
    public void SerializeUnionMemberWithHeaderStyleStaysInline()
    {
        // The pinned case is a primitive, so the header style does not apply to this member.
        var target = new UnionHolder() { Name = "cfg", Value = new StringOrInt(123) };
        using var bytes = CsTomlSerializer.Serialize(target, Option.Header);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Name = \"cfg\"");
        writer.AppendLine("Value = 123");
        writer.Flush();

        bytes.ByteSpan.ToArray().ShouldBe(buffer.ToArray());
    }

    [Fact]
    public void RoundTripUnionMember()
    {
        var target = new UnionHolder() { Name = "cfg", Value = new StringOrInt(123) };
        using var bytes = CsTomlSerializer.Serialize(target);
        var deserialized = CsTomlSerializer.Deserialize<UnionHolder>(bytes.ByteSpan);

        deserialized!.Name.ShouldBe("cfg");
        deserialized.Value.Value.ShouldBe(123);
    }

    [Fact]
    public void DeserializeMissingUnionKeyAsDefault()
    {
        var deserialized = CsTomlSerializer.Deserialize<UnionHolder>("Name = \"x\""u8);

        deserialized!.Name.ShouldBe("x");
        deserialized.Value.Value.ShouldBeNull();
    }

    [Fact]
    public void SerializeDefaultUnionThrows()
    {
        var target = new UnionHolder() { Name = "x", Value = default };
        Should.Throw<CsTomlException>(() =>
        {
            using var bytes = CsTomlSerializer.Serialize(target);
        });
    }

    [Fact]
    public void SerializeMismatchedCaseThrows()
    {
        // StringOrInt is pinned to int; holding the string case is a declaration violation.
        var target = new UnionHolder() { Name = "x", Value = new StringOrInt("text") };
        Should.Throw<CsTomlException>(() =>
        {
            using var bytes = CsTomlSerializer.Serialize(target);
        });
    }

    [Fact]
    public void RoundTripTryGetValueUnion()
    {
        var target = new TriStateHolder() { State = new TriState(42L) };
        using var bytes = CsTomlSerializer.Serialize(target);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("State = 42");
        writer.Flush();
        bytes.ByteSpan.ToArray().ShouldBe(buffer.ToArray());

        var deserialized = CsTomlSerializer.Deserialize<TriStateHolder>(bytes.ByteSpan);
        deserialized!.State.TryGetValue(out long value).ShouldBeTrue();
        value.ShouldBe(42L);
    }

    [Fact]
    public void SerializeValuelessTryGetValueUnionThrows()
    {
        var target = new TriStateHolder() { State = default };
        Should.Throw<CsTomlException>(() =>
        {
            using var bytes = CsTomlSerializer.Serialize(target);
        });
    }

    [Fact]
    public void RoundTripProvidedUnion()
    {
        var target = new ProvidedUnionHolder() { Value = ProvidedUnion.IUnionMembers.Create(7L) };
        using var bytes = CsTomlSerializer.Serialize(target);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = 7");
        writer.Flush();
        bytes.ByteSpan.ToArray().ShouldBe(buffer.ToArray());

        var deserialized = CsTomlSerializer.Deserialize<ProvidedUnionHolder>(bytes.ByteSpan);
        ((ProvidedUnion.IUnionMembers)deserialized!.Value).Value.ShouldBe(7L);
    }

    [Fact]
    public void RoundTripClassUnion()
    {
        var target = new ClassUnionHolder() { Value = new ClassUnion("abc") };
        using var bytes = CsTomlSerializer.Serialize(target);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = \"abc\"");
        writer.Flush();
        bytes.ByteSpan.ToArray().ShouldBe(buffer.ToArray());

        var deserialized = CsTomlSerializer.Deserialize<ClassUnionHolder>(bytes.ByteSpan);
        deserialized!.Value!.Value.ShouldBe("abc");
    }

    [Fact]
    public void SerializeNullClassUnionIsIgnored()
    {
        var target = new ClassUnionHolder() { Value = null };
        using var bytes = CsTomlSerializer.Serialize(target);
        bytes.ByteSpan.Length.ShouldBe(0);
    }

    [Fact]
    public void RoundTripNestedUnion()
    {
        var target = new OuterUnionHolder() { Value = new OuterUnion(new StringOrInt(5)) };
        using var bytes = CsTomlSerializer.Serialize(target);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = 5");
        writer.Flush();
        bytes.ByteSpan.ToArray().ShouldBe(buffer.ToArray());

        var deserialized = CsTomlSerializer.Deserialize<OuterUnionHolder>(bytes.ByteSpan);
        var inner = (StringOrInt)deserialized!.Value.Value!;
        inner.Value.ShouldBe(5);
    }

    [Fact]
    public void RoundTripGenericUnionWithMemberLevelPin()
    {
        var target = new GenericUnionHolder() { Value = new ListOrText<long>(new List<long> { 1, 2 }) };
        using var bytes = CsTomlSerializer.Serialize(target);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = [ 1, 2 ]");
        writer.Flush();
        bytes.ByteSpan.ToArray().ShouldBe(buffer.ToArray());

        var deserialized = CsTomlSerializer.Deserialize<GenericUnionHolder>(bytes.ByteSpan);
        ((List<long>)deserialized!.Value.Value!).ShouldBe(new List<long> { 1, 2 });
    }

    [Fact]
    public void RoundTripUnionPinnedToPoco()
    {
        var target = new PinnedUnionHolder() { Value = new PinnedUnion(new PinCaseA() { Number = 5 }) };
        using var bytes = CsTomlSerializer.Serialize(target);

        // A member pinned to a POCO follows the TomlSerializedObject templates: in a single-member
        // holder that is the dotted-key style.
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value.Number = 5");
        writer.Flush();
        bytes.ByteSpan.ToArray().ShouldBe(buffer.ToArray());

        var deserialized = CsTomlSerializer.Deserialize<PinnedUnionHolder>(bytes.ByteSpan);
        ((PinCaseA)deserialized!.Value.Value!).Number.ShouldBe(5);
    }

    [Fact]
    public void SerializeUnionPinnedToPocoWithHeaderStyle()
    {
        var target = new PinnedUnionHolder() { Value = new PinnedUnion(new PinCaseA() { Number = 5 }) };
        using var bytes = CsTomlSerializer.Serialize(target, Option.Header);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("[Value]");
        writer.AppendLine("Number = 5");
        writer.Flush();
        bytes.ByteSpan.ToArray().ShouldBe(buffer.ToArray());

        var deserialized = CsTomlSerializer.Deserialize<PinnedUnionHolder>(bytes.ByteSpan);
        ((PinCaseA)deserialized!.Value.Value!).Number.ShouldBe(5);
    }

    [Fact]
    public void SerializeUnionPinnedToPocoWithMismatchedCaseThrows()
    {
        var target = new PinnedUnionHolder() { Value = new PinnedUnion(1L) };
        Should.Throw<CsTomlException>(() =>
        {
            using var bytes = CsTomlSerializer.Serialize(target);
        });
    }

    [Fact]
    public void RoundTripMemberLevelPinnedUnions()
    {
        var target = new MemberPinnedHolder()
        {
            Number = new UnpinnedUnion(9L),
            Text = new UnpinnedUnion("x"),
            AsText = new StringOrInt("hello"),
        };
        using var bytes = CsTomlSerializer.Serialize(target);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Number = 9");
        writer.AppendLine("Text = \"x\"");
        writer.AppendLine("AsText = \"hello\"");
        writer.Flush();
        bytes.ByteSpan.ToArray().ShouldBe(buffer.ToArray());

        var deserialized = CsTomlSerializer.Deserialize<MemberPinnedHolder>(bytes.ByteSpan);
        deserialized!.Number.Value.ShouldBe(9L);
        deserialized.Text.Value.ShouldBe("x");
        deserialized.AsText.Value.ShouldBe("hello");
    }

    [Fact]
    public void RoundTripUnionCollections()
    {
        var target = new UnionCollectionsHolder()
        {
            Items = [new StringOrInt(1), new StringOrInt(2)],
            ListItems = [new StringOrInt(3)],
            Map = new Dictionary<string, StringOrInt>() { ["k"] = new StringOrInt(4) },
            MaybeValue = new StringOrInt(5),
        };
        using var bytes = CsTomlSerializer.Serialize(target);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Items = [ 1, 2 ]");
        writer.AppendLine("ListItems = [ 3 ]");
        writer.AppendLine("MaybeValue = 5");
        writer.AppendLine("Map = {k = 4}");
        writer.Flush();
        bytes.ByteSpan.ToArray().ShouldBe(buffer.ToArray());

        var deserialized = CsTomlSerializer.Deserialize<UnionCollectionsHolder>(bytes.ByteSpan);
        deserialized!.Items![0].Value.ShouldBe(1);
        deserialized.Items[1].Value.ShouldBe(2);
        deserialized.ListItems![0].Value.ShouldBe(3);
        deserialized.Map!["k"].Value.ShouldBe(4);
        deserialized.MaybeValue!.Value.Value.ShouldBe(5);
    }

    [Fact]
    public void RoundTripStandaloneUnionWithManualRegistration()
    {
        global::CsToml.Generated.CsToml.Generator.Tests.StandaloneUnionFormatter.Register();
        // Register() is idempotent.
        global::CsToml.Generated.CsToml.Generator.Tests.StandaloneUnionFormatter.Register();

        var value = new StandaloneUnion(11L);
        using var bytes = CsTomlSerializer.SerializeValueType(value);
        Encoding.UTF8.GetString(bytes.ByteSpan).ShouldBe("11");

        var deserialized = CsTomlSerializer.DeserializeValueType<StandaloneUnion>(bytes.ByteSpan);
        deserialized.Value.ShouldBe(11L);
    }

    [Fact]
    public void RegistrationIsFirstWins()
    {
        TomlValueFormatterResolver.Register<FirstWinsUnion>(new FirstWinsMarkerFormatter());
        // The generated registration must silently lose against the earlier manual one.
        global::CsToml.Generated.CsToml.Generator.Tests.FirstWinsUnionFormatter.Register();

        var value = new FirstWinsUnion(1L);
        using var bytes = CsTomlSerializer.SerializeValueType(value);
        Encoding.UTF8.GetString(bytes.ByteSpan).ShouldBe("\"marker\"");
    }

    private sealed class FirstWinsMarkerFormatter : ITomlValueFormatter<FirstWinsUnion>
    {
        public FirstWinsUnion Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
            => new FirstWinsUnion(1L);

        public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, FirstWinsUnion target, CsTomlSerializerOptions options)
            where TBufferWriter : IBufferWriter<byte>
        {
            options.Resolver.GetFormatter<string>()!.Serialize(ref writer, "marker", options);
        }
    }
}
