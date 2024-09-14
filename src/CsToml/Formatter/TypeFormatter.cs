using CsToml.Error;
using System.Buffers;
using System.Text;

namespace CsToml.Formatter;

internal sealed class TypeFormatter : ITomlValueFormatter<Type>
{
    public static readonly TypeFormatter Instance = new TypeFormatter();

    public Type Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.TryGetString(out var value))
        {
            return (Type)Type.GetType(value, true)!;
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(Type));
        return default!;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, Type target, CsTomlSerializerOptions options) where TBufferWriter : IBufferWriter<byte>
    {
        if (target != null)
        {
            writer.WriteString(target.AssemblyQualifiedName);
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(StringBuilder));
        }
    }
}
