using BenchmarkDotNet.Attributes;
using gRPC.Performance;
using Performance.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gRPC.Benchmark
{
    public class BenchmarkTest
    {
        private static readonly Performance.Client.SampleClient grpcClient;
        private static readonly Identity[] ids;

        static BenchmarkTest()
        {
            grpcClient = new Performance.Client.SampleClient();

            var ids = new List<Identity>();
            for (int i = 0; i < Constants.BatchSize; i++)
            {
                ids.Add(new Identity());
            }

            BenchmarkTest.ids = ids.ToArray();
        }

        [Benchmark]
        public async Task GrpcTest()
        {
            var samples = await grpcClient.GetSamplesAsync(ids);
            _ = samples.Length;
        }
    }
}
