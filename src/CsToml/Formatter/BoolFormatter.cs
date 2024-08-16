using CsToml.Error;
using CsToml.Utility;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CsToml.Formatter;

internal class BoolFormatter : ITomlValueFormatter<bool>
{
    public static readonly BoolFormatter Default = new BoolFormatter();

    public void Serialize<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer, bool value)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (value)
        {
            writer.Write(TomlCodes.Alphabet.t);
            writer.Write(TomlCodes.Alphabet.r);
            writer.Write(TomlCodes.Alphabet.u);
            writer.Write(TomlCodes.Alphabet.e);
        }
        else
        {
            writer.Write(TomlCodes.Alphabet.f);
            writer.Write(TomlCodes.Alphabet.a);
            writer.Write(TomlCodes.Alphabet.l);
            writer.Write(TomlCodes.Alphabet.s);
            writer.Write(TomlCodes.Alphabet.e);
        }
    }

    public void Deserialize(ReadOnlySpan<byte> bytes, ref bool value)
    {
        if (bytes.Length == 4)
        {
            var trueValue = Unsafe.ReadUnaligned<int>(ref MemoryMarshal.GetReference<byte>(bytes));
            if (trueValue == 1702195828) // true
            {
                value = true;
                return;
            }
        }
        if (bytes.Length == 5)
        {
            var falseValue = Unsafe.ReadUnaligned<int>(ref MemoryMarshal.GetReference<byte>(bytes));
            if (falseValue == 1936482662 // fals
                && bytes[4] == TomlCodes.Alphabet.e) // e
            {
                value =  false;
                return;
            }
        }

        ExceptionHelper.ThrowDeserializationFailed("A value that cannot be deserialized into a boolean is set.");
    }
}

