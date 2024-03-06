namespace CsToml;

public readonly struct ByteArray
{
    public readonly byte[] value;

    public ByteArray(ReadOnlySpan<byte> byteSpan)
    {
        value = byteSpan.ToArray();
    }

    public ByteArray(byte[] byteSpan)
    {
        value = byteSpan;
    }

    public static implicit operator ByteArray(ReadOnlySpan<byte> bytes)
        => new(bytes);

    public static implicit operator ByteArray(byte[] bytes)
        => new(bytes);
}
