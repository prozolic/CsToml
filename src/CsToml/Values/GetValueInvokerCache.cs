using CsToml.Error;
using System.Collections.ObjectModel;

namespace CsToml.Values;

internal sealed class GetValueInvokerCache
{
    private class CacheCore<T>
    {
        public static Func<CsTomlValue, T>? invoker;
        public static bool isDefault;
    }

    static GetValueInvokerCache()
    {
        // default
        CacheCore<bool>.isDefault = true;
        CacheCore<bool>.invoker = (value) => value.GetBool();
        CacheCore<long>.isDefault = true;
        CacheCore<long>.invoker = (value) => value.GetInt64();
        CacheCore<double>.isDefault = true;
        CacheCore<double>.invoker = (value) => value.GetDouble();
        CacheCore<DateTime>.isDefault = true;
        CacheCore<DateTime>.invoker = (value) => value.GetDateTime();
        CacheCore<DateTimeOffset>.isDefault = true;
        CacheCore<DateTimeOffset>.invoker = (value) => value.GetDateTimeOffset();
        CacheCore<DateOnly>.isDefault = true;
        CacheCore<DateOnly>.invoker = (value) => value.GetDateOnly();
        CacheCore<TimeOnly>.isDefault = true;
        CacheCore<TimeOnly>.invoker = (value) => value.GetTimeOnly();
        CacheCore<string>.isDefault = true;
        CacheCore<string>.invoker = (value) => value.GetString();
        CacheCore<ReadOnlyCollection<CsTomlValue>>.isDefault = true;
        CacheCore<ReadOnlyCollection<CsTomlValue>>.invoker = (value) => value.GetArray();
        CacheCore<IEnumerable<CsTomlValue>>.isDefault = true;
        CacheCore<IEnumerable<CsTomlValue>>.invoker = (value) => value.GetArray();
        CacheCore<IReadOnlyCollection<CsTomlValue>>.isDefault = true;
        CacheCore<IReadOnlyCollection<CsTomlValue>>.invoker = (value) => value.GetArray();
        CacheCore<IReadOnlyList<CsTomlValue>>.isDefault = true;
        CacheCore<IReadOnlyList<CsTomlValue>>.invoker = (value) => value.GetArray();
        CacheCore<object>.isDefault = true;
        CacheCore<object>.invoker = (value) => value.GetObject();

        // primitive
        CacheCore<byte>.isDefault = true;
        CacheCore<byte>.invoker = (value) => value.GetNumber<byte>();
        CacheCore<sbyte>.isDefault = true;
        CacheCore<sbyte>.invoker = (value) => value.GetNumber<sbyte>();
        CacheCore<short>.isDefault = true;
        CacheCore<short>.invoker = (value) => value.GetNumber<short>();
        CacheCore<ushort>.isDefault = true;
        CacheCore<ushort>.invoker = (value) => value.GetNumber<ushort>();
        CacheCore<int>.isDefault = true;
        CacheCore<int>.invoker = (value) => value.GetNumber<int>();
        CacheCore<uint>.isDefault = true;
        CacheCore<uint>.invoker = (value) => value.GetNumber<uint>();
        CacheCore<ulong>.isDefault = true;
        CacheCore<ulong>.invoker = (value) => value.GetNumber<ulong>();

        // array
        CacheCore<bool[]>.isDefault = true;
        CacheCore<bool[]>.invoker = (value) =>
        {
            if (value is CsTomlArray array)
            {
                var values = new bool[array.Count];
                for (int i = 0; i < array.Count; i++)
                    values[i] = array[i].GetBool();
                return values;
            }
            return ExceptionHelper.NotReturnThrow<bool[]>(ExceptionHelper.ThrowInvalidCasting);
        };
        CacheCore<IEnumerable<bool>>.isDefault = true;
        CacheCore<IEnumerable<bool>>.invoker = CacheCore<bool[]>.invoker;
        CacheCore<IReadOnlyCollection<bool>>.isDefault = true;
        CacheCore<IReadOnlyCollection<bool>>.invoker = CacheCore<bool[]>.invoker;
        CacheCore<IReadOnlyList<bool>>.isDefault = true;
        CacheCore<IReadOnlyList<bool>>.invoker = CacheCore<bool[]>.invoker;
        CacheCore<byte[]>.isDefault = true;
        CacheCore<byte[]>.invoker = (value) =>
        {
            if (value is CsTomlArray array)
            {
                var values = new byte[array.Count];
                for (int i = 0; i < array.Count; i++)
                    values[i] = CacheCore<byte>.invoker(array[i]);
                return values;
            }
            return ExceptionHelper.NotReturnThrow<byte[]>(ExceptionHelper.ThrowInvalidCasting);
        };
        CacheCore<IEnumerable<byte>>.isDefault = true;
        CacheCore<IEnumerable<byte>>.invoker = CacheCore<byte[]>.invoker;
        CacheCore<IReadOnlyCollection<byte>>.isDefault = true;
        CacheCore<IReadOnlyCollection<byte>>.invoker = CacheCore<byte[]>.invoker;
        CacheCore<IReadOnlyList<byte>>.isDefault = true;
        CacheCore<IReadOnlyList<byte>>.invoker = CacheCore<byte[]>.invoker;
        CacheCore<sbyte[]>.isDefault = true;
        CacheCore<sbyte[]>.invoker = (value) =>
        {
            if (value is CsTomlArray array)
            {
                var values = new sbyte[array.Count];
                for (int i = 0; i < array.Count; i++)
                    values[i] = CacheCore<sbyte>.invoker(array[i]);
                return values;
            }
            return ExceptionHelper.NotReturnThrow<sbyte[]>(ExceptionHelper.ThrowInvalidCasting);
        };
        CacheCore<IEnumerable<sbyte>>.isDefault = true;
        CacheCore<IEnumerable<sbyte>>.invoker = CacheCore<sbyte[]>.invoker;
        CacheCore<IReadOnlyCollection<sbyte>>.isDefault = true;
        CacheCore<IReadOnlyCollection<sbyte>>.invoker = CacheCore<sbyte[]>.invoker;
        CacheCore<IReadOnlyList<sbyte>>.isDefault = true;
        CacheCore<IReadOnlyList<sbyte>>.invoker = CacheCore<sbyte[]>.invoker;
        CacheCore<short[]>.isDefault = true;
        CacheCore<short[]>.invoker = (value) =>
        {
            if (value is CsTomlArray array)
            {
                var values = new short[array.Count];
                for (int i = 0; i < array.Count; i++)
                    values[i] = CacheCore<short>.invoker(array[i]);
                return values;
            }
            return ExceptionHelper.NotReturnThrow<short[]>(ExceptionHelper.ThrowInvalidCasting);
        };
        CacheCore<IEnumerable<short>>.isDefault = true;
        CacheCore<IEnumerable<short>>.invoker = CacheCore<short[]>.invoker;
        CacheCore<IReadOnlyCollection<short>>.isDefault = true;
        CacheCore<IReadOnlyCollection<short>>.invoker = CacheCore<short[]>.invoker;
        CacheCore<IReadOnlyList<short>>.isDefault = true;
        CacheCore<IReadOnlyList<short>>.invoker = CacheCore<short[]>.invoker;
        CacheCore<ushort[]>.isDefault = true;
        CacheCore<ushort[]>.invoker = (value) =>
        {
            if (value is CsTomlArray array)
            {
                var values = new ushort[array.Count];
                for (int i = 0; i < array.Count; i++)
                    values[i] = CacheCore<ushort>.invoker(array[i]);
                return values;
            }
            return ExceptionHelper.NotReturnThrow<ushort[]>(ExceptionHelper.ThrowInvalidCasting);
        };
        CacheCore<IEnumerable<ushort>>.isDefault = true;
        CacheCore<IEnumerable<ushort>>.invoker = CacheCore<ushort[]>.invoker;
        CacheCore<IReadOnlyCollection<ushort>>.isDefault = true;
        CacheCore<IReadOnlyCollection<ushort>>.invoker = CacheCore<ushort[]>.invoker;
        CacheCore<IReadOnlyList<ushort>>.isDefault = true;
        CacheCore<IReadOnlyList<ushort>>.invoker = CacheCore<ushort[]>.invoker;
        CacheCore<int[]>.isDefault = true;
        CacheCore<int[]>.invoker = (value) =>
        {
            if (value is CsTomlArray array)
            {
                var values = new int[array.Count];
                for (int i = 0; i < array.Count; i++)
                    values[i] = CacheCore<int>.invoker(array[i]);
                return values;
            }
            return ExceptionHelper.NotReturnThrow<int[]>(ExceptionHelper.ThrowInvalidCasting);
        };
        CacheCore<IEnumerable<int>>.isDefault = true;
        CacheCore<IEnumerable<int>>.invoker = CacheCore<int[]>.invoker;
        CacheCore<IReadOnlyCollection<int>>.isDefault = true;
        CacheCore<IReadOnlyCollection<int>>.invoker = CacheCore<int[]>.invoker;
        CacheCore<IReadOnlyList<int>>.isDefault = true;
        CacheCore<IReadOnlyList<int>>.invoker = CacheCore<int[]>.invoker;
        CacheCore<uint[]>.isDefault = true;
        CacheCore<uint[]>.invoker = (value) =>
        {
            if (value is CsTomlArray array)
            {
                var values = new uint[array.Count];
                for (int i = 0; i < array.Count; i++)
                    values[i] = CacheCore<uint>.invoker(array[i]);
                return values;
            }
            return ExceptionHelper.NotReturnThrow<uint[]>(ExceptionHelper.ThrowInvalidCasting);
        };
        CacheCore<IEnumerable<uint>>.isDefault = true;
        CacheCore<IEnumerable<uint>>.invoker = CacheCore<uint[]>.invoker;
        CacheCore<IReadOnlyCollection<uint>>.isDefault = true;
        CacheCore<IReadOnlyCollection<uint>>.invoker = CacheCore<uint[]>.invoker;
        CacheCore<IReadOnlyList<uint>>.isDefault = true;
        CacheCore<IReadOnlyList<uint>>.invoker = CacheCore<uint[]>.invoker;
        CacheCore<long[]>.isDefault = true;
        CacheCore<long[]>.invoker = (value) =>
        {
            if (value is CsTomlArray array)
            {
                var values = new long[array.Count];
                for (int i = 0; i < array.Count; i++)
                    values[i] = array[i].GetInt64();
                return values;
            }
            return ExceptionHelper.NotReturnThrow<long[]>(ExceptionHelper.ThrowInvalidCasting);
        };
        CacheCore<IEnumerable<long>>.isDefault = true;
        CacheCore<IEnumerable<long>>.invoker = CacheCore<long[]>.invoker;
        CacheCore<IReadOnlyCollection<long>>.isDefault = true;
        CacheCore<IReadOnlyCollection<long>>.invoker = CacheCore<long[]>.invoker;
        CacheCore<IReadOnlyList<long>>.isDefault = true;
        CacheCore<IReadOnlyList<long>>.invoker = CacheCore<long[]>.invoker;
        CacheCore<ulong[]>.isDefault = true;
        CacheCore<ulong[]>.invoker = (value) =>
        {
            if (value is CsTomlArray array)
            {
                var values = new ulong[array.Count];
                for (int i = 0; i < array.Count; i++)
                    values[i] = CacheCore<ulong>.invoker(array[i]);
                return values;
            }
            return ExceptionHelper.NotReturnThrow<ulong[]>(ExceptionHelper.ThrowInvalidCasting);
        };
        CacheCore<IEnumerable<ulong>>.isDefault = true;
        CacheCore<IEnumerable<ulong>>.invoker = CacheCore<ulong[]>.invoker;
        CacheCore<IReadOnlyCollection<ulong>>.isDefault = true;
        CacheCore<IReadOnlyCollection<ulong>>.invoker = CacheCore<ulong[]>.invoker;
        CacheCore<IReadOnlyList<ulong>>.isDefault = true;
        CacheCore<IReadOnlyList<ulong>>.invoker = CacheCore<ulong[]>.invoker;
        CacheCore<double[]>.isDefault = true;
        CacheCore<double[]>.invoker = (value) =>
        {
            if (value is CsTomlArray array)
            {
                var values = new double[array.Count];
                for (int i = 0; i < array.Count; i++)
                    values[i] = array[i].GetDouble();
                return values;
            }
            return ExceptionHelper.NotReturnThrow<double[]>(ExceptionHelper.ThrowInvalidCasting);
        };
        CacheCore<IEnumerable<double>>.isDefault = true;
        CacheCore<IEnumerable<double>>.invoker = CacheCore<double[]>.invoker;
        CacheCore<IReadOnlyCollection<double>>.isDefault = true;
        CacheCore<IReadOnlyCollection<double>>.invoker = CacheCore<double[]>.invoker;
        CacheCore<IReadOnlyList<double>>.isDefault = true;
        CacheCore<IReadOnlyList<double>>.invoker = CacheCore<double[]>.invoker;
        CacheCore<DateTime[]>.isDefault = true;
        CacheCore<DateTime[]>.invoker = (value) =>
        {
            if (value is CsTomlArray array)
            {
                var values = new DateTime[array.Count];
                for (int i = 0; i < array.Count; i++)
                    values[i] = array[i].GetDateTime();
                return values;
            }
            return ExceptionHelper.NotReturnThrow<DateTime[]>(ExceptionHelper.ThrowInvalidCasting);
        };
        CacheCore<IEnumerable<DateTime>>.isDefault = true;
        CacheCore<IEnumerable<DateTime>>.invoker = CacheCore<DateTime[]>.invoker;
        CacheCore<IReadOnlyCollection<DateTime>>.isDefault = true;
        CacheCore<IReadOnlyCollection<DateTime>>.invoker = CacheCore<DateTime[]>.invoker;
        CacheCore<IReadOnlyList<DateTime>>.isDefault = true;
        CacheCore<IReadOnlyList<DateTime>>.invoker = CacheCore<DateTime[]>.invoker;
        CacheCore<DateTimeOffset[]>.isDefault = true;
        CacheCore<DateTimeOffset[]>.invoker = (value) =>
        {
            if (value is CsTomlArray array)
            {
                var values = new DateTimeOffset[array.Count];
                for (int i = 0; i < array.Count; i++)
                    values[i] = array[i].GetDateTimeOffset();
                return values;
            }
            return ExceptionHelper.NotReturnThrow<DateTimeOffset[]>(ExceptionHelper.ThrowInvalidCasting);
        };
        CacheCore<IEnumerable<DateTimeOffset>>.isDefault = true;
        CacheCore<IEnumerable<DateTimeOffset>>.invoker = CacheCore<DateTimeOffset[]>.invoker;
        CacheCore<IReadOnlyCollection<DateTimeOffset>>.isDefault = true;
        CacheCore<IReadOnlyCollection<DateTimeOffset>>.invoker = CacheCore<DateTimeOffset[]>.invoker;
        CacheCore<IReadOnlyList<DateTimeOffset>>.isDefault = true;
        CacheCore<IReadOnlyList<DateTimeOffset>>.invoker = CacheCore<DateTimeOffset[]>.invoker;
        CacheCore<DateOnly[]>.isDefault = true;
        CacheCore<DateOnly[]>.invoker = (value) =>
        {
            if (value is CsTomlArray array)
            {
                var values = new DateOnly[array.Count];
                for (int i = 0; i < array.Count; i++)
                    values[i] = array[i].GetDateOnly();
                return values;
            }
            return ExceptionHelper.NotReturnThrow<DateOnly[]>(ExceptionHelper.ThrowInvalidCasting);
        };
        CacheCore<IEnumerable<DateOnly>>.isDefault = true;
        CacheCore<IEnumerable<DateOnly>>.invoker = CacheCore<DateOnly[]>.invoker;
        CacheCore<IReadOnlyCollection<DateOnly>>.isDefault = true;
        CacheCore<IReadOnlyCollection<DateOnly>>.invoker = CacheCore<DateOnly[]>.invoker;
        CacheCore<IReadOnlyList<DateOnly>>.isDefault = true;
        CacheCore<IReadOnlyList<DateOnly>>.invoker = CacheCore<DateOnly[]>.invoker;
        CacheCore<TimeOnly[]>.isDefault = true;
        CacheCore<TimeOnly[]>.invoker = (value) =>
        {
            if (value is CsTomlArray array)
            {
                var values = new TimeOnly[array.Count];
                for (int i = 0; i < array.Count; i++)
                    values[i] = array[i].GetTimeOnly();
                return values;
            }
            return ExceptionHelper.NotReturnThrow<TimeOnly[]>(ExceptionHelper.ThrowInvalidCasting);
        };
        CacheCore<IEnumerable<TimeOnly>>.isDefault = true;
        CacheCore<IEnumerable<TimeOnly>>.invoker = CacheCore<TimeOnly[]>.invoker;
        CacheCore<IReadOnlyCollection<TimeOnly>>.isDefault = true;
        CacheCore<IReadOnlyCollection<TimeOnly>>.invoker = CacheCore<TimeOnly[]>.invoker;
        CacheCore<IReadOnlyList<TimeOnly>>.isDefault = true;
        CacheCore<IReadOnlyList<TimeOnly>>.invoker = CacheCore<TimeOnly[]>.invoker;
        CacheCore<string[]>.isDefault = true;
        CacheCore<string[]>.invoker = (value) =>
        {
            if (value is CsTomlArray array)
            {
                var values = new string[array.Count];
                for (int i = 0; i < array.Count; i++)
                    values[i] = array[i].GetString();
                return values;
            }
            return ExceptionHelper.NotReturnThrow<string[]>(ExceptionHelper.ThrowInvalidCasting);
        };
        CacheCore<IEnumerable<string>>.isDefault = true;
        CacheCore<IEnumerable<string>>.invoker = CacheCore<string[]>.invoker;
        CacheCore<IReadOnlyCollection<string>>.isDefault = true;
        CacheCore<IReadOnlyCollection<string>>.invoker = CacheCore<string[]>.invoker;
        CacheCore<IReadOnlyList<string>>.isDefault = true;
        CacheCore<IReadOnlyList<string>>.invoker = CacheCore<string[]>.invoker;
        CacheCore<object[]>.isDefault = true;
        CacheCore<object[]>.invoker = (value) =>
        {
            if (value is CsTomlArray array)
            {
                var values = new object[array.Count];
                for (int i = 0; i < array.Count; i++)
                    values[i] = array[i].GetObject();
                return values;
            }
            return ExceptionHelper.NotReturnThrow<object[]>(ExceptionHelper.ThrowInvalidCasting);
        };
        CacheCore<IEnumerable<object>>.isDefault = true;
        CacheCore<IEnumerable<object>>.invoker = CacheCore<object[]>.invoker;
        CacheCore<IReadOnlyCollection<object>>.isDefault = true;
        CacheCore<IReadOnlyCollection<object>>.invoker = CacheCore<object[]>.invoker;
        CacheCore<IReadOnlyList<object>>.isDefault = true;
        CacheCore<IReadOnlyList<object>>.invoker = CacheCore<object[]>.invoker;
    }

    public static Func<CsTomlValue, TValue>? Get<TValue>()
    {
        return CacheCore<TValue>.invoker;
    }

    public static void AddInvoker<TValue>(Func<CsTomlValue, TValue>? invoker)
    {
        if (CacheCore<TValue>.isDefault) return;

        CacheCore<TValue>.invoker = invoker;
    }
}

