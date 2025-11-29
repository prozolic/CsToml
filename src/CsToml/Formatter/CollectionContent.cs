using CsToml.Extension;
using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CsToml.Formatter;

internal struct CollectionContent(object collection)
{
    public object Collection = collection;
}

internal static class IEnumerableSerializer<T>
{
    public static void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, CollectionContent content, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        var enumerable = Unsafe.As<IEnumerable<T>>(content.Collection);
        if (enumerable.TryGetNonEnumeratedCount2(out var count) && count == 0)
        {
            writer.BeginArray();
            writer.EndArray();
            return;
        }

        var formatter = options.Resolver.GetFormatter<T>()!;
        writer.BeginArray();
        using (var en = enumerable.GetEnumerator())
        {
            if (!en.MoveNext())
            {
                writer.EndArray();
                return;
            }

            formatter.Serialize(ref writer, en.Current!, options);
            if (!en.MoveNext())
            {
                writer.WriteSpace();
                writer.EndArray();
                return;
            }

            do
            {
                writer.Write(TomlCodes.Symbol.COMMA);
                writer.WriteSpace();
                formatter.Serialize(ref writer, en.Current!, options);

            } while (en.MoveNext());
        }
        writer.WriteSpace();
        writer.EndArray();
    }
    public static bool TrySerializeTomlArrayHeaderStyle<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ReadOnlySpan<byte> header, CollectionContent content, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        var enumerable = Unsafe.As<IEnumerable<T>>(content.Collection);
        if (enumerable.TryGetNonEnumeratedCount2(out var count) && count == 0)
        {
            // Header-style table arrays must have at least one element
            // If there are 0 elements, return false to serialize in inline table format.
            return false;
        }

        var formatter = options.Resolver.GetFormatter<T>()!;
        using var en = enumerable.GetEnumerator();
        if (!en.MoveNext())
        {
            return false;
        }

        do
        {
            writer.BeginArrayOfTablesHeader();
            writer.WriteKey(header);
            writer.EndArrayOfTablesHeader();
            writer.WriteNewLine();
            writer.BeginCurrentState(TomlValueState.ArrayOfTableForMulitiLine);
            formatter.Serialize(ref writer, en.Current!, options);
            writer.EndCurrentState();
            writer.EndKeyValue(false);

        } while (en.MoveNext());

        // Return true if serialized in header style.
        return true;
    }
}

internal static class ArraySerializer<T>
{
    public static void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, CollectionContent content, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        var array = Unsafe.As<T[]>(content.Collection);
        writer.BeginArray();
        if (array.Length == 0)
        {
            writer.EndArray();
            return;
        }

        var targetSpan = array.AsSpan();
        var formatter = options.Resolver.GetFormatter<T>()!;
        formatter.Serialize(ref writer, targetSpan[0], options);
        if (targetSpan.Length == 1)
        {
            writer.WriteSpace();
            writer.EndArray();
            return;
        }

        for (int i = 1; i < targetSpan.Length; i++)
        {
            writer.Write(TomlCodes.Symbol.COMMA);
            writer.WriteSpace();
            formatter.Serialize(ref writer, targetSpan[i], options);
        }
        writer.WriteSpace();
        writer.EndArray();
    }

    public static bool TrySerializeTomlArrayHeaderStyle<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ReadOnlySpan<byte> header, CollectionContent content, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (!writer.IsRoot)
        {
            return false;
        }

        var array = Unsafe.As<T[]>(content.Collection);
        var arraySpan = array.AsSpan();
        if (arraySpan.Length == 0)
        {
            return false;
        }

        var formatter = options.Resolver.GetFormatter<T>()!;
        for (int i = 0; i < arraySpan.Length; i++)
        {
            writer.BeginArrayOfTablesHeader();
            writer.WriteKey(header);
            writer.EndArrayOfTablesHeader();
            writer.WriteNewLine();
            writer.BeginCurrentState(TomlValueState.ArrayOfTableForMulitiLine);
            formatter.Serialize(ref writer, arraySpan[i], options);
            writer.EndCurrentState();
            writer.EndKeyValue(false);
        }

        return true;
    }
}

internal static class ListSerializer<T>
{
    public static void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, CollectionContent content, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        var list = Unsafe.As<List<T>>(content.Collection);

        writer.BeginArray();
        if (list.Count == 0)
        {
            writer.EndArray();
            return;
        }

        var targetSpan = CollectionsMarshal.AsSpan(list);
        var formatter = options.Resolver.GetFormatter<T>()!;
        formatter.Serialize(ref writer, targetSpan[0], options);
        if (targetSpan.Length == 1)
        {
            writer.WriteSpace();
            writer.EndArray();
            return;
        }

