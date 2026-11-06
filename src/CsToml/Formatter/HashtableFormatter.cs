using CsToml.Error;
using CsToml.Values;
using System.Buffers;
using System.Collections;

namespace CsToml.Formatter;

internal sealed class HashtableFormatter : ITomlValueFormatter<Hashtable>
{
    public static readonly HashtableFormatter Instance = new HashtableFormatter();

    public Hashtable Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (rootNode.NodeCount == 0)
        {
            if (rootNode.Value is TomlTable table)
            {
                var tableDocumentNode = new TomlDocumentNode(table);
                return options.Resolver.GetFormatter<Hashtable>()!.Deserialize(ref tableDocumentNode, options);
            }
            else if (rootNode.Value is TomlInlineTable inlineTable)
            {
                var tableDocumentNode = new TomlDocumentNode(inlineTable.Table);
                return options.Resolver.GetFormatter<Hashtable>()!.Deserialize(ref tableDocumentNode, options);
            }
            return new Hashtable();
        }
        else
        {
            var dictionary = new Hashtable(rootNode.NodeCount);
            var formatter = options.Resolver.GetFormatter<object>();

            foreach ((var key, var value) in rootNode.Table!.KeyValuePairs)
            {
                var keyNode = new TomlDocumentNode(key);
                var valueNode = TomlDocumentNode.FromChild(value);
                dictionary.Add(
                    formatter!.Deserialize(ref keyNode, options),
                    formatter!.Deserialize(ref valueNode, options));
            }
            return dictionary;
        }
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, Hashtable target, CsTomlSerializerOptions options)
            where TBufferWriter : IBufferWriter<byte>
    {
        if (target == null)
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(Hashtable));
            return;
        }

        writer.BeginInlineTable();
        IDictionaryEnumerator enumerator = target.GetEnumerator();
        var en = target.GetEnumerator();
        {
            if (!en.MoveNext())
            {
                writer.EndInlineTable();
                return;
            }

            var key = en.Entry.Key;
            var value = en.Entry.Value;
            if (PrimitiveObjectFormatter.TryGetJumpCode(key!.GetType(), out var jumpCode))
            {
                switch (jumpCode)
                {
                    case 0:
                        writer.WriteBoolean((bool)key);
                        break;
                    case 1:
                        writer.WriteInt64((byte)key);
                        break;
                    case 2:
                        writer.WriteInt64((sbyte)key);
                        break;
                    case 3:
                        writer.WriteInt64((char)key);
                        break;
                    case 4:
                        writer.WriteInt64((short)key);
                        break;
                    case 5:
                        writer.WriteInt64((int)key);
                        break;
                    case 6:
                        writer.WriteInt64((long)key);
                        break;
                    case 7:
                        writer.WriteInt64((ushort)key);
                        break;
                    case 8:
                        writer.WriteInt64((uint)key);
                        break;
                    case 9:
                        writer.WriteInt64(checked((long)key));
                        break;
                    case 10:
                        writer.Write(TomlCodes.Symbol.DOUBLEQUOTED);
                        writer.WriteDouble((float)key);
                        writer.Write(TomlCodes.Symbol.DOUBLEQUOTED);
                        break;
                    case 11:
                        writer.Write(TomlCodes.Symbol.DOUBLEQUOTED);
                        writer.WriteDouble((double)key);
                        writer.Write(TomlCodes.Symbol.DOUBLEQUOTED);
                        break;
                    case 12:
                        writer.WriteKeyInternal((string)key);
                        break;
                    case 13:
                        writer.Write(TomlCodes.Symbol.DOUBLEQUOTED);
                        writer.WriteDateTime((DateTime)key);
                        writer.Write(TomlCodes.Symbol.DOUBLEQUOTED);
                        break;
                    case 14:
                        writer.Write(TomlCodes.Symbol.DOUBLEQUOTED);
                        writer.WriteDateTimeOffset((DateTimeOffset)key);
                        writer.Write(TomlCodes.Symbol.DOUBLEQUOTED);
                        break;
                    case 15:
                        writer.Write(TomlCodes.Symbol.DOUBLEQUOTED);
                        writer.WriteDateOnly((DateOnly)key);

                        break;
                    case 16:
                        writer.Write(TomlCodes.Symbol.DOUBLEQUOTED);
                        writer.WriteTimeOnly((TimeOnly)key);
                        writer.Write(TomlCodes.Symbol.DOUBLEQUOTED);
                        break;
                }
            }
            else
            {
                ExceptionHelper.ThrowSerializationFailedAsKey(target.GetType());
            }
            writer.WriteEqual();
            options.Resolver.GetFormatter<object>()!.Serialize(ref writer, value!, options);

            if (!en.MoveNext())
            {
                writer.WriteSpace();
                writer.EndInlineTable();
                return;
            }

            do
            {
                writer.Write(TomlCodes.Symbol.COMMA);
                writer.WriteSpace();

                key = en.Entry.Key;
                value = en.Entry.Value;
                if (PrimitiveObjectFormatter.TryGetJumpCode(key!.GetType(), out jumpCode))
                {
                    switch (jumpCode)
                    {
                        case 0:
                            writer.WriteBoolean((bool)key);
                            break;
                        case 1:
                            writer.WriteInt64((byte)key);
                            break;
                        case 2:
                            writer.WriteInt64((sbyte)key);
                            break;
                        case 3:
                            writer.WriteInt64((char)key);
                            break;
                        case 4:
                            writer.WriteInt64((short)key);
                            break;
                        case 5:
                            writer.WriteInt64((int)key);
                            break;
                        case 6:
                            writer.WriteInt64((long)key);
                            break;
                        case 7:
                            writer.WriteInt64((ushort)key);
                            break;
                        case 8:
                            writer.WriteInt64((uint)key);
                            break;
                        case 9:
                            writer.WriteInt64(checked((long)key));
                            break;
                        case 10:
                            writer.Write(TomlCodes.Symbol.DOUBLEQUOTED);
                            writer.WriteDouble((float)key);
                            writer.Write(TomlCodes.Symbol.DOUBLEQUOTED);
                            break;
                        case 11:
                            writer.Write(TomlCodes.Symbol.DOUBLEQUOTED);
                            writer.WriteDouble((double)key);
                            writer.Write(TomlCodes.Symbol.DOUBLEQUOTED);
                            break;
                        case 12:
                            writer.WriteKeyInternal((string)key);
                            break;
                        case 13:
                            writer.Write(TomlCodes.Symbol.DOUBLEQUOTED);
                            writer.WriteDateTime((DateTime)key);
                            writer.Write(TomlCodes.Symbol.DOUBLEQUOTED);
                            break;
                        case 14:
                            writer.Write(TomlCodes.Symbol.DOUBLEQUOTED);
                            writer.WriteDateTimeOffset((DateTimeOffset)key);
                            writer.Write(TomlCodes.Symbol.DOUBLEQUOTED);
                            break;
                        case 15:
                            writer.Write(TomlCodes.Symbol.DOUBLEQUOTED);
                            writer.WriteDateOnly((DateOnly)key);

                            break;
                        case 16:
                            writer.Write(TomlCodes.Symbol.DOUBLEQUOTED);
                            writer.WriteTimeOnly((TimeOnly)key);
                            writer.Write(TomlCodes.Symbol.DOUBLEQUOTED);
                            break;
                    }
                }
                else
                {
                    ExceptionHelper.ThrowSerializationFailedAsKey(target.GetType());
                }
                writer.WriteEqual();
                options.Resolver.GetFormatter<object>()!.Serialize(ref writer, value!, options);

            } while (en.MoveNext());
        }
        writer.EndInlineTable();
    }
}
