
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CsToml.Values;

internal partial class CsTomlArray
{
    internal static CsTomlArray Parse<TArrayItem>(IEnumerable<TArrayItem> value)
    {
        if (value is List<TArrayItem> list)
        {
            var listSpan = CollectionsMarshal.AsSpan(list);
            var array = new CsTomlArray(listSpan.Length);
            for (int i = 0; i < listSpan.Length; i++)
            {
                object? item = listSpan[i]!;
                if (item == null) continue;

                var itemType = item.GetType();
                if (typeof(Array).IsAssignableFrom(itemType))
                {
                    switch (itemType)
                    {
                        case var _ when typeof(IEnumerable<bool>).IsAssignableFrom(itemType):
                            array.Add(Parse(Unsafe.As<IEnumerable<bool>>(item)));
                            break;
                        case var _ when typeof(IEnumerable<byte>).IsAssignableFrom(itemType):
                            array.Add(Parse(Unsafe.As<IEnumerable<byte>>(item)));
                            break;
                        case var _ when typeof(IEnumerable<sbyte>).IsAssignableFrom(itemType):
                            array.Add(Parse(Unsafe.As<IEnumerable<sbyte>>(item)));
                            break;
                        case var _ when typeof(IEnumerable<int>).IsAssignableFrom(itemType):
                            array.Add(Parse(Unsafe.As<IEnumerable<int>>(item)));
                            break;
                        case var _ when typeof(IEnumerable<uint>).IsAssignableFrom(itemType):
                            array.Add(Parse(Unsafe.As<IEnumerable<uint>>(item)));
                            break;
                        case var _ when typeof(IEnumerable<long>).IsAssignableFrom(itemType):
                            array.Add(Parse(Unsafe.As<IEnumerable<long>>(item)));
                            break;
                        case var _ when typeof(IEnumerable<ulong>).IsAssignableFrom(itemType):
                            array.Add(Parse(Unsafe.As<IEnumerable<ulong>>(item)));
                            break;
                        case var _ when typeof(IEnumerable<short>).IsAssignableFrom(itemType):
                            array.Add(Parse(Unsafe.As<IEnumerable<short>>(item)));
                            break;
                        case var _ when typeof(IEnumerable<ushort>).IsAssignableFrom(itemType):
                            array.Add(Parse(Unsafe.As<IEnumerable<ushort>>(item)));
                            break;
                        case var _ when typeof(IEnumerable<double>).IsAssignableFrom(itemType):
                            array.Add(Parse(Unsafe.As<IEnumerable<double>>(item)));
                            break;
                        case var _ when typeof(IEnumerable<DateTime>).IsAssignableFrom(itemType):
                            array.Add(Parse(Unsafe.As<IEnumerable<DateTime>>(item)));
                            break;
                        case var _ when typeof(IEnumerable<DateTimeOffset>).IsAssignableFrom(itemType):
                            array.Add(Parse(Unsafe.As<IEnumerable<DateTimeOffset>>(item)));
                            break;
                        case var _ when typeof(IEnumerable<DateOnly>).IsAssignableFrom(itemType):
                            array.Add(Parse(Unsafe.As<IEnumerable<DateOnly>>(item)));
                            break;
                        case var _ when typeof(IEnumerable<TimeOnly>).IsAssignableFrom(itemType):
                            array.Add(Parse(Unsafe.As<IEnumerable<TimeOnly>>(item)));
                            break;
                        case var _ when typeof(IEnumerable<string>).IsAssignableFrom(itemType):
                            array.Add(Parse(Unsafe.As<IEnumerable<string>>(item)));
                            break;
                        case var _ when typeof(IEnumerable<object>).IsAssignableFrom(itemType):
                            array.Add(Parse(Unsafe.As<IEnumerable<object>>(item)));
                            break;
                    }
                }
                else
                {
                    if (CsTomlValueResolver.TryResolve(ref item, out var v))
                    {
                        array.Add(v!);
                    }
                }
            }
            return array;
        }
        else
        {
            var array = new CsTomlArray();
            foreach (var i in value)
            {
                object? item = i;
                if (item == null) continue;
                var itemType = item.GetType();
                if (typeof(Array).IsAssignableFrom(itemType))
                {
                    switch (itemType)
                    {
                        case var _ when typeof(IEnumerable<bool>).IsAssignableFrom(itemType):
                            array.Add(Parse(Unsafe.As<IEnumerable<bool>>(item)));
                            break;
                        case var _ when typeof(IEnumerable<byte>).IsAssignableFrom(itemType):
                            array.Add(Parse(Unsafe.As<IEnumerable<byte>>(item)));
                            break;
                        case var _ when typeof(IEnumerable<sbyte>).IsAssignableFrom(itemType):
                            array.Add(Parse(Unsafe.As<IEnumerable<sbyte>>(item)));
                            break;
                        case var _ when typeof(IEnumerable<int>).IsAssignableFrom(itemType):
                            array.Add(Parse(Unsafe.As<IEnumerable<int>>(item)));
                            break;
                        case var _ when typeof(IEnumerable<uint>).IsAssignableFrom(itemType):
                            array.Add(Parse(Unsafe.As<IEnumerable<uint>>(item)));
                            break;
                        case var _ when typeof(IEnumerable<long>).IsAssignableFrom(itemType):
                            array.Add(Parse(Unsafe.As<IEnumerable<long>>(item)));
                            break;
                        case var _ when typeof(IEnumerable<ulong>).IsAssignableFrom(itemType):
                            array.Add(Parse(Unsafe.As<IEnumerable<ulong>>(item)));
                            break;
                        case var _ when typeof(IEnumerable<short>).IsAssignableFrom(itemType):
                            array.Add(Parse(Unsafe.As<IEnumerable<short>>(item)));
                            break;
                        case var _ when typeof(IEnumerable<ushort>).IsAssignableFrom(itemType):
                            array.Add(Parse(Unsafe.As<IEnumerable<ushort>>(item)));
                            break;
                        case var _ when typeof(IEnumerable<double>).IsAssignableFrom(itemType):
                            array.Add(Parse(Unsafe.As<IEnumerable<double>>(item)));
                            break;
                        case var _ when typeof(IEnumerable<DateTime>).IsAssignableFrom(itemType):
                            array.Add(Parse(Unsafe.As<IEnumerable<DateTime>>(item)));
                            break;
                        case var _ when typeof(IEnumerable<DateTimeOffset>).IsAssignableFrom(itemType):
                            array.Add(Parse(Unsafe.As<IEnumerable<DateTimeOffset>>(item)));
                            break;
                        case var _ when typeof(IEnumerable<DateOnly>).IsAssignableFrom(itemType):
                            array.Add(Parse(Unsafe.As<IEnumerable<DateOnly>>(item)));
                            break;
                        case var _ when typeof(IEnumerable<TimeOnly>).IsAssignableFrom(itemType):
                            array.Add(Parse(Unsafe.As<IEnumerable<TimeOnly>>(item)));
                            break;
                        case var _ when typeof(IEnumerable<string>).IsAssignableFrom(itemType):
                            array.Add(Parse(Unsafe.As<IEnumerable<string>>(item)));
                            break;
                        case var _ when typeof(IEnumerable<object>).IsAssignableFrom(itemType):
                            array.Add(Parse(Unsafe.As<IEnumerable<object>>(item)));
                            break;
                    }
                }
                else
                {
                    if (CsTomlValueResolver.TryResolve(ref item, out var v))
                    {
                        array.Add(v!);
                    }
                }
            }
            return array;
        }
    }

