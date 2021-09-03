using System;
using System.Collections;
using System.Threading.Tasks;
using M5.BloomFilter;

namespace Performance
{
    public interface ISampleService
    {
        ValueTask<VersionedResponse<ReadOnlyFilter>> GetBloomFilterAsync(Guid? serverId, int? version);
        ValueTask<Sample[]> GetSamplesAsync(Identity[] ids);
        ValueTask<Sample[]> GetSamplesFromCacheAsync(Identity[] ids);
    }
}
