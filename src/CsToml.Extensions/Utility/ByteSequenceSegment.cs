
using System.Buffers;

namespace CsToml.Extensions.Utility;

internal class ByteSequenceSegment : ReadOnlySequenceSegment<byte>
{
    public int Length => Memory.Length;

    public ByteSequenceSegment()
    {}

    public ByteSequenceSegment(ReadOnlyMemory<byte> memory)
        => Memory = memory;

    public void SetMemory(ReadOnlyMemory<byte> memory)
        => Memory = memory;

    public void SetRunningIndex(long runningIndex)
        => RunningIndex = runningIndex;

    public void SetNext(ByteSequenceSegment? next)
        => Next = next;

    public ByteSequenceSegment AddNext()
    {
        ByteSequenceSegment segment = new ByteSequenceSegment();
        segment.SetRunningIndex(base.RunningIndex + Length);
        segment.SetNext(null);
        Next = segment;
        return segment;
    }

    public ByteSequenceSegment AddNext(ReadOnlyMemory<byte> memory)
    {
        var segment = new ByteSequenceSegment(memory);
        segment.SetRunningIndex(RunningIndex + Length);
        segment.SetNext(null);
        Next = segment;
        return segment;
    }
}
