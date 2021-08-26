using gRPC.Performance;
using M5.BloomFilter;
using Microsoft.AspNetCore.Mvc;
using Performance;
using Performance.Cache;
using System.Collections.Generic;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace REST.Performance
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class SampleController : ControllerBase, ISampleService
    {
        private static (IDictionary<Identity, Sample> Cache, IBloomFilter Filter) _instance;

        static SampleController()
        {
            _instance = CacheInstance.GetCache();
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
