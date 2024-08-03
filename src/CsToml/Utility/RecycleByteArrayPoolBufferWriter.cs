
using System.Collections.Concurrent;

namespace CsToml.Utility;

internal sealed class RecycleByteArrayPoolBufferWriter
{
    private static readonly ConcurrentQueue<ArrayPoolBufferWriter<byte>> writerQueue = new();

    private RecycleByteArrayPoolBufferWriter() { }

    public static ArrayPoolBufferWriter<byte> Rent(int initialCapacity = 1024)
    {
        if (writerQueue.TryDequeue(out var buffer))
        {
            buffer.Reserve(initialCapacity);
            return buffer;
        }
        return new ArrayPoolBufferWriter<byte>(initialCapacity);
    }

    public static void Return(ArrayPoolBufferWriter<byte> buffer)
    {
        buffer.Return();
        writerQueue.Enqueue(buffer);
    }
}

