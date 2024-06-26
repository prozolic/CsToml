﻿
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CsToml.Utility;

internal ref struct Utf8Writer<TBufferWriter>(ref TBufferWriter writer)
    where TBufferWriter : IBufferWriter<byte>
{
    private ref TBufferWriter bufferWriter = ref writer;
    private int writtingCount = 0;

    public readonly int WrittingCount => writtingCount;

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(byte buffer)
    {
        GetSpan(1)[0] = buffer;
        Advance(1);
    }

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length == 0) return;

        bytes.CopyTo(GetSpan(bytes.Length));
        Advance(bytes.Length);
    }

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<byte> GetSpan(int writtingLength)
    {
        return bufferWriter.GetSpan(writtingLength)[..writtingLength];
    }

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<byte> GetWriteSpan(int writtingLength)
    {
        var span = GetSpan(writtingLength);
        Advance(writtingLength);
        return span;
    }

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Advance(int count)
    {
        bufferWriter.Advance(count);
        writtingCount += count;
    }
}

