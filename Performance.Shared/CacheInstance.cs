using M5.BloomFilter;
using System.Collections.Generic;

namespace Performance.Cache
{
    public static class CacheInstance
    {
        // We have 70 million possible entries
        public const int CacheSize = 70000000;

        public static (IDictionary<Identity, Sample> Cache, IBloomFilter Filter) GetCache()
        {
            var cache = new Dictionary<Identity, Sample>(CacheSize);

            for (int i = 0; i < CacheSize; i++)
            {
                // We only have 7 million entries, 10% of the 70 million
                if (i % 10 == 0)
                {
                    var id = new Identity { Catalog = 1, ID = i };
                    var sample = new Sample { ID = id, Values = new[] { new SampleData() } };
                    cache.Add(id, sample);
                }
            }

            var filter = FilterBuilder.Build(cache.Count, 0.01, HashMethod.Murmur3KirschMitzenmacher);

            foreach (var item in cache.Keys)
            {
                filter.Add(item);
            }

            return (cache, filter);
        }
    }
}
