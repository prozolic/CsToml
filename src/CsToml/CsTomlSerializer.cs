using CsToml.Error;
using CsToml.Formatter;
using CsToml.Utility;
using CsToml.Values;
using System.Buffers;

namespace CsToml;

public sealed class CsTomlSerializer : ITomlValueSerializer
{
    private CsTomlSerializer() { }

    public static T Deserialize<T>(ReadOnlySpan<byte> tomlText, CsTomlSerializerOptions? options = null)
    where T : ITomlSerializedObject<T>
    {
        options ??= CsTomlSerializerOptions.Default;

        return T.Deserialize<CsTomlSerializer>(tomlText, options);
    }

    public static T Deserialize<T>(in ReadOnlySequence<byte> tomlTextSequence, CsTomlSerializerOptions? options = null)
        where T : ITomlSerializedObject<T>
    {
        options ??= CsTomlSerializerOptions.Default;
        return T.Deserialize<CsTomlSerializer>(tomlTextSequence, options);
    }


    public static ByteMemoryResult Serialize<T>(T? target, CsTomlSerializerOptions? options = null)
        where T : ITomlSerializedObject<T>
    {
        var bufferWriter = RecycleArrayPoolBufferWriter<byte>.Rent();
        try
        {
            Serialize(ref bufferWriter, target, options);
            return ByteMemoryResult.Create(bufferWriter);
        }
        finally
        {
            RecycleArrayPoolBufferWriter<byte>.Return(bufferWriter);
        }
    }

    public static void Serialize<TBufferWriter, T>(ref TBufferWriter bufferWriter, T? target, CsTomlSerializerOptions? options = null)
        where TBufferWriter : IBufferWriter<byte>
        where T : ITomlSerializedObject<T>
    {
        options ??= CsTomlSerializerOptions.Default;
        try
        {
            T.Serialize<TBufferWriter, CsTomlSerializer>(ref bufferWriter, target, options);
        }
        catch (CsTomlException cte)
        {
            throw new CsTomlSerializeException([cte]);
        }
    }

    #region ICsTomlValueSerializer

    static void ITomlValueSerializer.Serialize<TBufferWriter>(ref TBufferWriter writer, long value)
    {
        var utf8Writer = new Utf8Writer<TBufferWriter>(ref writer);
        ValueFormatter.Serialize(ref utf8Writer, value);
    }

    static void ITomlValueSerializer.Serialize<TBufferWriter>(ref TBufferWriter writer, bool value)
    {
        var utf8Writer = new Utf8Writer<TBufferWriter>(ref writer);
        ValueFormatter.Serialize(ref utf8Writer, value);
    }

    static void ITomlValueSerializer.Serialize<TBufferWriter>(ref TBufferWriter writer, double value)
    {
        var utf8Writer = new Utf8Writer<TBufferWriter>(ref writer);
        ValueFormatter.Serialize(ref utf8Writer, value);
    }

    static void ITomlValueSerializer.Serialize<TBufferWriter>(ref TBufferWriter writer, DateTime value)
    {
        var utf8Writer = new Utf8Writer<TBufferWriter>(ref writer);
        ValueFormatter.Serialize(ref utf8Writer, value);
    }

    static void ITomlValueSerializer.Serialize<TBufferWriter>(ref TBufferWriter writer, DateTimeOffset value)
    {
        var utf8Writer = new Utf8Writer<TBufferWriter>(ref writer);
        ValueFormatter.Serialize(ref utf8Writer, value);
    }

    static void ITomlValueSerializer.Serialize<TBufferWriter>(ref TBufferWriter writer, DateOnly value)
    {
        var utf8Writer = new Utf8Writer<TBufferWriter>(ref writer);
        ValueFormatter.Serialize(ref utf8Writer, value);
    }

    static void ITomlValueSerializer.Serialize<TBufferWriter>(ref TBufferWriter writer, TimeOnly value)
    {
        var utf8Writer = new Utf8Writer<TBufferWriter>(ref writer);
        ValueFormatter.Serialize(ref utf8Writer, value);
    }

    static void ITomlValueSerializer.Serialize<TBufferWriter>(ref TBufferWriter writer, ReadOnlySpan<char> value)
    {
        var cstomlStr = TomlString.Parse(value);
        var utf8Writer = new Utf8Writer<TBufferWriter>(ref writer);
        cstomlStr.ToTomlString(ref utf8Writer);
    }