        for (int i = 1; i < targetSpan.Length; i++)
        {
            writer.Write(TomlCodes.Symbol.COMMA);
            writer.WriteSpace();
            formatter.Serialize(ref writer, targetSpan[i], options);
        }
        writer.WriteSpace();
        writer.EndArray();
    }

    public static bool TrySerializeTomlArrayHeaderStyle<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ReadOnlySpan<byte> header, CollectionContent content, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (!writer.IsRoot)
        {
            return false;
        }

        var list = Unsafe.As<List<T>>(content.Collection);
        var listSpan = CollectionsMarshal.AsSpan(list);
        if (listSpan.Length == 0)
        {
            return false;
        }

        var formatter = options.Resolver.GetFormatter<T>()!;
        for (int i = 0; i < listSpan.Length; i++)
        {
            writer.BeginArrayOfTablesHeader();
            writer.WriteKey(header);
            writer.EndArrayOfTablesHeader();
            writer.WriteNewLine();
            writer.BeginCurrentState(TomlValueState.ArrayOfTableForMulitiLine);
            formatter.Serialize(ref writer, listSpan[i], options);
            writer.EndCurrentState();
            writer.EndKeyValue(false);
        }

        return true;
    }
}

internal static class IListSerializer<T> 
{
    public static void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, CollectionContent content, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        var ilist = Unsafe.As<IList<T>>(content.Collection);

        writer.BeginArray();
        if (ilist.Count == 0)
        {
            writer.EndArray();
            return;
        }

        var formatter = options.Resolver.GetFormatter<T>()!;
        formatter.Serialize(ref writer, ilist[0], options);
        if (ilist.Count == 1)
        {
            writer.WriteSpace();
            writer.EndArray();
            return;
        }

        for (int i = 1; i < ilist.Count; i++)
        {
            writer.Write(TomlCodes.Symbol.COMMA);
            writer.WriteSpace();
            formatter.Serialize(ref writer, ilist[i], options);
        }
        writer.WriteSpace();
        writer.EndArray();
    }

    public static bool TrySerializeTomlArrayHeaderStyle<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ReadOnlySpan<byte> header, CollectionContent content, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (!writer.IsRoot)
        {
            return false;
        }

        var iList = Unsafe.As<IList<T>>(content.Collection);
        if (iList.Count == 0)
        {
            return false;
        }

        var formatter = options.Resolver.GetFormatter<T>()!;
        for (int i = 0; i < iList.Count; i++)
        {
            writer.BeginArrayOfTablesHeader();
            writer.WriteKey(header);
            writer.EndArrayOfTablesHeader();
            writer.WriteNewLine();
            writer.BeginCurrentState(TomlValueState.ArrayOfTableForMulitiLine);
            formatter.Serialize(ref writer, iList[i], options);
            writer.EndCurrentState();
            writer.EndKeyValue(false);
        }

        return true;
    }
}

internal static class IReadOnlyListSerializer<T>
{
    public static void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, CollectionContent content, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        var iReadonlyList = Unsafe.As<IReadOnlyList<T>>(content.Collection);

        writer.BeginArray();
        if (iReadonlyList.Count == 0)
        {
            writer.EndArray();
            return;
        }

        var formatter = options.Resolver.GetFormatter<T>()!;
        formatter.Serialize(ref writer, iReadonlyList[0], options);
        if (iReadonlyList.Count == 1)
        {
            writer.WriteSpace();
            writer.EndArray();
            return;
        }

        for (int i = 1; i < iReadonlyList.Count; i++)
        {
            writer.Write(TomlCodes.Symbol.COMMA);
            writer.WriteSpace();
            formatter.Serialize(ref writer, iReadonlyList[i], options);
        }
        writer.WriteSpace();
        writer.EndArray();
    }

    public static bool TrySerializeTomlArrayHeaderStyle<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ReadOnlySpan<byte> header, CollectionContent content, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (!writer.IsRoot)
        {
            return false;
        }

        var iReadonlyList = Unsafe.As<IReadOnlyList<T>>(content.Collection);
        if (iReadonlyList.Count == 0)
        {
            return false;
        }

        var formatter = options.Resolver.GetFormatter<T>()!;
        for (int i = 0; i < iReadonlyList.Count; i++)
        {
            writer.BeginArrayOfTablesHeader();
            writer.WriteKey(header);
            writer.EndArrayOfTablesHeader();
            writer.WriteNewLine();
            writer.BeginCurrentState(TomlValueState.ArrayOfTableForMulitiLine);
            formatter.Serialize(ref writer, iReadonlyList[i], options);
            writer.EndCurrentState();
            writer.EndKeyValue(false);
        }

        return true;
    }
}
