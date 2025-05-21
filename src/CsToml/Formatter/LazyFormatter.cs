using CsToml.Error;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace CsToml.Formatter;

public sealed class LazyFormatter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T> : ITomlValueFormatter<Lazy<T>?>
{
    public static readonly LazyFormatter<T> Instance = new ();

    public Lazy<T>? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return null;
        }

        var formatter = options.Resolver.GetFormatter<T>()!;
        var value = formatter.Deserialize(ref rootNode, options);
        return new Lazy<T>(() => value);
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, Lazy<T>? target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (target != null)
        {
            var formatter = options.Resolver.GetFormatter<T>()!;
            formatter.Serialize(ref writer, target.Value, options);
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(Lazy<T>));
        }

    }
}