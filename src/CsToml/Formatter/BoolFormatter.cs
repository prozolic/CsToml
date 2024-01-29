using CsToml.Error;
using CsToml.Utility;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CsToml.Formatter;

internal class BoolFormatter : ICsTomlFormatter<bool>
{
    public static void Serialize(ref Utf8Writer writer, bool value)
    {
        var boolValueSpan = value ? "true"u8 : "false"u8;
        writer.Write(boolValueSpan);
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
                && bytes[4] == CsTomlSyntax.AlphaBet.e) // e
            {
                return false;
            }
        }

        return ExceptionHelper.NotReturnThrow<bool, string>(ExceptionHelper.ThrowDeserializationFailed, "A value that cannot be deserialized into a boolean is set.");
    }
}

