﻿using CsToml.Formatter;
using CsToml.Utility;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CsToml.Values;

[DebuggerDisplay("{Value}")]
internal sealed partial class TomlBoolean : TomlValue
{
    public static readonly TomlBoolean True = new(true);
    public static readonly TomlBoolean False = new(false);

    public bool Value { get; init; }

    public override bool HasValue => true;

    private TomlBoolean(bool value) : base()
    {
        Value = value;
    }

    internal override bool ToTomlString<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer)
    {
        FormatterCache.GetTomlValueFormatter<bool>()?.Serialize(ref writer, Value);
        return true;
    }

    public override string ToString()
        => GetString();

    public override bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
        var destinationSize = Value ? 4 : 5;
        if (destination.Length < destinationSize)
        {
            charsWritten = 0;
            return false;
        }

        GetString().TryCopyTo(destination);
        charsWritten = destinationSize;
        return true;
    }

    public override string ToString(string? format, IFormatProvider? formatProvider)
    {
        Span<char> destination = Value ? stackalloc char[4] : stackalloc char[5];
        TryFormat(destination, out var charsWritten, format.AsSpan(), formatProvider);
        return destination.ToString();
    }

    public override bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
        if (Value)
        {
            if (utf8Destination.Length < 4)
            {
                bytesWritten = 0;
                return false;
            }
            utf8Destination[0] = TomlCodes.Alphabet.T;
            utf8Destination[1] = TomlCodes.Alphabet.r;
            utf8Destination[2] = TomlCodes.Alphabet.u;
            utf8Destination[3] = TomlCodes.Alphabet.e;
            bytesWritten = 4;
        }
        else
        {
            if (utf8Destination.Length < 5)
            {
                bytesWritten = 0;
                return false;
            }
            utf8Destination[0] = TomlCodes.Alphabet.F;
            utf8Destination[1] = TomlCodes.Alphabet.a;
            utf8Destination[2] = TomlCodes.Alphabet.l;
            utf8Destination[3] = TomlCodes.Alphabet.s;
            utf8Destination[4] = TomlCodes.Alphabet.e;
            bytesWritten = 5;
        }
        return true;
    }

}

