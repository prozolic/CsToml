using CsToml.Error;
using CsToml.Utility;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CsToml.Formatter;

internal class BoolFormatter : ICsTomlFormatter<bool>
{
    public static void Serialize<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer, bool value)
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

    public static bool Deserialize(ref Utf8Reader reader, int length)
    {
        var bytes = reader.ReadBytes(length);

        if (bytes.Length == 4)
        {
            var trueValue = Unsafe.ReadUnaligned<int>(ref MemoryMarshal.GetReference<byte>(bytes));
            if (trueValue == 1702195828) // true
            {
                return true;
            }
        }
        if (bytes.Length == 5)
        {
            var falseValue = Unsafe.ReadUnaligned<int>(ref MemoryMarshal.GetReference<byte>(bytes));
            if (falseValue == 1936482662 // fals
                && bytes[4] == TomlCodes.Alphabet.e) // e
            {
                return false;
            }
        }

        return ExceptionHelper.NotReturnThrow<bool, string>(ExceptionHelper.ThrowDeserializationFailed, "A value that cannot be deserialized into a boolean is set.");
    }
}

