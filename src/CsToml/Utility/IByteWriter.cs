
namespace CsToml.Utility;

internal interface IByteWriter
{
    void Write(ReadOnlySpan<byte> bytes);

    ValueTask WriteAsync(ReadOnlyMemory<byte> bytes, bool configureAwait, CancellationToken cancellationToken);

    void Flush();
}


internal sealed class StreamByteWriter(Stream stream) : IByteWriter
{
    private readonly Stream stream = stream;

    public IByteWriter ByteWriter => this;

    void IByteWriter.Write(ReadOnlySpan<byte> bytes)
    {
        stream.Write(bytes);
    }

    async ValueTask IByteWriter.WriteAsync(ReadOnlyMemory<byte> bytes, bool configureAwait, CancellationToken cancellationToken)
    {
        await stream.WriteAsync(bytes, cancellationToken).ConfigureAwait(configureAwait);
    }

    void IByteWriter.Flush()
    {
        stream.Flush();
    }
}