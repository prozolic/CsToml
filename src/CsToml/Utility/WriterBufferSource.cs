using CsToml.Error;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace CsToml.Utility;

internal struct WriterBufferSource
{
    private readonly IBufferWriter<byte>? bufferWriter;
    private long committed;

    public WriterBufferSource(IBufferWriter<byte> bufferWriter)
    {
        this.bufferWriter = bufferWriter;
    }

    public readonly bool IsActive => bufferWriter is not null;

    public readonly long CommittedLength => committed;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public Span<byte> Next(int pending, int sizeHint)
    {
        var bufferWriter = this.bufferWriter!;
        if (pending > 0)
        {
            committed += pending;
            bufferWriter.Advance(pending);
        }

        var span = bufferWriter.GetSpan(sizeHint);
        if (span.Length < sizeHint)
        {
            ExceptionHelper.ThrowBufferTooSmallFailed();
        }

        return span;
    }

    public void Commit(int pending)
    {
        if (pending > 0)
        {
            committed += pending;
            bufferWriter!.Advance(pending);
        }
    }
}
