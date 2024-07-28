using Microsoft.Win32.SafeHandles;

namespace CsToml.Extensions.Utility;

internal interface IFileWriter
{
    void Write(ReadOnlySpan<byte> bytes);

    ValueTask WriteAsync(ReadOnlyMemory<byte> bytes, bool configureAwait, CancellationToken cancellationToken);

    void Flush();
}

internal class RandomAccessFileWriter(SafeFileHandle handle) : IFileWriter
{
    private SafeFileHandle handle = handle;
    private long written = 0;

    public IFileWriter FileWriter => this;

    void IFileWriter.Write(ReadOnlySpan<byte> bytes)
    {
        RandomAccess.Write(handle, bytes, written);
        written += bytes.Length;
    }

    async ValueTask IFileWriter.WriteAsync(ReadOnlyMemory<byte> bytes, bool configureAwait, CancellationToken cancellationToken)
    {
        await RandomAccess.WriteAsync(handle, bytes, written, cancellationToken).ConfigureAwait(configureAwait);
        written += bytes.Length;
    }

    void IFileWriter.Flush()
    {
        RandomAccess.FlushToDisk(handle);
    }
}