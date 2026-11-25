using CsToml.Error;
using CsToml.Values;

namespace CsToml.Formatter;

internal sealed class GuidFormatter : ITomlValueFormatter<Guid>
{
    public static readonly GuidFormatter Instance = new GuidFormatter();

    public Guid Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.Value is TomlString tomlString)
        {
            if (Guid.TryParse(tomlString.Utf16String.AsSpan(), out var id))
            {
                return id;
            }
        }
        ExceptionHelper.ThrowDeserializationFailed(typeof(Guid), rootNode);
        return default!;
    }

    public void Serialize(ref Utf8TomlDocumentWriter writer, Guid target, CsTomlSerializerOptions options)
    {
        writer.WriteString(target.ToString());
    }
}

internal sealed class NullableGuidFormatter : ITomlValueFormatter<Guid?>
{
    public static readonly NullableGuidFormatter Instance = new NullableGuidFormatter();

    public Guid? Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default;
        }

        if (rootNode.Value is TomlString tomlString)
        {
            if (Guid.TryParse(tomlString.Utf16String.AsSpan(), out var id))
            {
                return id;
            }
        }
        ExceptionHelper.ThrowDeserializationFailed(typeof(Guid), rootNode);
        return default;
    }

    public void Serialize(ref Utf8TomlDocumentWriter writer, Guid? target, CsTomlSerializerOptions options)
    {
        if (target.HasValue)
        {
            writer.WriteString(target.GetValueOrDefault().ToString());
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(Guid?));
        }
    }
}