using BenchmarkDotNet.Attributes;
using System;
using System.Threading.Tasks;

namespace gRPC.Benchmark
{
    public class BenchmarkTest
    {
        private static gRPC.Performance.Client.SampleClient grpcClient;

        static BenchmarkTest()
        {
            grpcClient = new gRPC.Performance.Client.SampleClient();
        }

        [Benchmark]
        public async Task GrpcTest()
        {
            var identity = new global::Performance.Contracts.Identity();
            var sample = await grpcClient.GetSampleAsync(identity);
            _ = sample.ID;
        }
    }
}
