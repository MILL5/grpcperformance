using System;
using M5.BloomFilter;
using Microsoft.AspNetCore.Mvc;
using Performance;
using Performance.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace REST.Performance
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class SampleController : ControllerBase, ISampleService
    {
        // We made this volatile on purpose
        private static volatile VersionedCache _instance;
        private static volatile bool _isrunning;

        static SampleController()
        {
            _instance = CacheInstance.GetCache();

            _isrunning = true;
            AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;

            Task.Run(UpdateCache).ConfigureAwait(false);
        }

        private static void CurrentDomain_DomainUnload(object sender, EventArgs e)
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

        [HttpGet("bloomfilter")]
        public async ValueTask<VersionedResponse<ReadOnlyFilter>> GetBloomFilterAsync([FromQuery] Guid? serverId, [FromQuery] int? version)
        {
            var versionInfo = VersionInfo.ToVersionInfo(serverId, version);

            VersionedResponse<ReadOnlyFilter> versionedResponse;

            var currentCache = _instance;

            if (versionInfo == null)
            {
                // Client asking for the first time
                versionedResponse = new VersionedResponse<ReadOnlyFilter>
                {
                    Update = VersionUpdate.Initial,
                    Version = new VersionInfo { ServerId = VersionedCache.ServerId, Version = currentCache.Version },
                    Value = currentCache.Filter.ToImmutable()
                };
            }
            else if (versionInfo.ServerId != VersionedCache.ServerId)
            {
                // Client migrated to another server
                versionedResponse = new VersionedResponse<ReadOnlyFilter>
                {
                    Update = VersionUpdate.ServerMigration,
                    Version = new VersionInfo { ServerId = VersionedCache.ServerId, Version = currentCache.Version },
                    Value = currentCache.Filter.ToImmutable()
                };
            }
            else if (versionInfo.Version != currentCache.Version)
            {
                // Client has a different version of the cache
                versionedResponse = new VersionedResponse<ReadOnlyFilter>
                {
                    Update = VersionUpdate.ServerMigration,
                    Version = new VersionInfo { ServerId = VersionedCache.ServerId, Version = currentCache.Version },
                    Value = currentCache.Filter.ToImmutable()
                };
            }
            else
            {
                // No update to send to the client
                versionedResponse = new VersionedResponse<ReadOnlyFilter>
                {
                    Update = VersionUpdate.None,
                    Version = new VersionInfo { ServerId = VersionedCache.ServerId, Version = currentCache.Version }
                };
            }

            return await ValueTask.FromResult(versionedResponse);
        }

        [HttpPost]
        public async ValueTask<Sample[]> GetSamplesAsync(Identity[] ids)
        {
            var samples = new List<Sample>(ids.Length);

            foreach (var id in ids)
            {
                samples.Add(Mocks.GetSample());
            }

            return await ValueTask.FromResult(samples.ToArray());
        }


        [HttpPost("fromcache")]
        public async ValueTask<Sample[]> GetSamplesFromCacheAsync(Identity[] ids)
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
