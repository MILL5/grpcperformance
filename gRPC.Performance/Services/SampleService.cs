using M5.BloomFilter;
using M5.BloomFilter.Serialization;
using Performance;
using Performance.Shared;
using ProtoBuf.Grpc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gRPC.Performance
{
    public class SampleService : ISampleService
    {
        private static VersionedCache _instance;

        static SampleService()
        {
            _instance = CacheInstance.GetCache();
        }

        public async ValueTask<VersionedResponse<BloomFilter>> GetBloomFilterAsync(VersionInfo versionInfo, CallContext context = default)
        {
            VersionedResponse<BloomFilter> versionedResponse;

            if (versionInfo == null)
            {
                // Client asking for the first time
                versionedResponse = new VersionedResponse<BloomFilter>
                {
                    Update = VersionUpdate.Initial,
                    Version = new VersionInfo { ServerId = VersionedCache.ServerId, Version = _instance.Version },
                    Value = new BloomFilter(_instance.Filter)
                };
            }
            else if (versionInfo.ServerId != VersionedCache.ServerId)
            {
                // Client migrated to another server
                versionedResponse = new VersionedResponse<BloomFilter>
                {
                    Update = VersionUpdate.ServerMigration,
                    Version = new VersionInfo { ServerId = VersionedCache.ServerId, Version = _instance.Version },
                    Value = new BloomFilter(_instance.Filter)
                };
            }
            else if (versionInfo.Version != _instance.Version)
            {
                // Client has a different version of the cache
                versionedResponse = new VersionedResponse<BloomFilter>
                {
                    Update = VersionUpdate.ServerMigration,
                    Version = new VersionInfo { ServerId = VersionedCache.ServerId, Version = _instance.Version },
                    Value = new BloomFilter(_instance.Filter)
                };
            }
            else
            {
                // No update to send to the client
                versionedResponse = new VersionedResponse<BloomFilter>
                {
                    Update = VersionUpdate.None,
                    Version = new VersionInfo { ServerId = VersionedCache.ServerId, Version = _instance.Version }
                };
            }

            return await ValueTask.FromResult(versionedResponse);
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
