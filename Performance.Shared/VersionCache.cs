using M5.BloomFilter;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Performance.Shared
{
    public class VersionedCache
    {
        public static readonly Guid ServerId = Guid.NewGuid();

        private static int _lastVersion = 0;

        public VersionedCache(IReadOnlyDictionary<Identity, Sample> cache, IBloomFilter bloomFilter)
        {
            Version = Interlocked.Increment(ref _lastVersion);
            Cache = cache;
            Filter = bloomFilter;
        }

        public int Version { get; }

        public IReadOnlyDictionary<Identity, Sample> Cache { get; }

        public IBloomFilter Filter { get; }
    }
}
