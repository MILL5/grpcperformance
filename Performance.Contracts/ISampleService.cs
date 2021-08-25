using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Configuration;
using System.Threading.Tasks;

namespace Performance.Contracts
{
    [Service("sample.v1")]
    public interface ISampleService
    {
        [Operation("getsample")]
        ValueTask<Sample[]> GetSampleAsync(Identity[] ids, CallContext context = default);
    }
}
