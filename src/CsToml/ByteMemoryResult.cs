
using CsToml.Utility;
using System.Buffers;

namespace CsToml;

public sealed class ByteMemoryResult : IDisposable
{
    public int Length { get; }

    public bool IsRent { get; private set; }

    public IMemoryOwner<byte> Owner { get; }

    public Span<byte> ByteSpan => Owner.Memory.Span[..Length];

    public Memory<byte> ByteMemory => Owner.Memory[..Length];

    internal ByteMemoryResult(IMemoryOwner<byte> owner, int length)
    {
        Owner = owner;
        Length = length;
        IsRent = true;
    }

    internal static ByteMemoryResult Create(ArrayPoolBufferWriter<byte> bufferWriter)
    {
        var span = bufferWriter.WrittenSpan;
        var result = new ByteMemoryResult(MemoryPool<byte>.Shared.Rent(span.Length), span.Length);
        span.CopyTo(result.ByteSpan);
        return result;
    }

    public void Return()
    {
        if (IsRent)
        {
            Owner?.Dispose();
            IsRent = false;
        }
    }

    public override string ToString()
        => Utf8Helper.ToUtf16(ByteSpan);

    public void Dispose()
        => Return();
}