    internal static CsTomlArray Parse(IEnumerable<bool> value)
    {
        if (value is List<bool> list)
        {
            var listSpan = CollectionsMarshal.AsSpan(list);
            var array = new CsTomlArray(listSpan.Length);
            for (int i = 0; i < listSpan.Length; i++)
            {
                array.Add(listSpan[i] ? CsTomlBool.True : CsTomlBool.False);
            }
            return array;
        }
        else
        {
            var array = value.TryGetNonEnumeratedCount(out var count) ? new CsTomlArray(count) : new CsTomlArray();
            foreach (var item in value)
            {
                array.Add(item ? CsTomlBool.True : CsTomlBool.False);
            }
            return array;
        }
    }

    internal static CsTomlArray Parse(IEnumerable<byte> value)
    {
        if (value is List<byte> list)
        {
            var listSpan = CollectionsMarshal.AsSpan(list);
            var array = new CsTomlArray(listSpan.Length);
            for (int i = 0; i < listSpan.Length; i++)
            {
                array.Add(CsTomlInt.Create(listSpan[i]));
            }
            return array;
        }
        else
        {
            var array = value.TryGetNonEnumeratedCount(out var count) ? new CsTomlArray(count) : new CsTomlArray();
            foreach (var item in value)
            {
                array.Add(CsTomlInt.Create(item));
            }
            return array;
        }
    }

