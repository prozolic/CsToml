using CsToml.Formatter;
using CsToml.Utility;
using CsToml.Values;
using System.Buffers;
using System.Runtime.InteropServices;

namespace CsToml;

public partial class CsTomlSerializer : ICsTomlValueSerializer
{
    public static byte[] Serialize<TPackagePart>(ref TPackagePart target)
        where TPackagePart : ICsTomlPackagePart<TPackagePart>
    {
        var writer = new ArrayBufferWriter<byte>();
        Serialize(ref writer, ref target);
        return writer.WrittenSpan.ToArray();
    }

    public static void Serialize<TBufferWriter, TPackagePart>(ref TBufferWriter bufferWriter, ref TPackagePart target)
        where TBufferWriter : IBufferWriter<byte>
        where TPackagePart : ICsTomlPackagePart<TPackagePart>
    {
        TPackagePart.Serialize<TBufferWriter, CsTomlSerializer>(ref bufferWriter, ref target);
    }

    public static byte[] Serialize<TPackage>(TPackage? package)
        where TPackage : CsTomlPackage
    {
        var writer = new ArrayBufferWriter<byte>();
        Serialize(ref writer, package);
        return writer.WrittenSpan.ToArray();
    }

    public static void Serialize<TBufferWriter, TPackage>(ref TBufferWriter bufferWriter, TPackage? package)
        where TBufferWriter : IBufferWriter<byte>
        where TPackage : CsTomlPackage
    {
        var utf8Writer = new Utf8Writer(bufferWriter);
        package?.Serialize(ref utf8Writer);
    }

    #region ICsTomlValueSerializer

    static void ICsTomlValueSerializer.Serialize<TWriter>(ref TWriter writer, long value)
    {
        var utf8Writer = new Utf8Writer(writer);
        ValueFormatter.Serialize(ref utf8Writer, value);
    }

    static void ICsTomlValueSerializer.Serialize<TWriter>(ref TWriter writer, bool value)
    {
        var utf8Writer = new Utf8Writer(writer);
        ValueFormatter.Serialize(ref utf8Writer, value);
    }

    static void ICsTomlValueSerializer.Serialize<TWriter>(ref TWriter writer, double value)
    {
        var utf8Writer = new Utf8Writer(writer);
        ValueFormatter.Serialize(ref utf8Writer, value);
    }

    static void ICsTomlValueSerializer.Serialize<TWriter>(ref TWriter writer, DateTime value)
    {
        var utf8Writer = new Utf8Writer(writer);
        ValueFormatter.Serialize(ref utf8Writer, value);
    }

    static void ICsTomlValueSerializer.Serialize<TWriter>(ref TWriter writer, DateTimeOffset value)
    {
        var utf8Writer = new Utf8Writer(writer);
        ValueFormatter.Serialize(ref utf8Writer, value);
    }

    static void ICsTomlValueSerializer.Serialize<TWriter>(ref TWriter writer, DateOnly value)
    {
        var utf8Writer = new Utf8Writer(writer);
        ValueFormatter.Serialize(ref utf8Writer, value);
    }

    static void ICsTomlValueSerializer.Serialize<TWriter>(ref TWriter writer, TimeOnly value)
    {
        var utf8Writer = new Utf8Writer(writer);
        ValueFormatter.Serialize(ref utf8Writer, value);
    }

    static void ICsTomlValueSerializer.Serialize<TWriter>(ref TWriter writer, ReadOnlySpan<char> value)
    {
        var cstomlStr = CsTomlString.Parse(value);
        var utf8Writer = new Utf8Writer(writer);
        cstomlStr.ToTomlString(ref utf8Writer);
    }

    static void ICsTomlValueSerializer.SerializeDynamic<TWriter>(ref TWriter writer, dynamic value)
    {
        if (value == null) return;
        SerializeDynamic<TWriter, CsTomlSerializer>(ref writer, value);
    }

    static void ICsTomlValueSerializer.SerializeKey<TWriter>(ref TWriter writer, ReadOnlySpan<char> value)
    {
        var cstomlStr = CsTomlString.ParseKey(value);
        var utf8Writer = new Utf8Writer(writer);
        cstomlStr.ToTomlString(ref utf8Writer);
    }

    static void ICsTomlValueSerializer.Serialize<TWriter, TArrayItem>(ref TWriter writer, IEnumerable<TArrayItem> value)
    {
        if (value is List<TArrayItem> list)
        {
            var arr = new CsTomlArray();
            var listSpan = CollectionsMarshal.AsSpan(list);
            for (int i = 0; i < listSpan.Length; i++)
            {
                object? item = listSpan[i]!;
                if (CsTomlValueResolver.TryResolve(ref item, out var v))
                {
                    arr.Add(v!);
                }
            }
            var utf8Writer = new Utf8Writer(writer);
            arr.ToTomlString(ref utf8Writer);
        }
        else
        {
            var arr = new CsTomlArray();
            foreach (var item in value)
            {
                object? i = item;
                if (CsTomlValueResolver.TryResolve(ref i, out var v))
                {
                    arr.Add(v!);
                }
            }
            var utf8Writer = new Utf8Writer(writer);
            arr.ToTomlString(ref utf8Writer);
        }
    }

    private static void SerializeDynamic<TWriter, TSerializer>(ref TWriter writer, dynamic value)
        where TWriter : IBufferWriter<byte>
        where TSerializer : ICsTomlValueSerializer
    {
        switch (value.GetType())
        {
            case var t when t == typeof(bool):
                TSerializer.Serialize(ref writer, (bool)value);
                return;
            case var t when t == typeof(byte):
                TSerializer.Serialize(ref writer, (long)value);
                return;
            case var t when t == typeof(sbyte):
                TSerializer.Serialize(ref writer, (long)value);
                return;
            case var t when t == typeof(int):
                TSerializer.Serialize(ref writer, (long)value);
                return;
            case var t when t == typeof(uint):
                TSerializer.Serialize(ref writer, (long)value);
                return;
            case var t when t == typeof(long):
                TSerializer.Serialize(ref writer, (long)value);
                return;
            case var t when t == typeof(ulong):
                TSerializer.Serialize(ref writer, (long)value);
                return;
            case var t when t == typeof(short):
                TSerializer.Serialize(ref writer, (long)value);
                return;
            case var t when t == typeof(ushort):
                TSerializer.Serialize(ref writer, (long)value);
                return;
            case var t when t == typeof(double):
                TSerializer.Serialize(ref writer, (double)value);
                return;
            case var t when t == typeof(DateTime):
                TSerializer.Serialize(ref writer, (DateTime)value);
                return;
            case var t when t == typeof(DateTimeOffset):
                TSerializer.Serialize(ref writer, (DateTimeOffset)value);
                return;
            case var t when t == typeof(DateOnly):
                TSerializer.Serialize(ref writer, (DateOnly)value);
                return;
            case var t when t == typeof(TimeOnly):
                TSerializer.Serialize(ref writer, (TimeOnly)value);
                return;
            case var t when t == typeof(ReadOnlySpan<char>):
                TSerializer.Serialize(ref writer, (ReadOnlySpan<char>)value);
                return;
            case var t when t == typeof(string):
                TSerializer.Serialize(ref writer, ((string)value).AsSpan());
                return;
        }
    }

    #endregion

}