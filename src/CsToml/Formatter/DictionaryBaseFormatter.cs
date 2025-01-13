using CsToml.Error;
using CsToml.Utility;
using CsToml.Values;
using System.Buffers;

namespace CsToml.Formatter;

internal abstract class DictionaryBaseFormatter<TKey, TValue, TDicitonary, TMediator> : ITomlValueFormatter<TDicitonary>
    where TDicitonary : IEnumerable<KeyValuePair<TKey, TValue>>
{
    public TDicitonary Deserialize(ref TomlDocumentNode rootNode, CsTomlSerializerOptions options)
    {
        if (rootNode.NodeCount == 0)
        {
            if (rootNode.Value is TomlTable table)
            {
                var tableDocumentNode = new TomlDocumentNode(table.RootNode);
                return options.Resolver.GetFormatter<TDicitonary>()!.Deserialize(ref tableDocumentNode, options);
            }
            else if (rootNode.Value is TomlInlineTable inlineTable)
            {
                var tableDocumentNode = new TomlDocumentNode(inlineTable.RootNode);
                return options.Resolver.GetFormatter<TDicitonary>()!.Deserialize(ref tableDocumentNode, options);
            }
            return Complete(CreateMediator(0));
        }
        else
        {
            var dictionary = CreateMediator(rootNode.NodeCount);
            var keyFormatter = options.Resolver.GetFormatter<TKey>();

            foreach ((var key, var node) in rootNode.Node.KeyValuePairs)
            {
                if (node.Value!.HasValue)
                {
                    var keyNode = new TomlDocumentNode(key);
                    var valueNode = new TomlDocumentNode(node);
                    AddValue(dictionary,
                        keyFormatter!.Deserialize(ref keyNode, options),
                        options.Resolver.GetFormatter<TValue>()!.Deserialize(ref valueNode, options));
                }
                else
                {
                    if (rootNode.Node.TryGetChildNode(key.Value, out var childNode))
                    {
                        var keyNode = new TomlDocumentNode(key);
                        var valueNode = new TomlDocumentNode(childNode!);
                        AddValue(dictionary,
                            keyFormatter!.Deserialize(ref keyNode, options),
                            (TValue)options.Resolver.GetFormatter<IDictionary<TKey, TValue>>()!.Deserialize(ref valueNode, options));
                    }
                }
            }

            return Complete(dictionary);
        }
    }

    public void Serialize<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer, TDicitonary target, CsTomlSerializerOptions options)
            where TBufferWriter : IBufferWriter<byte>
    {
        if (target == null)
        {
            ExceptionHelper.ThrowSerializationFailed(typeof(TDicitonary));
            return;
        }

        writer.BeginInlineTable();
        var enumerator = target.GetEnumerator();
        using (IEnumerator<KeyValuePair<TKey, TValue>> en = target.GetEnumerator())
        {
            if (!en.MoveNext())
            {
                writer.EndInlineTable();
                return;
            }

            var (key, value) = en.Current;
            if (PrimitiveObjectFormatter.TryGetJumpCode(key!.GetType(), out var jumpCode))
            {
                var refKey = key;
                switch (jumpCode)
                {
                    case 0:
                        writer.WriteBoolean(UnsafeHelper.BitCast<TKey, bool>(refKey));
                        break;
                    case 1:
                        writer.WriteInt64(UnsafeHelper.BitCast<TKey, byte>(refKey));
                        break;
                    case 2:
                        writer.WriteInt64(UnsafeHelper.BitCast<TKey, sbyte>(refKey));
                        break;
                    case 3:
                        writer.WriteInt64(UnsafeHelper.BitCast<TKey, char>(refKey));
                        break;
                    case 4:
                        writer.WriteInt64(UnsafeHelper.BitCast<TKey, short>(refKey));
                        break;
                    case 5:
                        writer.WriteInt64(UnsafeHelper.BitCast<TKey, int>(refKey));
                        break;
                    case 6:
                        writer.WriteInt64(UnsafeHelper.BitCast<TKey, long>(refKey));
                        break;
                    case 7:
                        writer.WriteInt64(UnsafeHelper.BitCast<TKey, ushort>(refKey));
                        break;
                    case 8:
                        writer.WriteInt64(UnsafeHelper.BitCast<TKey, uint>(refKey));
                        break;
                    case 9:
                        writer.WriteInt64(checked(UnsafeHelper.BitCast<TKey, long>(refKey)));
                        break;
                    case 10:
                        writer.Write(TomlCodes.Symbol.DOUBLEQUOTED);
                        writer.WriteDouble(UnsafeHelper.BitCast<TKey, float>(refKey));
                        writer.Write(TomlCodes.Symbol.DOUBLEQUOTED);
                        break;
                    case 11:
                        writer.Write(TomlCodes.Symbol.DOUBLEQUOTED);
                        writer.WriteDouble(UnsafeHelper.BitCast<TKey, double>(refKey));
                        writer.Write(TomlCodes.Symbol.DOUBLEQUOTED);
                        break;
                    case 12:
                        var strKey = refKey as string;
                        TomlDottedKey.ParseKey(strKey.AsSpan()).ToTomlString(ref writer);
                        break;
                    case 13:
                        writer.Write(TomlCodes.Symbol.DOUBLEQUOTED);
                        writer.WriteDateTime(UnsafeHelper.BitCast<TKey, DateTime>(refKey));
                        writer.Write(TomlCodes.Symbol.DOUBLEQUOTED);
                        break;
                    case 14:
                        writer.Write(TomlCodes.Symbol.DOUBLEQUOTED);
                        writer.WriteDateTimeOffset(UnsafeHelper.BitCast<TKey, DateTimeOffset>(refKey));
                        writer.Write(TomlCodes.Symbol.DOUBLEQUOTED);
                        break;
                    case 15:
                        writer.Write(TomlCodes.Symbol.DOUBLEQUOTED);
                        writer.WriteDateOnly(UnsafeHelper.BitCast<TKey, DateOnly>(refKey));

                        break;
                    case 16:
                        writer.Write(TomlCodes.Symbol.DOUBLEQUOTED);
                        writer.WriteTimeOnly(UnsafeHelper.BitCast<TKey, TimeOnly>(refKey));
                        writer.Write(TomlCodes.Symbol.DOUBLEQUOTED);
                        break;
                }
            }
            else
            {
                ExceptionHelper.ThrowSerializationFailedAsKey(target.GetType());
            }
            writer.WriteEqual();
            options.Resolver.GetFormatter<TValue>()!.Serialize(ref writer, value!, options);

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

                (key, value) = en.Current;
                if (PrimitiveObjectFormatter.TryGetJumpCode(key!.GetType(), out jumpCode))
                {
                    var refKey = key;
                    switch (jumpCode)
                    {
                        case 0:
                            writer.WriteBoolean(UnsafeHelper.BitCast<TKey, bool>(refKey));
                            break;
                        case 1:
                            writer.WriteInt64(UnsafeHelper.BitCast<TKey, byte>(refKey));
                            break;
                        case 2:
                            writer.WriteInt64(UnsafeHelper.BitCast<TKey, sbyte>(refKey));
                            break;
                        case 3:
                            writer.WriteInt64(UnsafeHelper.BitCast<TKey, char>(refKey));
                            break;
                        case 4:
                            writer.WriteInt64(UnsafeHelper.BitCast<TKey, short>(refKey));
                            break;
                        case 5:
                            writer.WriteInt64(UnsafeHelper.BitCast<TKey, int>(refKey));
                            break;
                        case 6:
                            writer.WriteInt64(UnsafeHelper.BitCast<TKey, long>(refKey));
                            break;
                        case 7:
                            writer.WriteInt64(UnsafeHelper.BitCast<TKey, ushort>(refKey));
                            break;
                        case 8:
                            writer.WriteInt64(UnsafeHelper.BitCast<TKey, uint>(refKey));
                            break;
                        case 9:
                            writer.WriteInt64(checked(UnsafeHelper.BitCast<TKey, long>(refKey)));
                            break;
                        case 10:
                            writer.Write(TomlCodes.Symbol.DOUBLEQUOTED);
                            writer.WriteDouble(UnsafeHelper.BitCast<TKey, float>(refKey));
                            writer.Write(TomlCodes.Symbol.DOUBLEQUOTED);
                            break;
                        case 11:
                            writer.Write(TomlCodes.Symbol.DOUBLEQUOTED);
                            writer.WriteDouble(UnsafeHelper.BitCast<TKey, double>(refKey));
                            writer.Write(TomlCodes.Symbol.DOUBLEQUOTED);
                            break;
                        case 12:
                            var strKey = refKey as string;
                            TomlDottedKey.ParseKey(strKey.AsSpan()).ToTomlString(ref writer);
                            break;
                        case 13:
                            writer.Write(TomlCodes.Symbol.DOUBLEQUOTED);
                            writer.WriteDateTime(UnsafeHelper.BitCast<TKey, DateTime>(refKey));
                            writer.Write(TomlCodes.Symbol.DOUBLEQUOTED);
                            break;
                        case 14:
                            writer.Write(TomlCodes.Symbol.DOUBLEQUOTED);
                            writer.WriteDateTimeOffset(UnsafeHelper.BitCast<TKey, DateTimeOffset>(refKey));
                            writer.Write(TomlCodes.Symbol.DOUBLEQUOTED);
                            break;
                        case 15:
                            writer.Write(TomlCodes.Symbol.DOUBLEQUOTED);
                            writer.WriteDateOnly(UnsafeHelper.BitCast<TKey, DateOnly>(refKey));

                            break;
                        case 16:
                            writer.Write(TomlCodes.Symbol.DOUBLEQUOTED);
                            writer.WriteTimeOnly(UnsafeHelper.BitCast<TKey, TimeOnly>(refKey));
                            writer.Write(TomlCodes.Symbol.DOUBLEQUOTED);
                            break;
                    }
                }
                else
                {
                    ExceptionHelper.ThrowSerializationFailedAsKey(target.GetType());
                }
                writer.WriteEqual();
                options.Resolver.GetFormatter<TValue>()!.Serialize(ref writer, value!, options);

            } while (en.MoveNext());
        }

        writer.EndInlineTable();
    }

    protected abstract void AddValue(TMediator mediator, TKey key, TValue value);

    protected abstract TDicitonary Complete(TMediator dictionary);

    protected abstract TMediator CreateMediator(int capacity);
}