    static void ITomlValueSerializer.SerializeDynamic<TBufferWriter>(ref TBufferWriter writer, dynamic value)
    {
        var utf8Writer = new Utf8Writer<TBufferWriter>(ref writer);
        if (value == null)
        {
            ValueFormatter.Serialize(ref utf8Writer, 0);
            return;
        }

        switch (value!.GetType())
        {
            case var t when t == typeof(bool):
                ValueFormatter.Serialize(ref utf8Writer, (int)(value));
                return;
            case var t when t == typeof(byte):
                ValueFormatter.Serialize(ref utf8Writer, (byte)(value));
                return;
            case var t2 when t2 == typeof(sbyte):
                ValueFormatter.Serialize(ref utf8Writer, (sbyte)(value));
                return;
            case var t3 when t3 == typeof(int):
                ValueFormatter.Serialize(ref utf8Writer, (int)(value));
                return;
            case var t4 when t4 == typeof(uint):
                ValueFormatter.Serialize(ref utf8Writer, (uint)(value));
                return;
            case var t5 when t5 == typeof(long):
                ValueFormatter.Serialize(ref utf8Writer, (long)(value));
                return;
            case var t6 when t6 == typeof(ulong):
                ValueFormatter.Serialize(ref utf8Writer, (ulong)(value));
                return;
            case var t7 when t7 == typeof(short):
                ValueFormatter.Serialize(ref utf8Writer, (short)(value));
                return;
            case var t8 when t8 == typeof(ushort):
                ValueFormatter.Serialize(ref utf8Writer, (ushort)(value));
                return;
            case var t when t == typeof(double):
                ValueFormatter.Serialize(ref utf8Writer, (double)(value));
                return;
            case var t when t == typeof(DateTime):
                ValueFormatter.Serialize(ref utf8Writer, (DateTime)(value));
                return;
            case var t when t == typeof(DateTimeOffset):
                ValueFormatter.Serialize(ref utf8Writer, (DateTimeOffset)(value));
                return;
            case var t when t == typeof(DateOnly):
                ValueFormatter.Serialize(ref utf8Writer, (DateOnly)(value));
                return;
            case var t when t == typeof(TimeOnly):
                ValueFormatter.Serialize(ref utf8Writer, (TimeOnly)(value));
                return;
            case var t when t == typeof(string):
                var cstomlStr = TomlString.Parse((string)value);
                cstomlStr.ToTomlString(ref utf8Writer);
                return;
            default:
                ValueFormatter.Serialize(ref utf8Writer, 0);
                return;
        }
    }

    static void ITomlValueSerializer.Serialize<TBufferWriter, TArrayItem>(ref TBufferWriter writer, IEnumerable<TArrayItem> value)
    {
        var utf8Writer = new Utf8Writer<TBufferWriter>(ref writer);
        TomlArray.Parse(value).ToTomlString(ref utf8Writer);
    }

    static void ITomlValueSerializer.SerializeKey<TBufferWriter>(ref TBufferWriter writer, ReadOnlySpan<char> value)
    {
        var cstomlStr = TomlString.ParseKey(value);
        var utf8Writer = new Utf8Writer<TBufferWriter>(ref writer);
        cstomlStr.ToTomlString(ref utf8Writer);
    }

    static void ITomlValueSerializer.SerializeTableHeader<TBufferWriter>(ref TBufferWriter writer, ReadOnlySpan<char> value)
    {
        var utf8Writer = new Utf8Writer<TBufferWriter>(ref writer);
        var csTomlWriter = new CsTomlWriter<TBufferWriter>(ref utf8Writer);
        var str = TomlString.ParseKey(value);
        csTomlWriter.WriteTableHeader([str]);
    }

    static void ITomlValueSerializer.SerializeArrayOfTablesHeader<TBufferWriter>(ref TBufferWriter writer, ReadOnlySpan<char> value)
    {
        var utf8Writer = new Utf8Writer<TBufferWriter>(ref writer);
        var csTomlWriter = new CsTomlWriter<TBufferWriter>(ref utf8Writer);
        var str = TomlString.ParseKey(value);
        csTomlWriter.WriteArrayOfTablesHeader([str]);
    }

    static void ITomlValueSerializer.SerializeNewLine<TBufferWriter>(ref TBufferWriter writer)
    {
        var utfWriter = new Utf8Writer<TBufferWriter>(ref writer);
        var csTomlWriter = new CsTomlWriter<TBufferWriter>(ref utfWriter);
        csTomlWriter.WriteNewLine();
    }

    static void ITomlValueSerializer.SerializeEqual<TBufferWriter>(ref TBufferWriter writer)
    {
        var utfWriter = new Utf8Writer<TBufferWriter>(ref writer);
        var csTomlWriter = new CsTomlWriter<TBufferWriter>(ref utfWriter);
        csTomlWriter.WriteEquals();
    }

    static void ITomlValueSerializer.Serialize<TBufferWriter>(ref TBufferWriter writer, IEnumerable<bool> value)
    {
        var utf8Writer = new Utf8Writer<TBufferWriter>(ref writer);
        TomlArray.Parse(value).ToTomlString(ref utf8Writer);
    }

