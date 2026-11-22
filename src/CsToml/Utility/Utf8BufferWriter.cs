using System.Buffers;
using System.Runtime.CompilerServices;

namespace CsToml.Utility;

internal ref struct Utf8BufferWriter(IBufferWriter<byte> bufferWriter) : IUtf8Writer
{
    private readonly IBufferWriter<byte> bufferWriter = bufferWriter;
    private long written = 0;
    private int pending = 0;

    public readonly long WrittenLength => written;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Advance(int count)
    {
        Flush();
        bufferWriter.Advance(count);
        written += count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<byte> GetSpan(int sizeHint = 0)
    {
        Flush();
        return bufferWriter.GetSpan(sizeHint);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<byte> GetWrittenSpan(int writtingLength = 1)
    {
        Flush();
        var span = bufferWriter.GetSpan(writtingLength)[..writtingLength];
        pending = writtingLength;
        written += writtingLength;
        return span;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Flush()
    {
        var pending = this.pending;
        if (pending != 0)
        {
            this.pending = 0;
            bufferWriter.Advance(pending);
        }
    }
}

internal ref struct Utf8BufferWriter<TBufferWriter>(ref TBufferWriter bufferWriter) : IUtf8Writer
    where TBufferWriter : IBufferWriter<byte>
{
    private readonly ref TBufferWriter bufferWriter = ref bufferWriter;
    private long written = 0;
    private int pending = 0;

    public readonly long WrittenLength => written;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Advance(int count)
    {
        Flush();
        bufferWriter.Advance(count);
        written += count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<byte> GetSpan(int sizeHint = 0)
    {
        Flush();
        return bufferWriter.GetSpan(sizeHint);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<byte> GetWrittenSpan(int writtingLength = 1)
    {
        Flush();
        var span = bufferWriter.GetSpan(writtingLength)[..writtingLength];
        pending = writtingLength;
        written += writtingLength;
        return span;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Flush()
    {
        var pending = this.pending;
        if (pending != 0)
        {
            this.pending = 0;
            bufferWriter.Advance(pending);
        }
    }
}
