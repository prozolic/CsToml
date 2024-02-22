using CsToml.Utility;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


// -----------------------------------------------------------------------------------------------------------------------------
// The original code for CsTomlTableNodeDictionary is from dotnet/runtime(MIT license), Please check the original license.
//
// [dotnet/runtime]
// https://github.com/dotnet/runtime
//
// [System.Collections.Generic.Dictionary]
// https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Collections/Generic/Dictionary.cs
//
// [System.Collections.HashHelpers]
// https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Collections/HashHelpers.cs
// -----------------------------------------------------------------------------------------------------------------------------

namespace CsToml.Values.Internal;

[DebuggerDisplay("Count = {Count}")]
internal class CsTomlTableNodeDictionary
{
    private int[] buckets;
    private Entry[] entries;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private int count;

    public int Count => count;

    public CsTomlTableNodeDictionary()
    {
        buckets = [];
        entries = [];
    }

    public bool TryAdd(CsTomlString key, CsTomlTableNode value)
    {
        if (buckets.Length == 0)
            Initialize(0);

        var hashCode = key.GetHashCode();
        ref var bucket = ref GetBucket((uint)hashCode);

        var index = bucket - 1;
        var collisionCount = 0;
        while ((uint)index <= (uint)buckets.Length)
        {
            ref var e = ref entries[index];
            if (e.hashCode == hashCode && e.key.Equals(key))
                return false;

            index = e.next;
            if ((uint)buckets.Length < ++collisionCount)
                throw new Exception();
        }

        var currentCount = count;
        if (currentCount == entries.Length)
        {
            Reserve(HashHelpers.ExpandPrime(currentCount));
            bucket = ref GetBucket((uint)hashCode);
        }
        index = currentCount;
        count++;

        ref Entry entry = ref entries[index];
        entry.hashCode = hashCode;
        entry.next = bucket - 1;
        entry.key = key;
        entry.value = value;
        bucket = index + 1;

        return true;
    }

    public bool ContainsKey(CsTomlString key)
        => TryGetValue(key, out var _);

    public bool ContainsKey(ReadOnlySpan<byte> key)
        => TryGetValue(key, out var _);

    public bool TryGetValue(CsTomlString key, out CsTomlTableNode? value)
        => TryGetValueCore(key.Value, key.GetHashCode(), out value);

    public bool TryGetValue(ReadOnlySpan<byte> key, out CsTomlTableNode? value)
        => TryGetValueCore(key, ByteArrayHash.ToInt32(key), out value);

    private bool TryGetValueCore(ReadOnlySpan<byte> key, int hashCode, out CsTomlTableNode? value)
    {
        if (buckets.Length == 0)
        {
            value = null;
            return false;
        }

        ref var bucket = ref GetBucket((uint)hashCode);
        var index = bucket - 1;
        var ConflictCount = 0;
        while (true)
        {
            if ((uint)index > (uint)buckets.Length)
                break;

            ref var e = ref entries[index];
            if (e.hashCode == hashCode && e.key.Equals(key))
            {
                value = e.value;
                return true;
            }

            index = e.next;
            if (++ConflictCount > (uint)buckets.Length)
            {
                break;
            }
        }
        value = null;
        return false;
    }