    internal static CsTomlArray Parse(IEnumerable<sbyte> value)
    {
        if (value is List<sbyte> list)
        {
            var listSpan = CollectionsMarshal.AsSpan(list);
            var array = new CsTomlArray(listSpan.Length);
            for (int i = 0; i < listSpan.Length; i++)
            {
                array.Add(CsTomlInt.Create(listSpan[i]));
            }
            return array;
        }
        else
        {
            var array = value.TryGetNonEnumeratedCount(out var count) ? new CsTomlArray(count) : new CsTomlArray();
            foreach (var item in value)
            {
                array.Add(CsTomlInt.Create(item));
            }
            return array;
        }
    }

    internal static CsTomlArray Parse(IEnumerable<int> value)
    {
        if (value is List<int> list)
        {
            var listSpan = CollectionsMarshal.AsSpan(list);
            var array = new CsTomlArray(listSpan.Length);
            for (int i = 0; i < listSpan.Length; i++)
            {
                array.Add(CsTomlInt.Create(listSpan[i]));
            }
            return array;
        }
        else
        {
            var array = value.TryGetNonEnumeratedCount(out var count) ? new CsTomlArray(count) : new CsTomlArray();
            foreach (var item in value)
            {
                array.Add(CsTomlInt.Create(item));
            }
            return array;
        }
    }

    internal static CsTomlArray Parse(IEnumerable<uint> value)
    {
        if (value is List<uint> list)
        {
            var listSpan = CollectionsMarshal.AsSpan(list);
            var array = new CsTomlArray(listSpan.Length);
            for (int i = 0; i < listSpan.Length; i++)
            {
                array.Add(CsTomlInt.Create(listSpan[i]));
            }
            return array;
        }
        else
        {
            var array = value.TryGetNonEnumeratedCount(out var count) ? new CsTomlArray(count) : new CsTomlArray();
            foreach (var item in value)
            {
                array.Add(CsTomlInt.Create(item));
            }
            return array;
        }
    }

    internal static CsTomlArray Parse(IEnumerable<long> value)
    {
        if (value is List<long> list)
        {
            var listSpan = CollectionsMarshal.AsSpan(list);
            var array = new CsTomlArray(listSpan.Length);
            for (int i = 0; i < listSpan.Length; i++)
            {
                array.Add(CsTomlInt.Create(listSpan[i]));
            }
            return array;
        }
        else
        {
            var array = value.TryGetNonEnumeratedCount(out var count) ? new CsTomlArray(count) : new CsTomlArray();
            foreach (var item in value)
            {
                array.Add(CsTomlInt.Create(item));
            }
            return array;
        }
    }

    internal static CsTomlArray Parse(IEnumerable<ulong> value)
    {
        if (value is List<ulong> list)
        {
            var listSpan = CollectionsMarshal.AsSpan(list);
            var array = new CsTomlArray(listSpan.Length);
            for (int i = 0; i < listSpan.Length; i++)
            {
                array.Add(CsTomlInt.Create(long.CreateChecked(listSpan[i])));
            }
            return array;
        }
        else
        {
            var array = value.TryGetNonEnumeratedCount(out var count) ? new CsTomlArray(count) : new CsTomlArray();
            foreach (var item in value)
            {
                array.Add(CsTomlInt.Create(long.CreateChecked(item)));
            }
            return array;
        }
    }

    internal static CsTomlArray Parse(IEnumerable<short> value)
    {
        if (value is List<short> list)
        {
            var listSpan = CollectionsMarshal.AsSpan(list);
            var array = new CsTomlArray(listSpan.Length);
            for (int i = 0; i < listSpan.Length; i++)
            {
                array.Add(CsTomlInt.Create(listSpan[i]));
            }
            return array;
        }
        else
        {
            var array = value.TryGetNonEnumeratedCount(out var count) ? new CsTomlArray(count) : new CsTomlArray();
            foreach (var item in value)
            {
                array.Add(CsTomlInt.Create(item));
            }
            return array;
        }
    }

