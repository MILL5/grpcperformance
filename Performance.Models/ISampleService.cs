using Performance.Models;
using System;
using System.Threading.Tasks;

namespace Performance.Models
{
    public interface ISampleService
    {
        ValueTask<Sample[]> GetSamplesAsync(Identity[] ids);
    }
}
