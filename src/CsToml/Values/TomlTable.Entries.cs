using CsToml.Utility;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

// -----------------------------------------------------------------------------------------------------------------------------
// The original code for the hash table part of TomlTable is from dotnet/runtime(MIT license), Please check the original license.
//
// Original source: https://github.com/dotnet/runtime
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See LICENSE.TXT: https://github.com/dotnet/runtime/blob/main/LICENSE.TXT
//
// [System.Collections.Generic.Dictionary]
// https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Collections/Generic/Dictionary.cs
//
// [System.Collections.HashHelpers]
// https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Collections/HashHelpers.cs
// -----------------------------------------------------------------------------------------------------------------------------

namespace CsToml.Values;

internal sealed partial class TomlTable
{
    private int[] buckets = [];
    private Entry[] entries = [];
    private int count;

    internal int Count => count;

    internal KeyValuePairEnumerator KeyValuePairs => new(this);

    // Largest initial capacity EnsureCapacity may allocate. Keeps a wildly overestimated
    // hint (e.g. '=' bytes inside huge strings) from over-allocating; larger tables
    // simply grow through Reserve as before.
    private const int MaxInitialCapacity = 1103;

    // Used when the estimate saturates MaxInitialCapacity: such a table will keep growing
    // past the initial allocation, so it must start on the default Reserve chain.
    // An off-chain start such as 1103 strands large tables on the 1103→2333→4861→10103 chain.
    private const int SaturatedInitialCapacity = 919;

    internal void EnsureCapacity(int estimatedCount)
    {
        if (buckets.Length != 0 || estimatedCount <= 0)
        {
            return;
        }

        var capacity = estimatedCount <= MaxInitialCapacity
            ? HashHelpers.GetPrime(estimatedCount)
            : SaturatedInitialCapacity;
        entries = new Entry[capacity];
        buckets = new int[capacity];
    }