    internal static CsTomlArray Parse(IEnumerable<ushort> value)
    {
        if (value is List<ushort> list)
        {
            var listSpan = CollectionsMarshal.AsSpan(list);
            var array = new CsTomlArray(listSpan.Length);
            for (int i = 0; i < listSpan.Length; i++)
            {
                array.Add(CsTomlInt.Create(listSpan[i]));
            }
            return array;
        }
        else
        {
            var array = value.TryGetNonEnumeratedCount(out var count) ? new CsTomlArray(count) : new CsTomlArray();
            foreach (var item in value)
            {
                array.Add(CsTomlInt.Create(item));
            }
            return array;
        }
    }

    internal static CsTomlArray Parse(IEnumerable<double> value)
    {
        if (value is List<double> list)
        {
            var listSpan = CollectionsMarshal.AsSpan(list);
            var array = new CsTomlArray(listSpan.Length);
            for (int i = 0; i < listSpan.Length; i++)
            {
                array.Add(new CsTomlFloat(listSpan[i]));
            }
            return array;
        }
        else
        {
            var array = value.TryGetNonEnumeratedCount(out var count) ? new CsTomlArray(count) : new CsTomlArray();
            foreach (var item in value)
            {
                array.Add(new CsTomlFloat(item));
            }
            return array;
        }
    }

    internal static CsTomlArray Parse(IEnumerable<DateTime> value)
    {
        if (value is List<DateTime> list)
        {
            var listSpan = CollectionsMarshal.AsSpan(list);
            var array = new CsTomlArray(listSpan.Length);
            for (int i = 0; i < listSpan.Length; i++)
            {
                array.Add(new CsTomlLocalDateTime(listSpan[i]));
            }
            return array;
        }
        else
        {
            var array = value.TryGetNonEnumeratedCount(out var count) ? new CsTomlArray(count) : new CsTomlArray();
            foreach (var item in value)
            {
                array.Add(new CsTomlLocalDateTime(item));
            }
            return array;
        }
    }

    internal static CsTomlArray Parse(IEnumerable<DateTimeOffset> value)
    {
        if (value is List<DateTimeOffset> list)
        {
            var listSpan = CollectionsMarshal.AsSpan(list);
            var array = new CsTomlArray(listSpan.Length);
            for (int i = 0; i < listSpan.Length; i++)
            {
                array.Add(new CsTomlOffsetDateTime(listSpan[i]));
            }
            return array;
        }
        else
        {
            var array = value.TryGetNonEnumeratedCount(out var count) ? new CsTomlArray(count) : new CsTomlArray();
            foreach (var item in value)
            {
                array.Add(new CsTomlOffsetDateTime(item));
            }
            return array;
        }
    }

    internal static CsTomlArray Parse(IEnumerable<DateOnly> value)
    {
        if (value is List<DateOnly> list)
        {
            var listSpan = CollectionsMarshal.AsSpan(list);
            var array = new CsTomlArray(listSpan.Length);
            for (int i = 0; i < listSpan.Length; i++)
            {
                array.Add(new CsTomlLocalDate(listSpan[i]));
            }
            return array;
        }
        else
        {
            var array = value.TryGetNonEnumeratedCount(out var count) ? new CsTomlArray(count) : new CsTomlArray();
            foreach (var item in value)
            {
                array.Add(new CsTomlLocalDate(item));
            }
            return array;
        }
    }

    internal static CsTomlArray Parse(IEnumerable<TimeOnly> value)
    {
        if (value is List<TimeOnly> list)
        {
            var listSpan = CollectionsMarshal.AsSpan(list);
            var array = new CsTomlArray(listSpan.Length);
            for (int i = 0; i < listSpan.Length; i++)
            {
                array.Add(new CsTomlLocalTime(listSpan[i]));
            }
            return array;
        }
        else
        {
            var array = value.TryGetNonEnumeratedCount(out var count) ? new CsTomlArray(count) : new CsTomlArray();
            foreach (var item in value)
            {
                array.Add(new CsTomlLocalTime(item));
            }
            return array;
        }
    }

    internal static CsTomlArray Parse(IEnumerable<string> value)
    {
        if (value is List<string> list)
        {
            var listSpan = CollectionsMarshal.AsSpan(list);
            var array = new CsTomlArray(listSpan.Length);
            for (int i = 0; i < listSpan.Length; i++)
            {
                array.Add(CsTomlString.Parse(listSpan[i]));
            }
            return array;
        }
        else
        {
            var array = value.TryGetNonEnumeratedCount(out var count) ? new CsTomlArray(count) : new CsTomlArray();
            foreach (var item in value)
            {
                array.Add(CsTomlString.Parse(item));
            }
            return array;
        }
    }

}

