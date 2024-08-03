using CsToml.Utility;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CsToml.Values;

[DebuggerDisplay("CsTomlInlineTable : {RootNode}")]
internal partial class CsTomlInlineTable : CsTomlValue
{
    private readonly CsTomlTable inlineTable = new CsTomlTable();

    public override bool HasValue => true;

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    internal CsTomlTableNode RootNode => inlineTable.RootNode;

    public CsTomlInlineTable() : base() 
    {}

    internal CsTomlTableNode AddKeyValue(ReadOnlySpan<CsTomlDotKey> keyArray, CsTomlValue value, CsTomlTableNode? searchRootNode)
        => inlineTable.AddKeyValue(keyArray, value, searchRootNode, []);

    internal override bool ToTomlString<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer)
    {
        writer.Write(CsTomlSyntax.Symbol.LEFTBRACES);
        var csTomlWriter = new CsTomlWriter<TBufferWriter>(ref writer);
        csTomlWriter.WriteSpace();

        var keys = new List<CsTomlDotKey>();
        ToTomlStringCore(ref csTomlWriter, RootNode, keys);

        csTomlWriter.WriteSpace();
        writer.Write(CsTomlSyntax.Symbol.RIGHTBRACES);
        return false;
    }

    private void ToTomlStringCore<TBufferWriter>(ref CsTomlWriter<TBufferWriter> writer, CsTomlTableNode parentNode, List<CsTomlDotKey> keys)
        where TBufferWriter : IBufferWriter<byte>
    {
        var count = 0;

        foreach (var (key, childNode) in parentNode.KeyValuePairs)
        {
            if (childNode.IsGroupingProperty)
            {
                keys.Add(key);
                ToTomlStringCore(ref writer, childNode, keys);
                count++;

                if (count != parentNode.NodeCount)
                {
                    writer.WriteComma();
                    writer.WriteSpace();
                }
                continue;
            }
            else
            {
                var keysSpan = CollectionsMarshal.AsSpan(keys);
                if (keysSpan.Length > 0)
                {
                    for (var i = 0; i < keysSpan.Length; i++)
                    {
                        writer.WriterKey(keysSpan[i], true);
                    }
                }
                writer.WriteKeyValue(key, childNode.Value!);
                count++;

                if (count != parentNode.NodeCount)
                {
                    writer.WriteComma();
                    writer.WriteSpace();
                }
            }
        }

        keys.Clear(); // clear subkey
    }

    public override bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
        var tableFormat = $"TOML Inline Table[{inlineTable.RootNode.NodeCount}]";
        if (tableFormat.TryCopyTo(destination))
        {
            charsWritten = tableFormat.Length;
            return true;
        }
        charsWritten = 0;
        return false;
    }

    public override string ToString(string? format, IFormatProvider? formatProvider)
        => ToString();

    public override bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
        if (!"TOML Inline Table["u8.TryCopyTo(utf8Destination))
        {
            bytesWritten = 0;
            return false;
        }
        var written = 18;

        if (!inlineTable.RootNode.NodeCount.TryFormat(utf8Destination.Slice(written), out var byteWritten2, format, provider))
        {
            bytesWritten = 0;
            return false;
        }
        written += byteWritten2;

        if (utf8Destination.Length - written <= 0)
        {
            bytesWritten = 0;
            return false;
        }

        utf8Destination[written++] = CsTomlSyntax.Symbol.RIGHTSQUAREBRACKET;
        bytesWritten = written;
        return true;
    }

    public override string ToString()
    {
        var length = 65536; // 64K
        using var bufferWriter = new ArrayPoolBufferWriter<char>(length);

        var conflictCount = 0;
        var charsWritten = 0;
        while (!TryFormat(bufferWriter.GetSpan(length), out charsWritten))
        {
            if (++conflictCount >= 15)
            {
                break;
            }
            length *= 2;
        }

        bufferWriter.Advance(charsWritten);
        return new string(bufferWriter.WrittenSpan);
    }

}

