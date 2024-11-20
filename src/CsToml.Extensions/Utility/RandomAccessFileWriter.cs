using Microsoft.Win32.SafeHandles;

namespace CsToml.Extensions.Utility;

internal interface IByteWriter
{
    void Write(ReadOnlySpan<byte> bytes);

    ValueTask WriteAsync(ReadOnlyMemory<byte> bytes, bool configureAwait, CancellationToken cancellationToken);

    void Flush();
}

internal sealed class RandomAccessFileWriter(SafeFileHandle handle) : IByteWriter
{
    private SafeFileHandle handle = handle;
    private long written = 0;

    public IByteWriter ByteWriter => this;

    void IByteWriter.Write(ReadOnlySpan<byte> bytes)
    {
        RandomAccess.Write(handle, bytes, written);
        written += bytes.Length;
    }

    async ValueTask IByteWriter.WriteAsync(ReadOnlyMemory<byte> bytes, bool configureAwait, CancellationToken cancellationToken)
    {
        await RandomAccess.WriteAsync(handle, bytes, written, cancellationToken).ConfigureAwait(configureAwait);
        written += bytes.Length;
    }

    void IByteWriter.Flush()
    {
        RandomAccess.FlushToDisk(handle);
    }
}