    static void ITomlValueSerializer.Serialize<TBufferWriter>(ref TBufferWriter writer, IEnumerable<byte> value)
    {
        var utf8Writer = new Utf8Writer<TBufferWriter>(ref writer);
        TomlArray.Parse(value).ToTomlString(ref utf8Writer);
    }

    static void ITomlValueSerializer.Serialize<TBufferWriter>(ref TBufferWriter writer, IEnumerable<sbyte> value)
    {
        var utf8Writer = new Utf8Writer<TBufferWriter>(ref writer);
        TomlArray.Parse(value).ToTomlString(ref utf8Writer);
    }

    static void ITomlValueSerializer.Serialize<TBufferWriter>(ref TBufferWriter writer, IEnumerable<int> value)
    {
        var utf8Writer = new Utf8Writer<TBufferWriter>(ref writer);
        TomlArray.Parse(value).ToTomlString(ref utf8Writer);
    }

    static void ITomlValueSerializer.Serialize<TBufferWriter>(ref TBufferWriter writer, IEnumerable<uint> value)
    {
        var utf8Writer = new Utf8Writer<TBufferWriter>(ref writer);
        TomlArray.Parse(value).ToTomlString(ref utf8Writer);
    }

    static void ITomlValueSerializer.Serialize<TBufferWriter>(ref TBufferWriter writer, IEnumerable<long> value)
    {
        var utf8Writer = new Utf8Writer<TBufferWriter>(ref writer);
        TomlArray.Parse(value).ToTomlString(ref utf8Writer);
    }

    static void ITomlValueSerializer.Serialize<TBufferWriter>(ref TBufferWriter writer, IEnumerable<ulong> value)
    {
        var utf8Writer = new Utf8Writer<TBufferWriter>(ref writer);
        TomlArray.Parse(value).ToTomlString(ref utf8Writer);
    }

    static void ITomlValueSerializer.Serialize<TBufferWriter>(ref TBufferWriter writer, IEnumerable<short> value)
    {
        var utf8Writer = new Utf8Writer<TBufferWriter>(ref writer);
        TomlArray.Parse(value).ToTomlString(ref utf8Writer);
    }

    static void ITomlValueSerializer.Serialize<TBufferWriter>(ref TBufferWriter writer, IEnumerable<ushort> value)
    {
        var utf8Writer = new Utf8Writer<TBufferWriter>(ref writer);
        TomlArray.Parse(value).ToTomlString(ref utf8Writer);
    }

    static void ITomlValueSerializer.Serialize<TBufferWriter>(ref TBufferWriter writer, IEnumerable<double> value)
    {
        var utf8Writer = new Utf8Writer<TBufferWriter>(ref writer);
        TomlArray.Parse(value).ToTomlString(ref utf8Writer);
    }

    static void ITomlValueSerializer.Serialize<TBufferWriter>(ref TBufferWriter writer, IEnumerable<DateTime> value)
    {
        var utf8Writer = new Utf8Writer<TBufferWriter>(ref writer);
        TomlArray.Parse(value).ToTomlString(ref utf8Writer);
    }

    static void ITomlValueSerializer.Serialize<TBufferWriter>(ref TBufferWriter writer, IEnumerable<DateTimeOffset> value)
    {
        var utf8Writer = new Utf8Writer<TBufferWriter>(ref writer);
        TomlArray.Parse(value).ToTomlString(ref utf8Writer);
    }

    static void ITomlValueSerializer.Serialize<TBufferWriter>(ref TBufferWriter writer, IEnumerable<DateOnly> value)
    {
        var utf8Writer = new Utf8Writer<TBufferWriter>(ref writer);
        TomlArray.Parse(value).ToTomlString(ref utf8Writer);
    }

    static void ITomlValueSerializer.Serialize<TBufferWriter>(ref TBufferWriter writer, IEnumerable<TimeOnly> value)
    {
        var utf8Writer = new Utf8Writer<TBufferWriter>(ref writer);
        TomlArray.Parse(value).ToTomlString(ref utf8Writer);
    }

    static void ITomlValueSerializer.Serialize<TBufferWriter>(ref TBufferWriter writer, IEnumerable<string> value)
    {
        var utf8Writer = new Utf8Writer<TBufferWriter>(ref writer);
        TomlArray.Parse(value).ToTomlString(ref utf8Writer);
    }

    static void ITomlValueSerializer.SerializeAnyByte<TBufferWriter>(ref TBufferWriter writer, ReadOnlySpan<byte> value)
    {
        var utf8Writer = new Utf8Writer<TBufferWriter>(ref writer);
        utf8Writer.Write(value);
    }

    #endregion

}

