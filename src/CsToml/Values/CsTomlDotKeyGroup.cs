using CsToml.Formatter;
using CsToml.Utility;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CsToml.Values;

[DebuggerDisplay("CsTomlKey: {DotKeys}")]
internal sealed class CsTomlDotKeyGroup
{
    private readonly List<CsTomlDotKey> dotKeys;

    public IReadOnlyList<CsTomlDotKey> DotKeys => dotKeys;

    public ReadOnlySpan<CsTomlDotKey> DotKeysSpan => CollectionsMarshal.AsSpan(dotKeys);

    internal CsTomlDotKeyGroup(List<CsTomlDotKey> dotKeys)
    {
        this.dotKeys = dotKeys;
    }

    internal string GetJoinName()
    {
        var bufferWriter = new ArrayPoolBufferWriter<byte>();
        using var _ = bufferWriter;
        var writer = new Utf8Writer<ArrayPoolBufferWriter<byte>>(ref bufferWriter);
        var dotKeysSpan = DotKeysSpan;
        for (int i = 0; i < dotKeysSpan.Length; i++)
        {
            dotKeysSpan[i].ToTomlString(ref writer);
            if (i < dotKeysSpan.Length - 1)
                writer.Write(CsTomlSyntax.Symbol.PERIOD);
        }

        var tempReader = new Utf8Reader(bufferWriter.WrittenSpan);
        ValueFormatter.Deserialize(ref tempReader, tempReader.Length, out string value);
        return value;
    }
}
