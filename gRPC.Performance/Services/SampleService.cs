using M5.BloomFilter;
using Performance;
using Performance.Cache;
using ProtoBuf.Grpc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gRPC.Performance
{
    public class SampleService : ISampleService
    {
        private static (IDictionary<Identity, Sample> Cache, IBloomFilter Filter) _instance;

        static SampleService()
        {
            _instance = CacheInstance.GetCache();
        }

        public async ValueTask<Sample[]> GetSampleAsync(Identity[] ids, CallContext context = default)
        {
            var samples = new List<Sample>(ids.Length);

            foreach (var id in ids)
            {
                samples.Add(Mocks.GetSample());
            }

            return await ValueTask.FromResult(samples.ToArray());
        }

        public async ValueTask<Sample[]> GetSampleFromCacheAsync(Identity[] ids, CallContext context = default)
        {
            var samples = new List<Sample>(ids.Length);

            foreach (var id in ids)
            {
                if (_instance.Cache.TryGetValue(id, out var sample))
                {
                    samples.Add(sample);
                }
            }

            return await ValueTask.FromResult(samples.ToArray());
        }
    }
}
