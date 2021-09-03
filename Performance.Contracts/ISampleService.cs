using M5.BloomFilter.Serialization;
using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Configuration;
using System;
using System.Threading.Tasks;

namespace Performance
{
    [Service("sample.v1")]
    public interface ISampleService
    {
        [Operation("getbloomfilter")]
        ValueTask<VersionedResponse<BloomFilter>> GetBloomFilterAsync(VersionInfo versionInfo, CallContext context = default);

        [Operation("getsample")]
        ValueTask<Sample[]> GetSampleAsync(Identity[] ids, CallContext context = default);

        [Operation("getsamplefromcache")]
        ValueTask<Sample[]> GetSampleFromCacheAsync(Identity[] ids, CallContext context = default);
    }
}
