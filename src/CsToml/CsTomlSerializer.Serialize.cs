using CsToml.Formatter;
using CsToml.Utility;
using CsToml.Values;
using System.Buffers;
using System.Runtime.InteropServices;

namespace CsToml;

public partial class CsTomlSerializer : ICsTomlValueSerializer
{
    public static ByteMemoryResult Serialize<TPackagePart>(ref TPackagePart target)
        where TPackagePart : ICsTomlPackagePart<TPackagePart>
    {
        var writer = new ArrayPoolBufferWriter<byte>();
        using var _ = writer;
        Serialize(ref writer, ref target);
        return ByteMemoryResult.Create(writer);
    }

    public static void Serialize<TBufferWriter, TPackagePart>(ref TBufferWriter bufferWriter, ref TPackagePart target)
        where TBufferWriter : IBufferWriter<byte>
        where TPackagePart : ICsTomlPackagePart<TPackagePart>
    {
        TPackagePart.Serialize<TBufferWriter, CsTomlSerializer>(ref bufferWriter, ref target);
    }

    public static ByteMemoryResult Serialize<TPackage>(TPackage? package)
        where TPackage : CsTomlPackage
    {
        var writer = new ArrayPoolBufferWriter<byte>();
        using var _ = writer;
        Serialize(ref writer, package);
        return ByteMemoryResult.Create(writer);
    }

    public static void Serialize<TBufferWriter, TPackage>(ref TBufferWriter bufferWriter, TPackage? package)
        where TBufferWriter : IBufferWriter<byte>
        where TPackage : CsTomlPackage
    {
        var utf8Writer = new Utf8Writer<TBufferWriter>(ref bufferWriter);
        package?.Serialize(ref utf8Writer);
    }

    #region ICsTomlValueSerializer

    static void ICsTomlValueSerializer.Serialize<TBufferWriter>(ref TBufferWriter writer, long value)
    {
        var utf8Writer = new Utf8Writer<TBufferWriter>(ref writer);
        ValueFormatter.Serialize(ref utf8Writer, value);
    }

    static void ICsTomlValueSerializer.Serialize<TBufferWriter>(ref TBufferWriter writer, bool value)
    {
        var utf8Writer = new Utf8Writer<TBufferWriter>(ref writer);
        ValueFormatter.Serialize(ref utf8Writer, value);
    }

    static void ICsTomlValueSerializer.Serialize<TBufferWriter>(ref TBufferWriter writer, double value)
    {
        var utf8Writer = new Utf8Writer<TBufferWriter>(ref writer);
        ValueFormatter.Serialize(ref utf8Writer, value);
    }

    static void ICsTomlValueSerializer.Serialize<TBufferWriter>(ref TBufferWriter writer, DateTime value)
    {
        var utf8Writer = new Utf8Writer<TBufferWriter>(ref writer);
        ValueFormatter.Serialize(ref utf8Writer, value);
    }

    static void ICsTomlValueSerializer.Serialize<TBufferWriter>(ref TBufferWriter writer, DateTimeOffset value)
    {
        var utf8Writer = new Utf8Writer<TBufferWriter>(ref writer);
        ValueFormatter.Serialize(ref utf8Writer, value);
    }

    static void ICsTomlValueSerializer.Serialize<TBufferWriter>(ref TBufferWriter writer, DateOnly value)
    {
        var utf8Writer = new Utf8Writer<TBufferWriter>(ref writer);
        ValueFormatter.Serialize(ref utf8Writer, value);
    }

    static void ICsTomlValueSerializer.Serialize<TBufferWriter>(ref TBufferWriter writer, TimeOnly value)
    {
        var utf8Writer = new Utf8Writer<TBufferWriter>(ref writer);
        ValueFormatter.Serialize(ref utf8Writer, value);
    }

    static void ICsTomlValueSerializer.Serialize<TBufferWriter>(ref TBufferWriter writer, ReadOnlySpan<char> value)
    {
        var cstomlStr = CsTomlString.Parse(value);
        var utf8Writer = new Utf8Writer<TBufferWriter>(ref writer);
        cstomlStr.ToTomlString(ref utf8Writer);
    }

    static void ICsTomlValueSerializer.SerializeDynamic<TBufferWriter>(ref TBufferWriter writer, dynamic value)
    {
        if (value == null) return;

        var utf8Writer = new Utf8Writer<TBufferWriter>(ref writer);
        switch (value.GetType())
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
                var cstomlStr = CsTomlString.Parse((string)value);
                cstomlStr.ToTomlString(ref utf8Writer);
                return;
        }
    }

    static void ICsTomlValueSerializer.SerializeKey<TBufferWriter>(ref TBufferWriter writer, ReadOnlySpan<char> value)
    {
        var cstomlStr = CsTomlString.ParseKey(value);
        var utf8Writer = new Utf8Writer<TBufferWriter>(ref writer);
        cstomlStr.ToTomlString(ref utf8Writer);
    }

    static void ICsTomlValueSerializer.Serialize<TBufferWriter, TArrayItem>(ref TBufferWriter writer, IEnumerable<TArrayItem> value)
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
            var utf8Writer = new Utf8Writer<TBufferWriter>(ref writer);
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
            var utf8Writer = new Utf8Writer<TBufferWriter>(ref writer);
            arr.ToTomlString(ref utf8Writer);
        }
    }

    #endregion

}