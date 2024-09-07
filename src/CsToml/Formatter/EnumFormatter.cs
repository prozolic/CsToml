using CsToml.Error;
using System.Buffers;
using System.Reflection;
using System.Runtime.Serialization;

namespace CsToml.Formatter;

internal sealed class EnumFormatter<TEnum> : ITomlValueFormatter<TEnum>
    where TEnum : Enum
{
    private static readonly Dictionary<TEnum, string>? SerializedEnumTable;
    private static readonly Dictionary<string, TEnum>? DeserializedEnumTable;
    private static readonly Dictionary<TEnum, string>? SerializedEnumMemberTable;
    private static readonly Dictionary<string, TEnum>? DeserializedEnumMemberTable;

    static EnumFormatter()
    {
        SerializedEnumTable = new();
        DeserializedEnumTable = new();

        foreach(var e in typeof(TEnum).GetFields().Where(x => x.FieldType == typeof(TEnum)))
        {
            var enumValue = (TEnum)e.GetValue(null)!;
            var enumValueString = e.Name;

            SerializedEnumTable.Add(enumValue, e.Name);
            DeserializedEnumTable.Add(e.Name, enumValue);

            if (e.GetCustomAttributes().OfType<EnumMemberAttribute>().FirstOrDefault() is { Value: { } enumMember })
            {
                SerializedEnumMemberTable ??= new();
                DeserializedEnumMemberTable ??= new();
                SerializedEnumMemberTable.Add(enumValue, enumMember);
                DeserializedEnumMemberTable.Add(enumMember, enumValue);
            }
        }
    }

    public TEnum Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (!rootNode.HasValue)
        {
            return default!;
        }

        if (rootNode.TryGetString(out var value))
        {
            if (DeserializedEnumMemberTable?.TryGetValue(value, out var enumMember) ?? false)
            {
                return enumMember;
            }
            if (DeserializedEnumTable?.TryGetValue(value, out var enumValue) ?? false)
            {
                return enumValue;
            }
        }

        ExceptionHelper.ThrowDeserializationFailed(typeof(TEnum));
        return default!;
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, TEnum target, CsTomlSerializerOptions options)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (SerializedEnumMemberTable?.TryGetValue(target, out var enumMemberString) ?? false)
        {
            writer.WriteString(enumMemberString);
        }
        else if (SerializedEnumTable?.TryGetValue(target, out var enumValueString) ?? false)
        {
            writer.WriteString(enumValueString);
        }
        else
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(TEnum));
        }

    }
}
