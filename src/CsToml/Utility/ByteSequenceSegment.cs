using System.Buffers;

namespace CsToml.Utility;

internal class ByteSequenceSegment : ReadOnlySequenceSegment<byte>
{
    public int Length => Memory.Length;

    public ByteSequenceSegment(ReadOnlyMemory<byte> memory)
        => Memory = memory;

    public void SetRunningIndex(long runningIndex)
        => RunningIndex = runningIndex;

    public void SetNext(ByteSequenceSegment? next)
        => Next = next;

    public ByteSequenceSegment AddNext(ReadOnlyMemory<byte> memory)
    {
        var segment = new ByteSequenceSegment(memory);
        segment.SetRunningIndex(RunningIndex + Length);
        segment.SetNext(null);
        Next = segment;
        return segment;
    }
}
