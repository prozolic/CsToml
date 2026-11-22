namespace CsToml;

public interface IUtf8Writer
{
    long WrittenLength { get; }

    void Advance(int count);

    Span<byte> GetSpan(int sizeHint = 0);

    Span<byte> GetWrittenSpan(int writtingLength = 1);
}
