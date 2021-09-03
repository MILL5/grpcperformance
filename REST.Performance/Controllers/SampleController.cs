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
        private static VersionedCache _instance;

        static SampleController()
        {
            _instance = CacheInstance.GetCache();
        }

        [HttpGet("bloomfilter")]
        public async ValueTask<VersionedResponse<ReadOnlyFilter>> GetBloomFilterAsync([FromQuery] Guid? serverId, [FromQuery] int? version)
        {
            var versionInfo = VersionInfo.ToVersionInfo(serverId, version);

            VersionedResponse<ReadOnlyFilter> versionedResponse;

            if (versionInfo == null)
            {
                // Client asking for the first time
                versionedResponse = new VersionedResponse<ReadOnlyFilter>
                {
                    Update = VersionUpdate.Initial,
                    Version = new VersionInfo { ServerId = VersionedCache.ServerId, Version = _instance.Version },
                    Value = _instance.Filter.ToImmutable()
                };
            }
            else if (versionInfo.ServerId != VersionedCache.ServerId)
            {
                // Client migrated to another server
                versionedResponse = new VersionedResponse<ReadOnlyFilter>
                {
                    Update = VersionUpdate.ServerMigration,
                    Version = new VersionInfo { ServerId = VersionedCache.ServerId, Version = _instance.Version },
                    Value = _instance.Filter.ToImmutable()
                };
            }
            else if (versionInfo.Version != _instance.Version)
            {
                // Client has a different version of the cache
                versionedResponse = new VersionedResponse<ReadOnlyFilter>
                {
                    Update = VersionUpdate.ServerMigration,
                    Version = new VersionInfo { ServerId = VersionedCache.ServerId, Version = _instance.Version },
                    Value = _instance.Filter.ToImmutable()
                };
            }
            else
            {
                // No update to send to the client
                versionedResponse = new VersionedResponse<ReadOnlyFilter>
                {
                    Update = VersionUpdate.None,
                    Version = new VersionInfo { ServerId = VersionedCache.ServerId, Version = _instance.Version }
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
