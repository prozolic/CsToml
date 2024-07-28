using System.Buffers;
using System.Runtime.InteropServices;

namespace CsToml.Extensions.Utility;

internal class ByteBufferSegmentWriter  : IBufferWriter<byte>, IDisposable
{
    private struct ByteBufferSegment
    {
        private byte[] buffer;
        private int written;

        public readonly int BufferSize => buffer.Length;

        public readonly int FreeSize => buffer.Length - written;

        public readonly int WrittenSize => written;

        public readonly ReadOnlySpan<byte> WrittenSpan => buffer.AsSpan(0, written);

        public readonly ReadOnlyMemory<byte> WrittenMemory => buffer.AsMemory(0, written);

        public ByteBufferSegment(int capacity)
        {
            buffer = ArrayPool<byte>.Shared.Rent(capacity);
            written = 0;
        }

        public readonly Span<byte> GetSpan()
            => buffer.AsSpan(written);

        public void Advance(int count)
            => written += count;

        public void Return()
        {
            ArrayPool<byte>.Shared.Return(buffer);
            written = 0;
        }
    }

    private const int DefaultBufferSize = 65536; // 64K
    private List<ByteBufferSegment> segments;
    private ByteBufferSegment currentSegment;
    private int segmentSize;
    private long written;

    public ByteBufferSegmentWriter()
    {
        segments = new List<ByteBufferSegment>();
        currentSegment = new ByteBufferSegment(DefaultBufferSize);
        segmentSize = DefaultBufferSize;
        written = 0;
    }

    public void Advance(int count)
    {
        currentSegment.Advance(count);
        written += count;
    }

    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        throw new NotSupportedException();
    }

    public Span<byte> GetSpan(int sizeHint = 0)
    {
        if (sizeHint > currentSegment.FreeSize)
        {
            segments.Add(currentSegment);

            if (sizeHint > segmentSize)
            {
                currentSegment = new ByteBufferSegment(sizeHint);
                segmentSize = currentSegment.BufferSize;
            }
            else
            {
                segmentSize = Math.Min(segmentSize * 2, Array.MaxLength);
                currentSegment = new ByteBufferSegment(segmentSize);
            }
        }

        return currentSegment.GetSpan();
    }

    public void Dispose()
        => Return();

    public void Return()
    {
        foreach (var segment in CollectionsMarshal.AsSpan(segments))
        {
            segment.Return();
        }
        segments.Clear();
        currentSegment.Return();
        written = 0;
    }

    public void WriteTo<TFileWriter>(TFileWriter writer)
        where TFileWriter : IFileWriter
    {
        foreach (var segment in CollectionsMarshal.AsSpan(segments))
        {
            if (segment.WrittenSize > 0)
            {
                writer.Write(segment.WrittenSpan);
                writer.Flush();
            }
        }

        if (currentSegment.WrittenSize > 0)
        {
            writer.Write(currentSegment.WrittenSpan);
            writer.Flush();
        }
    }

    public async ValueTask WriteToAsync<TFileWriter>(TFileWriter writer, bool configureAwait, CancellationToken cancellationToken)
        where TFileWriter : IFileWriter
    {
        cancellationToken.ThrowIfCancellationRequested();

        foreach (var segment in segments)
        {
            if (segment.WrittenSize > 0)
            {
                await writer.WriteAsync(segment.WrittenMemory, configureAwait, cancellationToken).ConfigureAwait(configureAwait);
                writer.Flush();
            }
        }

        if (currentSegment.WrittenSize > 0)
        {
            await writer.WriteAsync(currentSegment.WrittenMemory, configureAwait, cancellationToken).ConfigureAwait(configureAwait);
            writer.Flush();
        }
    }
}
