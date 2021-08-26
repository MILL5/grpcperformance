using System;
using System.Threading.Tasks;

namespace Performance
{
    public interface ISampleService
    {
        ValueTask<Sample[]> GetSamplesAsync(Identity[] ids);
        ValueTask<Sample[]> GetSamplesFromCacheAsync(Identity[] ids);
    }
}
