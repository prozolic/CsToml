﻿using CsToml.Utility;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CsToml.Values;

[DebuggerDisplay("Inline Table = {RootNode.NodeCount}")]
internal sealed partial class TomlInlineTable : TomlValue
{
    public new static readonly TomlInlineTable Empty = new();

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly TomlTableNode node = new() { IsGroupingProperty = true, Value = TomlValue.Empty };

    public override bool HasValue => true;

    public override TomlValueType Type => TomlValueType.InlineTable;

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    internal TomlTableNode RootNode => node;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal IDictionary<object, object> GetDictionary()
        => node.GetDictionary();

    internal override void ToTomlString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer)
    {
        writer.BeginInlineTable();
        writer.WriteSpace();

        if (RootNode.NodeCount > 0)
        {
            var keys = new List<TomlDottedKey>(RootNode.NodeCount);
            ToTomlStringCore(ref writer, RootNode, keys);
        }

        writer.WriteSpace();
        writer.EndInlineTable();
    }

    private void ToTomlStringCore<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, TomlTableNode parentNode, List<TomlDottedKey> keys)
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
                    writer.Write(TomlCodes.Symbol.COMMA);
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
                        keysSpan[i].ToTomlString(ref writer);
                        writer.Write(TomlCodes.Symbol.DOT);
                    }
                }
                key.ToTomlString(ref writer);
                writer.WriteEqual();
                childNode.Value!.ToTomlString(ref writer);
                count++;

                if (count != parentNode.NodeCount)
                {
                    writer.Write(TomlCodes.Symbol.COMMA);
                    writer.WriteSpace();
                }
            }
        }

        keys.Clear(); // clear subkey
    }

    public override bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
        var tableFormat = $"TOML Inline Table[{RootNode.NodeCount}]";
        if (tableFormat.TryCopyTo(destination))
        {
            charsWritten = tableFormat.Length;
            return true;
        }
        charsWritten = 0;
        return false;
    }

    public override string ToString(string? format, IFormatProvider? formatProvider)
    {
        var length = 65536; // 64K
        var bufferWriter = RecycleArrayPoolBufferWriter<char>.Rent();
        try
        {
            var conflictCount = 0;
            var charsWritten = 0;
            while (!TryFormat(bufferWriter.GetSpan(length), out charsWritten, format, formatProvider))
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
        finally
        {
            RecycleArrayPoolBufferWriter<char>.Return(bufferWriter);
        }
    }

    public override bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
        if (!"TOML Inline Table["u8.TryCopyTo(utf8Destination))
        {
            bytesWritten = 0;
            return false;
        }
        var written = 18;

        if (!RootNode.NodeCount.TryFormat(utf8Destination.Slice(written), out var byteWritten2, format, provider))
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

        utf8Destination[written++] = TomlCodes.Symbol.RIGHTSQUAREBRACKET;
        bytesWritten = written;
        return true;
    }

    public override string ToString() => ToString(null, null);
}