    private int TryAdd(TomlDottedKey key, int hashCode, TomlValue value)
    {
        if (buckets.Length == 0)
        {
            var capacity = HashHelpers.Primes[0];
            this.entries = new Entry[capacity];
            this.buckets = new int[capacity];
        }

        // The chain loops index a local copy of entries under (uint)index < (uint)entries.Length,
        // which lets the JIT drop the bounds check: https://github.com/dotnet/runtime/pull/125893
        var entries = this.entries;
        ref var bucket = ref GetBucket((uint)hashCode);

        var index = bucket - 1;
        uint collisionCount = 0;
        while ((uint)index < (uint)entries.Length)
        {
            ref var e = ref entries[index];
            if (e.hashCode == hashCode && e.key.Equals(key))
                return -1;

            index = e.next;
            if ((uint)entries.Length < ++collisionCount)
                ThrowConcurrentOperation();
        }

        var currentCount = count;
        if (currentCount == entries.Length)
        {
            Reserve(HashHelpers.ExpandPrime(currentCount));
            bucket = ref GetBucket((uint)hashCode);
            entries = this.entries;
        }
        count = currentCount + 1;

        ref Entry entry = ref entries[currentCount];
        entry.hashCode = hashCode;
        entry.next = bucket - 1;
        entry.key = key;
        entry.value = value;
        bucket = currentCount + 1;

        return currentCount;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref Entry FindEntry(TomlDottedKey key, int hashCode)
    {
        var buckets = this.buckets;
        if (buckets.Length == 0)
        {
            return ref Unsafe.NullRef<Entry>();
        }

        var entries = this.entries;
        var index = buckets[(uint)hashCode % (uint)buckets.Length] - 1;
        var collisionCount = 0;
        while ((uint)index < (uint)entries.Length)
        {
            ref var entry = ref entries[index];
            if (entry.hashCode == hashCode && (ReferenceEquals(entry.key, key) || entry.key.Equals(key)))
            {
                return ref entry;
            }

            index = entry.next;
            if ((uint)entries.Length < (uint)++collisionCount)
            {
                ThrowConcurrentOperation();
            }
        }

        return ref Unsafe.NullRef<Entry>();
    }

    // The caller must have checked with FindEntry that the key does not exist.
    private void AddNew(TomlDottedKey key, int hashCode, TomlValue value)
    {
        if (buckets.Length == 0)
        {
            var capacity = HashHelpers.Primes[0];
            entries = new Entry[capacity];
            buckets = new int[capacity];
        }

        var currentCount = count;
        if (currentCount == entries.Length)
        {
            Reserve(HashHelpers.ExpandPrime(currentCount));
        }
        count = currentCount + 1;

        ref var bucket = ref GetBucket((uint)hashCode);
        ref var entry = ref entries[currentCount];
        entry.hashCode = hashCode;
        entry.next = bucket - 1;
        entry.key = key;
        entry.value = value;
        bucket = currentCount + 1;
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowConcurrentOperation()
        => throw new Exception();

    private bool TryGetValueCore(ReadOnlySpan<byte> key, int hashCode, out TomlDottedKey? dottedKey, out TomlValue? value)
    {
        var buckets = this.buckets;
        if (buckets.Length != 0)
        {
            // https://github.com/dotnet/runtime/pull/125893
            var entries = this.entries;
            var index = buckets[(uint)hashCode % (uint)buckets.Length] - 1;
            uint collisionCount = 0;
            while ((uint)index < (uint)entries.Length)
            {
                ref var e = ref entries[index];
                if (e.hashCode == hashCode && e.key.Equals(key))
                {
                    dottedKey = e.key;
                    value = e.value;
                    return true;
                }

                index = e.next;
                if ((uint)entries.Length < ++collisionCount)
                {
                    break;
                }
            }
        }

        dottedKey = null;
        value = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref int GetBucket(uint hashCode)
    {
        var buckets = this.buckets!;
        return ref buckets[hashCode % (uint)buckets.Length];
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Reserve(int capacity)
    {
        // https://github.com/dotnet/runtime/pull/132098
        Debug.Assert(HashHelpers.IsPrime(capacity));

        int count = this.count;
        Entry[] newEntries = new Entry[capacity];
        Span<Entry> newEntriesSpan = newEntries.AsSpan(0, count);
        this.entries.AsSpan().CopyTo(newEntriesSpan);

        this.buckets = new int[capacity];
        for (int i = 0; i < newEntriesSpan.Length; i++)
        {
            ref var bucket = ref GetBucket((uint)newEntriesSpan[i].hashCode);
            newEntriesSpan[i].next = bucket - 1;
            bucket = i + 1;
        }

        this.entries = newEntries;
    }

    private struct Entry
    {
        public int hashCode;
        public int next;
        public TomlDottedKey key;
        public TomlValue value;
    }

    internal struct KeyValuePairEnumerator
    {
        private readonly TomlTable? table;
        private int index;
        private KeyValuePair<TomlDottedKey, TomlValue> current;

        public readonly KeyValuePair<TomlDottedKey, TomlValue> Current => current;

        public readonly KeyValuePairEnumerator GetEnumerator() => this;

        internal KeyValuePairEnumerator(TomlTable? table)
        {
            this.table = table;
            index = 0;
            current = default;
        }

        public bool MoveNext()
        {
            if (table == null)
            {
                index = -1;
                current = default;
                return false;
            }

            if ((uint)index < (uint)table.count)
            {
                ref var entry = ref table.entries[index++];
                current = new KeyValuePair<TomlDottedKey, TomlValue>(entry.key, entry.value);
                return true;
            }

            index = table.count + 1;
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
        private const int MinPrime = 3;

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

        // IsPrime, GetPrime and GetPrimeAtLeastCore follow the prime search of
        // https://github.com/dotnet/runtime/pull/132098
        public static bool IsPrime(int candidate)
        {
            // This only tests hash table capacities, whose minimum is MinPrime, so 2 is intentionally excluded.
            Debug.Assert(candidate >= MinPrime);

            if ((candidate & 1) == 0 || (uint)candidate % MinPrime == 0)
            {
                return candidate == MinPrime;
            }

            return HasNoPrimeDivisors(candidate, (int)Math.Sqrt(candidate));
        }

        private static bool HasNoPrimeDivisors(int candidate, int limit)
        {
            // Every prime greater than 3 is 6k - 1 or 6k + 1, so test both candidates in each group.
            for (int divisor = 5; divisor <= limit; divisor += 6)
            {
                if (candidate % divisor == 0 || candidate % (divisor + 2) == 0)
                {
                    return false;
                }
            }

            return true;
        }

        public static int GetPrime(int min)
        {
            if (min <= MinPrime)
            {
                return MinPrime;
            }

            // A short linear scan is faster for the common small capacities.
            const int LinearSearchCount = 16;

            ReadOnlySpan<int> primes = Primes;
            if (min <= primes[LinearSearchCount - 1])
            {
                for (int i = 1; i < LinearSearchCount; i++)
                {
                    if (primes[i] >= min)
                    {
                        return primes[i];
                    }
                }
            }
            else
            {
                int index = primes.Slice(LinearSearchCount).BinarySearch(min);
                index = index < 0 ? ~index : index;
                index += LinearSearchCount;
                if ((uint)index < (uint)primes.Length)
                {
                    return primes[index];
                }
            }

            return GetPrimeAtLeastCore(min);
        }

        private static int GetPrimeAtLeastCore(int min)
        {
            Debug.Assert(min > MinPrime);

            int candidate = min | 1;
            uint remainder = (uint)candidate % 6;
            if (remainder == 3)
            {
                candidate += 2;
            }

            int increment = remainder == 1 ? 4 : 2;
            int limit = (int)Math.Sqrt(candidate);
            long nextLimitSquared = (long)(limit + 1) * (limit + 1);
            while (true)
            {
                while (nextLimitSquared <= candidate)
                {
                    limit++;
                    nextLimitSquared = (long)(limit + 1) * (limit + 1);
                }

                if ((uint)(candidate - 1) % HashPrime != 0 && HasNoPrimeDivisors(candidate, limit))
                {
                    return candidate;
                }

                candidate += increment;
                increment = 6 - increment;
            }
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
