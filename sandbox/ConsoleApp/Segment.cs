using System;
using System.Buffers;

namespace ConsoleApp;

public class Segment<T> : ReadOnlySequenceSegment<T>
{
    public int Length => Memory.Length;

    public Segment(ReadOnlyMemory<T> memory)
    {
        Memory = memory;
    }

    public void SetRunningIndex(long runningIndex)
    {
        RunningIndex = runningIndex;
    }

    public void SetNext(ReadOnlySequenceSegment<T>? next)
    {
        Next = next;
    }

    public Segment<T> AddNext(ReadOnlyMemory<T> memory)
    {
        var segment = new Segment<T>(memory);
        segment.SetRunningIndex(RunningIndex + Length);
        segment.SetNext(null);
        Next = segment;
        return segment;
    }
}
