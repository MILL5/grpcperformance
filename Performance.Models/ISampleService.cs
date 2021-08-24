using Performance.Models;
using System;
using System.Threading.Tasks;

namespace Performance.Contracts
{
    public interface ISampleService
    {
        ValueTask<Sample> GetSampleAsync(Identity id);
    }
}