    private void Initialize(int capacity)
    {
        capacity = HashHelpers.GetPrime(capacity);
        var newEntrys = new Entry[capacity];
        var newBuckets = new int[capacity];

        entries = newEntrys;
        buckets = newBuckets;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref int GetBucket(uint hashCode)
        => ref buckets[hashCode % buckets.Length];

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Reserve(int capacity)
    {
        var newEntries = new Entry[capacity];
        var byteCount = Unsafe.SizeOf<Entry>() * entries.Length;
        ref var source = ref Unsafe.As<Entry, byte>(ref MemoryMarshal.GetArrayDataReference(entries)!);
        ref var dest = ref Unsafe.As<Entry, byte>(ref MemoryMarshal.GetArrayDataReference(newEntries)!);
        Unsafe.CopyBlockUnaligned(ref dest, ref source, (uint)byteCount);

        entries = newEntries;

        buckets = new int[capacity];
        for (int i = 0; i < count; i++)
        {
            if (newEntries[i].next >= -1)
            {
                ref var bucket = ref GetBucket((uint)newEntries[i].hashCode);
                newEntries[i].next = bucket - 1;
                bucket = i + 1;
            }
        }
    }

    private struct Entry
    {
        public int hashCode;
        public int next;
        public CsTomlString key;
        public CsTomlTableNode value;
    }

    internal ref struct KeyValuePairEnumerator
    {
        private readonly CsTomlTableNodeDictionary dictionary;
        private int index;
        private KeyValuePair<CsTomlString, CsTomlTableNode> current;

        public readonly KeyValuePair<CsTomlString, CsTomlTableNode> Current => current;

        public readonly KeyValuePairEnumerator GetEnumerator() => this;

        internal KeyValuePairEnumerator(CsTomlTableNodeDictionary dict)
        {
            dictionary = dict;
            index = 0;
            current = default;
        }

        public bool MoveNext()
        {
            while ((uint)index < (uint)dictionary.Count)
            {
                ref var entry = ref dictionary.entries[index++];
                if (entry.next >= -1)
                {
                    current = new KeyValuePair<CsTomlString, CsTomlTableNode>(entry.key, entry.value);
                    return true;
                }
            }

            index = dictionary.Count + 1;
            current = default;
            return false;
        }

    }

    private static partial class HashHelpers
    {
        public const uint HashCollisionThreshold = 100;

        // This is the maximum prime smaller than Array.MaxLength.
        public const int MaxPrimeArrayLength = 0x7FFFFFC3;

        public const int HashPrime = 101;

        // Table of prime numbers to use as hash table sizes.
        // A typical resize algorithm would pick the smallest prime number in this array
        // that is larger than twice the previous capacity.
        // Suppose our Hashtable currently has capacity x and enough elements are added
        // such that a resize needs to occur. Resizing first computes 2x then finds the
        // first prime in the table greater than 2x, i.e. if primes are ordered
        // p_1, p_2, ..., p_i, ..., it finds p_n such that p_n-1 < 2x < p_n.
        // Doubling is important for preserving the asymptotic complexity of the
        // hashtable operations such as add.  Having a prime guarantees that double
        // hashing does not lead to infinite loops.  IE, your hash function will be
        // h1(key) + i*h2(key), 0 <= i < size.  h2 and the size must be relatively prime.
        // We prefer the low computation costs of higher prime numbers over the increased
        // memory allocation of a fixed prime number i.e. when right sizing a HashSet.
        internal static ReadOnlySpan<int> Primes =>
        [
            3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631, 761, 919,
            1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591,
            17519, 21023, 25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363, 156437,
            187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403, 968897, 1162687, 1395263,
            1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559, 5999471, 7199369
        ];

        public static bool IsPrime(int candidate)
        {
            if ((candidate & 1) != 0)
            {
                int limit = (int)Math.Sqrt(candidate);
                for (int divisor = 3; divisor <= limit; divisor += 2)
                {
                    if ((candidate % divisor) == 0)
                        return false;
                }
                return true;
            }
            return candidate == 2;
        }

        public static int GetPrime(int min)
        {
            foreach (int prime in Primes)
            {
                if (prime >= min)
                    return prime;
            }

            // Outside of our predefined table. Compute the hard way.
            for (int i = (min | 1); i < int.MaxValue; i += 2)
            {
                if (IsPrime(i) && ((i - 1) % HashPrime != 0))
                    return i;
            }
            return min;
        }

        public static int ExpandPrime(int oldSize)
        {
            int newSize = 2 * oldSize;

            // Allow the hashtables to grow to maximum possible size (~2G elements) before encountering capacity overflow.
            // Note that this check works even when _items.Length overflowed thanks to the (uint) cast
            if ((uint)newSize > MaxPrimeArrayLength && MaxPrimeArrayLength > oldSize)
            {
                return MaxPrimeArrayLength;
            }

            return GetPrime(newSize);
        }

    }
}
