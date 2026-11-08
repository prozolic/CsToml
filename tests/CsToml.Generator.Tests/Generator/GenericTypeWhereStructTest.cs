using CsToml.Formatter.Resolver;
using Shouldly;
using Utf8StringInterpolation;
using CsToml.Generator.Other;

namespace CsToml.Generator.Tests;

public class GenericTypeWhereStructTest
{
    [Test]
#if NET8_0
    // A failure here is a fail-fast that ends the whole test process, so the test must not run at all.
    [SkipOnNativeAot("Known gap: .NET 8 NativeAOT does not generate TomlSerializedObjectFormatter<GenericTypeWhereStruct<int>>.Serialize<TBufferWriter>, and the process fails fast.")]
#endif
    public void SerializeInt()
    {
        var intValue = new GenericTypeWhereStruct<int>()
        {
            Value = 123,
            NullableValue = 456
        };
        using var bytes = CsTomlSerializer.Serialize(intValue);
        var __ = CsTomlSerializer.Deserialize<GenericTypeWhereStruct<int>>(bytes.ByteSpan);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = 123");
        writer.AppendLine("NullableValue = 456");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Test]
    [SkipOnNativeAot("Known gap: as a type argument, SimpleStruct is resolved by reflection (TryRegister), and NativeAOT trims its Register method.")]
    public void Serialize()
    {
        {
            var value = new GenericTypeWhereStruct<SimpleStruct>()
            {
                Value = new SimpleStruct() { Value = "This is SimpleStruct" },
                NullableValue = new SimpleStruct() { Value = "This is Nullable SimpleStruct" }
            };
            using var bytes = CsTomlSerializer.Serialize(value);
            var __ = CsTomlSerializer.Deserialize<GenericTypeWhereStruct<SimpleStruct>>(bytes.ByteSpan);

            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value.Value = \"This is SimpleStruct\"");
            writer.AppendLine("NullableValue.Value = \"This is Nullable SimpleStruct\"");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);
        }
    }

    [Test]
    [SkipOnNativeAot("T is chosen by the caller, so the generated Register() cannot register ValueTuple<int, string>.")]
    public void SerializeValueTuple()
    {
        var intValue = new GenericTypeWhereStruct<ValueTuple<int, string>>()
        {
            Value = (10, "ten"),
            NullableValue = (11, "eleven")
        };
        using var bytes = CsTomlSerializer.Serialize(intValue);
        var __ = CsTomlSerializer.Deserialize<GenericTypeWhereStruct<ValueTuple<int, string>>>(bytes.ByteSpan);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = [ 10, \"ten\" ]");
        writer.AppendLine("NullableValue = [ 11, \"eleven\" ]");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Test]
    [SkipOnNativeAot("Known gap: as a type argument, SimpleStruct is resolved by reflection (TryRegister), and NativeAOT trims its Register method.")]
    public void SerializeWithHeaderOption()
    {
        var value = new GenericTypeWhereStruct<SimpleStruct>()
        {
            Value = new SimpleStruct() { Value = "This is SimpleStruct" },
            NullableValue = new SimpleStruct() { Value = "This is Nullable SimpleStruct" }
        };
        using var bytes = CsTomlSerializer.Serialize(value, options: Option.Header);
        var __ = CsTomlSerializer.Deserialize<GenericTypeWhereStruct<SimpleStruct>>(bytes.ByteSpan);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("[Value]");
        writer.AppendLine("Value = \"This is SimpleStruct\"");
        writer.AppendLine("[NullableValue]");
        writer.AppendLine("Value = \"This is Nullable SimpleStruct\"");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Test]
    [SkipOnNativeAot("Known gap: as a type argument, SimpleStruct is resolved by reflection (TryRegister), and NativeAOT trims its Register method.")]
    public void SerializeWithArrayHeaderOption()
    {
        var value = new GenericTypeWhereStruct<SimpleStruct>()
        {
            Value = new SimpleStruct() { Value = "This is SimpleStruct" },
            NullableValue = new SimpleStruct() { Value = "This is Nullable SimpleStruct" }
        };
        using var bytes = CsTomlSerializer.Serialize(value, options: Option.ArrayHeader);
        var __ = CsTomlSerializer.Deserialize<GenericTypeWhereStruct<SimpleStruct>>(bytes.ByteSpan);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value.Value = \"This is SimpleStruct\"");
        writer.AppendLine("NullableValue.Value = \"This is Nullable SimpleStruct\"");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Test]
    [SkipOnNativeAot("Known gap: as a type argument, SimpleStruct is resolved by reflection (TryRegister), and NativeAOT trims its Register method.")]
    public void SerializeWithHeaderAndArrayHeaderOption()
    {
        var value = new GenericTypeWhereStruct<SimpleStruct>()
        {
            Value = new SimpleStruct() { Value = "This is SimpleStruct" },
            NullableValue = new SimpleStruct() { Value = "This is Nullable SimpleStruct" }
        };
        using var bytes = CsTomlSerializer.Serialize(value, options: Option.HeaderAndArrayHeader);
        var __ = CsTomlSerializer.Deserialize<GenericTypeWhereStruct<SimpleStruct>>(bytes.ByteSpan);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("[Value]");
        writer.AppendLine("Value = \"This is SimpleStruct\"");
        writer.AppendLine("[NullableValue]");
        writer.AppendLine("Value = \"This is Nullable SimpleStruct\"");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Test]
    [SkipOnNativeAot("T is chosen by the caller, so the generated Register() cannot register ValueTuple<int, string>.")]
    public void DeserializeValueTuple()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = [ 10, \"ten\" ]");
        writer.AppendLine("NullableValue = [ 11, \"eleven\" ]");
        writer.Flush();

        var value = CsTomlSerializer.Deserialize<GenericTypeWhereStruct<ValueTuple<int, string>>>(buffer.WrittenSpan);
        value.Value.ShouldBe((10, "ten"));
        value.NullableValue.GetValueOrDefault().ShouldBe((11, "eleven"));
    }

    [Test]
