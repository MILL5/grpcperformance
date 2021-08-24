using Performance.Models;
using System.Collections.Generic;

namespace Performance.Cache
{
    public static class CacheInstance
    {
        public const int CacheSize = 7000000;

        public static IDictionary<Identity, Sample> GetCache()
        {

            IDictionary<Identity, Sample> cache = new Dictionary<Identity, Sample>(CacheSize);

            for (int i = 0; i < CacheSize; i++)
            {
                var id = new Identity { Catalog = 1, ID = i };
                var sample = new Sample { ID = id, Values = new [] { new SampleData() } };
                cache.Add(id, sample);
            }

            return cache;
        }
    }
}
