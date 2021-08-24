using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Configuration;
using System;
using System.Threading.Tasks;

namespace Performance.Contracts
{
    [Service]
    public interface ISampleService
    {
        [Operation]
        ValueTask<Sample> GetSampleAsync(Identity id);
    }
}