#if NET8_0
    // A failure here is a fail-fast that ends the whole test process, so the test must not run at all.
    [SkipOnNativeAot("Known gap: .NET 8 NativeAOT does not generate TomlSerializedObjectFormatter<GenericTypeWhereStruct<int>>.Serialize<TBufferWriter>, and the process fails fast.")]
#endif
    public void DeserializeInt()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Value = 123");
        writer.AppendLine("NullableValue = 456");
        writer.Flush();

        var value = CsTomlSerializer.Deserialize<GenericTypeWhereStruct<int>>(buffer.WrittenSpan);
        value.Value.ShouldBe(123);
        value.NullableValue.ShouldBe(456);
    }

    [Test]
    [SkipOnNativeAot("Known gap: as a type argument, SimpleStruct is resolved by reflection (TryRegister), and NativeAOT trims its Register method.")]
    public void Deserialize()
    {
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("Value.Value = \"This is SimpleStruct\"");
            writer.AppendLine("NullableValue.Value = \"This is Nullable SimpleStruct\"");
            writer.Flush();

            var value = CsTomlSerializer.Deserialize<GenericTypeWhereStruct<SimpleStruct>>(buffer.WrittenSpan);
            value.Value.Value.ShouldBe("This is SimpleStruct");
            value.NullableValue.GetValueOrDefault().Value.ShouldBe("This is Nullable SimpleStruct");
        }
        {
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("[Value]");
            writer.AppendLine("Value = \"This is SimpleStruct\"");
            writer.AppendLine("[NullableValue]");
            writer.AppendLine("Value = \"This is Nullable SimpleStruct\"");
            writer.Flush();

            var value = CsTomlSerializer.Deserialize<GenericTypeWhereStruct<SimpleStruct>>(buffer.WrittenSpan);
            value.Value.Value.ShouldBe("This is SimpleStruct");
            value.NullableValue.GetValueOrDefault().Value.ShouldBe("This is Nullable SimpleStruct");
        }
    }

}
