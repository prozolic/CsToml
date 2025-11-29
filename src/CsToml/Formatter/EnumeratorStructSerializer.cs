using System;
using System.Buffers;

namespace CsToml.Formatter;

internal struct EnumeratorStructSerializer<TElement, TEnumerator>(int Count, TEnumerator EnumeratorStruct)
    where TEnumerator : struct, IEnumerator<TElement>
{
    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.BeginArray();
        if (Count == 0)
        {
            writer.EndArray();
            return;
        }

        var en = EnumeratorStruct;
        en.MoveNext();

        var formatter = options.Resolver.GetFormatter<TElement>()!;
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
        writer.WriteSpace();
        writer.EndArray();
    }

    public bool TrySerializeTomlArrayHeaderStyle<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, ReadOnlySpan<byte> header, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (!writer.IsRoot || Count == 0)
        {
            return false;
        }

        var formatter = options.Resolver.GetFormatter<TElement>()!;
        var en = EnumeratorStruct;
        while (en.MoveNext())
        {
            writer.BeginArrayOfTablesHeader();
            writer.WriteKey(header);
            writer.EndArrayOfTablesHeader();
            writer.WriteNewLine();
            writer.BeginCurrentState(TomlValueState.ArrayOfTableForMulitiLine);
            formatter.Serialize(ref writer, en.Current, options);
            writer.EndCurrentState();
            writer.EndKeyValue(false);
        }

        // Return true if serialized in header style.
        return true;
    }
}
