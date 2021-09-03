using M5.BloomFilter;
using M5.BloomFilter.Serialization;
using Performance;
using Performance.Shared;
using ProtoBuf.Grpc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gRPC.Performance
{
    public class SampleService : ISampleService
    {
        private static volatile VersionedCache _instance;
        private static volatile bool _isrunning;

        static SampleService()
        {
            _instance = CacheInstance.GetCache();

            _isrunning = true;
            AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;

            Task.Run(UpdateCache).ConfigureAwait(false);
        }

        private static void CurrentDomain_DomainUnload(object? sender, EventArgs e)
        {
            _isrunning = false;
        }

        // Simulate updating the cache every 5 minutes
        private static async Task UpdateCache()
        {
            const int fiveminutes = 5 * 60 * 1000;

            while (_isrunning)
            {
                try
                {
                    _instance = CacheInstance.GetCache();
                }
                catch
                {
                }

                await Task.Delay(fiveminutes);
            }
        }

        public async ValueTask<VersionedResponse<BloomFilter>> GetBloomFilterAsync(VersionInfo versionInfo, CallContext context = default)
        {
            VersionedResponse<BloomFilter> versionedResponse;

            var currentCache = _instance;

            if (versionInfo == null)
            {
                // Client asking for the first time
                versionedResponse = new VersionedResponse<BloomFilter>
                {
                    Update = VersionUpdate.Initial,
                    Version = new VersionInfo { ServerId = VersionedCache.ServerId, Version = currentCache.Version },
                    Value = new BloomFilter(currentCache.Filter)
                };
            }
            else if (versionInfo.ServerId != VersionedCache.ServerId)
            {
                // Client migrated to another server
                versionedResponse = new VersionedResponse<BloomFilter>
                {
                    Update = VersionUpdate.ServerMigration,
                    Version = new VersionInfo { ServerId = VersionedCache.ServerId, Version = currentCache.Version },
                    Value = new BloomFilter(currentCache.Filter)
                };
            }
            else if (versionInfo.Version != currentCache.Version)
            {
                // Client has a different version of the cache
                versionedResponse = new VersionedResponse<BloomFilter>
                {
                    Update = VersionUpdate.ServerMigration,
                    Version = new VersionInfo { ServerId = VersionedCache.ServerId, Version = currentCache.Version },
                    Value = new BloomFilter(currentCache.Filter)
                };
            }
            else
            {
                // No update to send to the client
                versionedResponse = new VersionedResponse<BloomFilter>
                {
                    Update = VersionUpdate.None,
                    Version = new VersionInfo { ServerId = VersionedCache.ServerId, Version = currentCache.Version }
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
