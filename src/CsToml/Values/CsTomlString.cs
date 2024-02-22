﻿using CsToml.Formatter;
using CsToml.Utility;
using System.Buffers;
using System.Diagnostics;
using System.Text.Unicode;

namespace CsToml.Values;

[DebuggerDisplay("CsTomlString: {Utf16String}")]
internal partial class CsTomlString : 
    CsTomlValue,
    IEquatable<CsTomlString?>,
    ISpanFormattable
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly byte[] bytes;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public int Length => Value.Length;

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public string Utf16String
    {
        get
        {
            var tempReader = new Utf8Reader(Value);
            return StringFormatter.Deserialize(ref tempReader, tempReader.Length);
        }
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public ReadOnlySpan<byte> Value => bytes.AsSpan();

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public CsTomlStringType TomlStringType { get; }

    public CsTomlString(ReadOnlySpan<byte> value, CsTomlStringType type = CsTomlStringType.Basic) : base(CsTomlType.String)
    {
        bytes = value.ToArray();
        TomlStringType = type;
    }

    public CsTomlString(byte[] value, CsTomlStringType type = CsTomlStringType.Basic) : base(CsTomlType.String)
    {
        bytes = value;
        TomlStringType = type;
    }

    internal override bool ToTomlString(ref Utf8Writer writer)
    {
        if (TomlStringType == CsTomlStringType.Unquoted)
        {
            if (Length == 0)
            {
                return false;
            }
            writer.Write(Value);
            return true;
        }
        switch (TomlStringType)
        {
            case CsTomlStringType.Basic:
                return ToTomlBasicString(ref writer);
            case CsTomlStringType.MultiLineBasic:
                return ToTomlMultiLineBasicString(ref writer);
            case CsTomlStringType.Literal:
                return ToTomlLiteralString(ref writer);
            case CsTomlStringType.MultiLineLiteral:
                return ToTomlMultiLineLiteralString(ref writer);
        }

        return false;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null) return false;
        if (obj.GetType() != typeof(CsTomlString)) return false;

        return Equals((CsTomlString)obj);
    }

    public bool Equals(CsTomlString? other)
    {
        if (other == null) return false;

        return Equals(other.Value);
    }

    public bool Equals(ReadOnlySpan<byte> other)
    {
        if (Length != other.Length) return false;
        if (Length == 0) return true;

        return Value.SequenceEqual(other);
    }

    public override int GetHashCode()
        => ByteArrayHash.ToInt32(Value);

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        var status = Utf8.ToUtf16(Value, destination, out var bytesRead, out charsWritten, replaceInvalidSequences: false);
        return status != OperationStatus.Done;
    }

    public string ToString(string? format, IFormatProvider? formatProvider) => GetString();

    private bool ToTomlBasicString(ref Utf8Writer writer)
    {
        writer.Write(CsTomlSyntax.Symbol.DOUBLEQUOTED);

        var byteSpan = Value;
        for (int i = 0; i < byteSpan.Length; i++)
        {
            var ch = byteSpan[i];
            switch (ch)
            {
                case CsTomlSyntax.Symbol.DOUBLEQUOTED:
                    writer.Write(CsTomlSyntax.Symbol.BACKSLASH);
                    writer.Write(CsTomlSyntax.Symbol.DOUBLEQUOTED);
                    continue;
                case CsTomlSyntax.Symbol.BACKSLASH:
                    writer.Write(CsTomlSyntax.Symbol.BACKSLASH);
                    writer.Write(CsTomlSyntax.Symbol.BACKSLASH);
                    continue;
                case CsTomlSyntax.Symbol.BACKSPACE:
                    writer.Write(CsTomlSyntax.Symbol.BACKSLASH);
                    writer.Write(CsTomlSyntax.AlphaBet.b);
                    continue;
                case CsTomlSyntax.Symbol.TAB:
                    writer.Write(CsTomlSyntax.Symbol.BACKSLASH);
                    writer.Write(CsTomlSyntax.AlphaBet.t);
                    continue;
                case CsTomlSyntax.Symbol.LINEFEED:
                    writer.Write(CsTomlSyntax.Symbol.BACKSLASH);
                    writer.Write(CsTomlSyntax.AlphaBet.n);
                    continue;
                case CsTomlSyntax.Symbol.FORMFEED:
                    writer.Write(CsTomlSyntax.Symbol.BACKSLASH);
                    writer.Write(CsTomlSyntax.AlphaBet.f);
                    continue;
                case CsTomlSyntax.Symbol.CARRIAGE:
                    writer.Write(CsTomlSyntax.Symbol.BACKSLASH);
                    writer.Write(CsTomlSyntax.AlphaBet.r);
                    continue;
                default:
                    writer.Write(ch);
                    continue;
            }

        }

        writer.Write(CsTomlSyntax.Symbol.DOUBLEQUOTED);
        return true;
    }

    private bool ToTomlMultiLineBasicString(ref Utf8Writer writer)
    {
        writer.Write(CsTomlSyntax.Symbol.DOUBLEQUOTED);
        writer.Write(CsTomlSyntax.Symbol.DOUBLEQUOTED);
        writer.Write(CsTomlSyntax.Symbol.DOUBLEQUOTED);

        var byteSpan = Value;
        for (int i = 0; i < byteSpan.Length;i++)
        {
            var ch = byteSpan[i];
            switch (ch)
            {
                case CsTomlSyntax.Symbol.DOUBLEQUOTED:
                    writer.Write(CsTomlSyntax.Symbol.BACKSLASH);
                    writer.Write(CsTomlSyntax.Symbol.DOUBLEQUOTED);
                    continue;
                case CsTomlSyntax.Symbol.BACKSLASH:
                    writer.Write(CsTomlSyntax.Symbol.BACKSLASH);
                    writer.Write(CsTomlSyntax.Symbol.BACKSLASH);
                    continue;
                case CsTomlSyntax.Symbol.BACKSPACE:
                    writer.Write(CsTomlSyntax.Symbol.BACKSLASH);
                    writer.Write(CsTomlSyntax.AlphaBet.b);
                    continue;
                case CsTomlSyntax.Symbol.TAB:
                    writer.Write(CsTomlSyntax.Symbol.BACKSLASH);
                    writer.Write(CsTomlSyntax.AlphaBet.t);
                    continue;
                case CsTomlSyntax.Symbol.LINEFEED:
                    writer.Write(CsTomlSyntax.Symbol.BACKSLASH);
                    writer.Write(CsTomlSyntax.AlphaBet.n);
                    continue;
                case CsTomlSyntax.Symbol.FORMFEED:
                    writer.Write(CsTomlSyntax.Symbol.BACKSLASH);
                    writer.Write(CsTomlSyntax.AlphaBet.f);
                    continue;
                case CsTomlSyntax.Symbol.CARRIAGE:
                    writer.Write(CsTomlSyntax.Symbol.BACKSLASH);
                    writer.Write(CsTomlSyntax.AlphaBet.r);
                    continue;
                default:
                    writer.Write(ch);
                    continue;
            }
        }

        writer.Write(CsTomlSyntax.Symbol.DOUBLEQUOTED);
        writer.Write(CsTomlSyntax.Symbol.DOUBLEQUOTED);
        writer.Write(CsTomlSyntax.Symbol.DOUBLEQUOTED);
        return true;
    }

    private bool ToTomlLiteralString(ref Utf8Writer writer)
    {
        writer.Write(CsTomlSyntax.Symbol.SINGLEQUOTED);
        writer.Write(Value);
        writer.Write(CsTomlSyntax.Symbol.SINGLEQUOTED);
        return true;
    }

    private bool ToTomlMultiLineLiteralString(ref Utf8Writer writer)
    {
        writer.Write(CsTomlSyntax.Symbol.SINGLEQUOTED);
        writer.Write(CsTomlSyntax.Symbol.SINGLEQUOTED);
        writer.Write(CsTomlSyntax.Symbol.SINGLEQUOTED);
        writer.Write(Value);
        writer.Write(CsTomlSyntax.Symbol.SINGLEQUOTED);
        writer.Write(CsTomlSyntax.Symbol.SINGLEQUOTED);
        writer.Write(CsTomlSyntax.Symbol.SINGLEQUOTED);
        return true;
    }

    public enum CsTomlStringType : byte
    {
        Unquoted,
        Basic,
        MultiLineBasic,
        Literal,
        MultiLineLiteral
    }

}
